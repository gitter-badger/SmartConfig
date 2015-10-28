﻿using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartConfig.Data;
using SmartConfig.Tests.TestConfigs;

namespace SmartConfig.Tests.Data
{
    [TestClass]
    public class DbSourceTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            using (var context = SmartConfigContext<TestSetting>.Create(connectionString, "TestConfig", KeyNames.From<TestSetting>()))
            {
                context.Database.Initialize(true);
            }
        }

        [TestMethod]
        public void Select_Int32Field()
        {
            var dataSource = new DbSource<TestSetting>
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString,
                SettingsTableName = "TestConfig",
                CustomKeys = new[]
                {
                    new CustomKey(KeyNames.EnvironmentKeyName, "ABC", Filters.FilterByString),
                    new CustomKey(KeyNames.VersionKeyName, "1.3.0", Filters.FilterByVersion)
                }
            };

            Assert.AreEqual("123", dataSource.Select("Int32Field"));
        }

        [TestMethod]
        public void Update_Int32Fields()
        {
            var dataSource = new DbSource<TestSetting>
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString,
                SettingsTableName = "TestConfig",
                CustomKeys = new[]
                {
                    new CustomKey(KeyNames.EnvironmentKeyName, "ABC", Filters.FilterByString),
                    new CustomKey(KeyNames.VersionKeyName, "1.3.0", Filters.FilterByVersion)
                }
            };

            Assert.AreEqual("123", dataSource.Select("Int32Field"));

            dataSource.Update("Int32Field", "456");
            Assert.AreEqual("456", dataSource.Select("Int32Field"));

            dataSource.Update("Int32Field", "789");
            Assert.AreEqual("789", dataSource.Select("Int32Field"));
        }
    }
}
