﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SmartConfig.Data;
using SmartUtilities;
using SmartUtilities.Frameworks.InlineValidation;

namespace SmartConfig.DataStores.SqlServer
{
    /// <summary>
    /// Implements sql server data source.
    /// </summary>
    public class SqlServerStore : IDataStore
    {
        public SqlServerStore(string nameOrConnectionString, Action<SettingTableConfiguration.Builder> configure = null)
        {
            nameOrConnectionString.Validate(nameof(nameOrConnectionString)).IsNotNullOrEmpty();

            ConnectionString = nameOrConnectionString;

            string connectionStringName;
            if (nameOrConnectionString.TryGetConnectionStringName(out connectionStringName))
            {
                ConnectionString = AppConfigRepository.GetConnectionString(connectionStringName);
            }

            ConnectionString.Validate(nameof(nameOrConnectionString)).IsNotNullOrEmpty();

            var settingTablePropertiesBuilder = new SettingTableConfiguration.Builder();
            configure?.Invoke(settingTablePropertiesBuilder);
            SettingTableConfiguration = settingTablePropertiesBuilder.Build();
        }

        internal AppConfigRepository AppConfigRepository { get; } = new AppConfigRepository();

        public string ConnectionString { get; }

        public SettingTableConfiguration SettingTableConfiguration { get; }

        public Type MapDataType(Type settingType) => typeof(string);

        public List<Setting> GetSettings(Setting setting)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var commandFactory = new CommandFactory(SettingTableConfiguration);
                using (var command = commandFactory.CreateSelectCommand(connection, setting))
                {
                    connection.Open();
                    command.Prepare();

                    // Read settings.
                    using (var settingReader = command.ExecuteReader())
                    {
                        var settings = new List<Setting>();
                        while (settingReader.Read())
                        {
                            var result = new Setting
                            {
                                Name = new SettingPath((string)settingReader[nameof(Setting.Name)]),
                                Value = settingReader[nameof(Setting.Value)]
                            };
                            foreach (var property in setting.Namespaces)
                            {
                                result[property.Key] = settingReader[property.Key];
                            }
                            settings.Add(result);
                        }
                        return settings;
                    }
                }
            }
        }

        public int SaveSetting(Setting setting)
        {
            return SaveSettings(new[] { setting });
        }

        public int SaveSettings(IReadOnlyCollection<Setting> settings)
        {
            if (!settings.Any())
            {
                return 0;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var commandFactory = new CommandFactory(SettingTableConfiguration);

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var rowsAffected = 0;

                        var setting0 = settings.First();

                        using (var deleteCommand = commandFactory.CreateDeleteCommand(connection, setting0))
                        {
                            deleteCommand.Transaction = transaction;
                            deleteCommand.Prepare();

                            foreach (var s in settings.Select(x => x.Name.FullName).Distinct(StringComparer.OrdinalIgnoreCase))
                            {
                                deleteCommand.Parameters[nameof(Setting.Name)].Value = s;

                                foreach (var ns in setting0.Namespaces)
                                {
                                    deleteCommand.Parameters[ns.Key].Value = ns.Value;
                                }

                                rowsAffected += deleteCommand.ExecuteNonQuery();
                            }
                        }

                        // insert new settings
                        using (var insertCommand = commandFactory.CreateInsertCommand(connection, setting0))
                        {
                            insertCommand.Transaction = transaction;
                            insertCommand.Prepare();

                            foreach (var setting in settings)
                            {
                                insertCommand.Parameters[nameof(Setting.Name)].Value = setting.Name.FullNameEx;
                                insertCommand.Parameters[nameof(Setting.Value)].Value = setting.Value;

                                foreach (var ns in setting.Namespaces)
                                {
                                    insertCommand.Parameters[ns.Key].Value = ns.Value;
                                }

                                rowsAffected += insertCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return rowsAffected;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    public static class ConfigurationBuilderExtensions
    {
        public static Configuration.Builder FromSqlServer(
            this Configuration.Builder configurationBuilder,
            string nameOrConnectionString,
            Action<SettingTableConfiguration.Builder> configure = null
        )
        {
            return configurationBuilder.From(new SqlServerStore(nameOrConnectionString, configure));
        }
    }
}

