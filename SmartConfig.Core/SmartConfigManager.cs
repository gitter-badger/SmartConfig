﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartConfig.Collections;
using SmartConfig.Converters;
using SmartConfig.Data;
using SmartUtilities;

namespace SmartConfig
{
    /// <summary>
    /// This class takes care of loading and updating config values.
    /// </summary>
    public static class SmartConfigManager
    {
        private static readonly Dictionary<Type, IDataSource> DataSources;

        /// <summary>
        /// Gets the converters collection that holds all the default converters and allows to add additional ones.
        /// </summary>
        public static ObjectConverterCollection Converters { get; private set; }

        static SmartConfigManager()
        {
            DataSources = new Dictionary<Type, IDataSource>();

            Converters = new ObjectConverterCollection
            {
                new ColorConverter(),
                new DateTimeConverter(),
                new EnumConverter(),
                new JsonConverter(),
                new StringConverter(),
                new ValueTypeConverter(),
                new XmlConverter(),
            };
        }

        #region Loading

        /// <summary>
        /// Loads a configuration from the specified data source.
        /// </summary>
        /// <param name="configType">Type that is marked with the <c>SmartCofnigAttribute</c> and specifies the configuration.</param>
        /// <param name="dataSource">Data source that provides data.</param>
        public static void Load(Type configType, IDataSource dataSource)
        {
            if (configType == null) throw new ArgumentNullException("configType", "You need to specify a config type.");
            if (dataSource == null) throw new ArgumentNullException("dataSource", "You need to specify a data source.");
            if (!configType.IsStatic()) throw new InvalidOperationException("'configType' must be a static class.");
            if (!configType.HasAttribute<SmartConfigAttribute>()) throw new SmartConfigTypeNotFoundException() { ConfigType = configType };

            Logger.LogAction(() => "Loading [$configTypeName] from [$dataSourceName]..."
                .FormatWith(new
                {
                    configTypeName = configType.Name,
                    dataSourceName = dataSource.GetType().Name
                }, true));

            DataSources[configType] = dataSource;

            var fields = GetFields(configType);

            foreach (var field in fields)
            {
                LoadValue(field);
            }
        }

        private static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.GetCustomAttribute<IgnoreAttribute>() == null);
            foreach (var field in fields)
            {
                yield return field;
            }

            var nestedTypes =
                type
                .GetNestedTypes()
                .Where(t => t.GetCustomAttribute<IgnoreAttribute>() == null)
                .SelectMany(GetFields);

            foreach (var field in nestedTypes)
            {
                yield return field;
            }
        }

        private static void LoadValue(FieldInfo field)
        {
            var configFieldInfo = ConfigFieldInfo.From(field);
            var value = GetValue(configFieldInfo);

            if (string.IsNullOrEmpty(value))
            {
                if (!field.IsOptional())
                {
                    throw new OptionalException(configFieldInfo);
                }
                return;
            }

            var converter = GetConverter(configFieldInfo);

            try
            {
                var obj = converter.DeserializeObject(value, field.FieldType, field.Contraints());
                field.SetValue(null, obj);
            }
            catch (Exception ex)
            {
                throw new ObjectConverterException(configFieldInfo, ex)
                {
                    Value = value,
                    FromType = typeof(string),
                    ToType = field.FieldType
                };
            }
        }

        // gets a value for config field and throws detailed exception if failed
        private static string GetValue(ConfigFieldInfo configFieldInfo)
        {
            try
            {
                var dataSource = DataSources[configFieldInfo.ConfigType];
                var keys = CombineKeys(dataSource, configFieldInfo);
                var value = dataSource.Select(keys);
                return value;
            }
            catch (Exception ex)
            {
                throw new DataSourceException(configFieldInfo, ex);
            }
        }

        /// <summary>
        /// Combines keys from a data source and a config.
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="configFieldInfo"></param>
        /// <returns></returns>
        private static IDictionary<string, string> CombineKeys(IDataSource dataSource, ConfigFieldInfo configFieldInfo)
        {
            // merge custom and default keys
            var keys = new Dictionary<string, string>(dataSource.Keys)
            {
                { KeyNames.DefaultKeyName, configFieldInfo.FieldPath }
            };

            // add version key if defined
            if (!string.IsNullOrEmpty(configFieldInfo.ConfigVersion))
            {
                keys.Add(KeyNames.VersionKeyName, configFieldInfo.ConfigVersion);
            }
            return keys;
        }

        #endregion

        /// <summary>
        /// Updates a configuration field.
        /// </summary>
        /// <typeparam name="TField">Type of the field.</typeparam>
        /// <param name="expression">Lambda expression of the field.</param>
        /// <param name="value">Value to be set.</param>
        public static void Update<TField>(Expression<Func<TField>> expression, TField value)
        {
            var configFieldInfo = ConfigFieldInfo.From(expression);
            var field = configFieldInfo.Field;
            field.SetValue(null, value);

            //TField data = expression.Compile()();

            if (value == null && !field.IsNullable())
            {
                throw new OptionalException(configFieldInfo);
            }

            var converter = GetConverter(configFieldInfo);

            try
            {
                var serializedValue = converter.SerializeObject(value, field.FieldType, field.GetCustomAttributes<ConstraintAttribute>(false));
                var dataSource = DataSources[configFieldInfo.ConfigType];
                var keys = CombineKeys(dataSource, configFieldInfo);
                dataSource.Update(keys, serializedValue);
            }
            catch (ConstraintException<ConstraintAttribute>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ObjectConverterException(configFieldInfo, ex)
                {
                    Value = value,
                    FromType = field.FieldType,
                    ToType = typeof(string),
                };
            }
        }

        private static ObjectConverterBase GetConverter(ConfigFieldInfo configFieldInfo)
        {
            var type = GetConverterType(configFieldInfo.Field);

            var objectConverter = Converters[type];
            if (objectConverter == null)
            {
                throw new ObjectConverterNotFoundException(configFieldInfo, type);
            }

            return objectConverter;
        }

        private static Type GetConverterType(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;

            if (type.BaseType == typeof(Enum))
            {
                return typeof(Enum);
            }

            var objectConverterAttribute = fieldInfo.GetCustomAttribute<ObjectConverterAttribute>(false);
            if (objectConverterAttribute != null)
            {
                return objectConverterAttribute.Type;
            }

            return type;
        }

    }
}