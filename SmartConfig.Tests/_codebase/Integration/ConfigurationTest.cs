﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable;
using Reusable.Converters;
using Reusable.Data.Annotations;
using SmartConfig;
using SmartConfig.Data.Annotations;
using SmartConfig.DataStores;

namespace SmartConfig.Core.Tests.Integration
{
    // ReSharper disable BuiltInTypeReferenceStyle
    // ReSharper disable InconsistentNaming
    // ReSharper disable CheckNamespace

    using ConfigurationTestConfigs;
    using Reusable.Fuse;
    using Reusable.Fuse.Testing;

    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void Load_EmptyConfig()
        {
            var testStore = new TestStore();
            var configuration = Configuration.Load.From(new TestStore()).Select(typeof(EmptyConfig));

            configuration.Type.Verify().IsTrue(x => x == typeof(EmptyConfig));
            configuration.SettingProperties.Count().Verify().IsEqual(0);

            testStore.GetSettingsParameters.Count.Verify().IsEqual(0);
            testStore.SaveSettingsParameters.Count.Verify().IsEqual(0);
        }

        [TestMethod]
        public void Load_FullConfig()
        {
            var culture = CultureInfo.InvariantCulture;

            var config = Configuration.Load.From(new MemoryStore
            {
                {nameof(FullConfig.SByte), SByte.MaxValue.ToString()},
                {nameof(FullConfig.Byte), Byte.MaxValue.ToString()},
                {nameof(FullConfig.Char), Char.MaxValue.ToString()},
                {nameof(FullConfig.Int16), Int16.MaxValue.ToString()},
                {nameof(FullConfig.Int32), Int32.MaxValue.ToString()},
                {nameof(FullConfig.Int64), Int64.MaxValue.ToString()},
                {nameof(FullConfig.UInt16), UInt16.MaxValue.ToString()},
                {nameof(FullConfig.UInt32), UInt32.MaxValue.ToString()},
                {nameof(FullConfig.UInt64), UInt64.MaxValue.ToString()},
                {nameof(FullConfig.Single), Single.MaxValue.ToString("R", culture)},
                {nameof(FullConfig.Double), Double.MaxValue.ToString("R", culture)},
                {nameof(FullConfig.Decimal), Decimal.MaxValue.ToString(culture)},

                {nameof(FullConfig.String), "foo"},
                {nameof(FullConfig.False), bool.FalseString},
                {nameof(FullConfig.True), bool.TrueString},
                {nameof(FullConfig.DateTime), new DateTime(2016, 7, 30).ToString(culture)},
                {nameof(FullConfig.Enum), TestEnum.TestValue2.ToString()},

                {nameof(FullConfig.ColorName), Color.DarkRed.Name},
                {nameof(FullConfig.ColorDec), $"{Color.Plum.R},{Color.Plum.G},{Color.Plum.B}"},
                {nameof(FullConfig.ColorHex), Color.Beige.ToArgb().ToString("X")},

                {nameof(FullConfig.JsonArray), "[5, 8, 13]"},

                //{ nameof(FullConfig.Optional), "Fox" },

                {nameof(FullConfig.ItemizedArray) + "[0]", "5"},
                {nameof(FullConfig.ItemizedArray) + "[1]", "8"},

                {nameof(FullConfig.ItemizedDictionary) + "[foo]", "21"},
                {nameof(FullConfig.ItemizedDictionary) + "[bar]", "34"},

                {nameof(FullConfig.NestedConfig) + "." + nameof(FullConfig.NestedConfig.NestedString), "Quux"},

                //new Setting { Name = nameof(TypesConfig.Uri), Value = bool.TrueString },
                //new Setting { Name = nameof(TypesConfig.XDocument), Value = @"<?xml version=""1.0""?><testXml></testXml>" },
                //new Setting { Name = nameof(TypesConfig.XElement), Value = @"<testXml></testXml>" },
            })
            //.With<JsonToObjectConverter<List<Int32>>>()
            .Select(typeof(FullConfig));

            config.SettingProperties.Count().Verify().IsEqual(25);

            FullConfig.SByte.Verify().IsEqual(SByte.MaxValue);
            FullConfig.Byte.Verify().IsEqual(Byte.MaxValue);
            FullConfig.Char.Verify().IsEqual(Char.MaxValue);
            FullConfig.Int16.Verify().IsEqual(Int16.MaxValue);
            FullConfig.Int32.Verify().IsEqual(Int32.MaxValue);
            FullConfig.Int64.Verify().IsEqual(Int64.MaxValue);
            FullConfig.UInt16.Verify().IsEqual(UInt16.MaxValue);
            FullConfig.UInt32.Verify().IsEqual(UInt32.MaxValue);
            FullConfig.UInt64.Verify().IsEqual(UInt64.MaxValue);
            FullConfig.Single.Verify().IsEqual(Single.MaxValue);
            FullConfig.Double.Verify().IsEqual(Double.MaxValue);
            FullConfig.Decimal.Verify().IsEqual(Decimal.MaxValue);

            FullConfig.String.Verify().IsEqual("foo");
            FullConfig.False.Verify().IsEqual(false);
            FullConfig.True.Verify().IsEqual(true);
            FullConfig.DateTime.Verify().IsEqual(new DateTime(2016, 7, 30));
            FullConfig.Enum.Verify().IsTrue(x => x == TestEnum.TestValue2);

            FullConfig.ColorName.ToArgb().Verify().IsEqual(Color.DarkRed.ToArgb());
            FullConfig.ColorDec.ToArgb().Verify().IsEqual(Color.Plum.ToArgb());
            FullConfig.ColorHex.ToArgb().Verify().IsEqual(Color.Beige.ToArgb());

            FullConfig.JsonArray.Verify().IsTrue(x => x.SequenceEqual(new List<int> { 5, 8, 13 }));

            FullConfig.Optional.Verify().IsEqual("Waldo");

            FullConfig.ItemizedArray.Length.Verify().IsEqual(2);
            FullConfig.ItemizedArray[0].Verify().IsEqual(5);
            FullConfig.ItemizedArray[1].Verify().IsEqual(8);

            FullConfig.ItemizedDictionary.Count.Verify().IsEqual(2);
            FullConfig.ItemizedDictionary["foo"].Verify().IsEqual(21);
            FullConfig.ItemizedDictionary["bar"].Verify().IsEqual(34);

            FullConfig.NestedConfig.NestedString.Verify().IsEqual("Quux");
        }


        [TestMethod]
        public void Load_Where_FromExpression()
        {
            var config = Configuration.Load
                .From(new MemoryStore())
                .Where(() => TestConfig.Foo)
                .Select(typeof(TestConfig));

            config.Attributes["Foo"].Verify().IsTrue(x => x.ToString() == "Bar");
        }

        // Test invalid usage errors

        [TestMethod]
        public void Load_NonStaticConfigType_Throws_ValidationException()
        {
            new Action(() =>
            {
                Configuration.Load.From(new MemoryStore()).Select(typeof(NonStaticConfig));
            })
            .Verify()
            .Throws<ValidationException>(ex =>
            {
                // Config type "SmartConfig.Core.Tests.Integration.ConfigurationTestConfigs.NonStaticConfig" must be static.
                /*
#2
ConfigurationLoadException: Could not load "TestConfig" from "SqlServerStore".
- ConfigType: SmartConfig.DataStores.SqlServerStore.Tests.TestConfig
- DataSourceType: SmartConfig.DataStores.SqlServerStore
- Data:[]

#1
SettingNotFoundException: Setting "TestSetting" not found. You need to provide a value for it or decorate it with the "OptionalAttribute".
- WeakFullName: TestSetting
                 
                 */
                Debug.Write(ex.Message);                    
            });
        }

        [TestMethod]
        public void Load_ValueNull()
        {
            new Action(() => Configuration.Load.From(new MemoryStore()).Where("foo", null)).Verify().Throws<ValidationException>();
        }

        [TestMethod]
        public void Load_Throws_SettingNotFoundException()
        {
            new Action(() =>
            {
                Configuration.Load.From(new TestStore()).Select(typeof(SettingNotFoundConfig));
            })
            .Verify().Throws<ConfigurationLoadException>(exception =>
            {
                exception.InnerException.Verify().IsInstanceOfType(typeof(AggregateException));
                (exception.InnerException as AggregateException).InnerExceptions.OfType<SettingNotFoundException>()
                    .Count()
                    .Verify()
                    .IsEqual(1);
            });
        }

        [TestMethod]
        public void Load_ConfigNotDecorated()
        {
            new Action(() =>
            {
                Configuration.Load.From(new MemoryStore()).Select(typeof(ConfigNotDecorated));
            })
                .Verify().Throws<ValidationException>();
        }

        [TestMethod]
        public void Load_RequiredSettingNotFound()
        {
            new Action(() =>
            {
                Configuration.Load.From(new MemoryStore()).Select(typeof(RequiredSettings));
            })
            .Verify().Throws<ConfigurationLoadException>();
        }

        [TestMethod]
        public void Load_PropertyNameNullOrEmpty()
        {
            new Action(() =>
            {
                Configuration.Load.From(new MemoryStore()).Where(null, null);
            })
                .Verify().Throws<ValidationException>();

            new Action(() =>
            {
                Configuration.Load.From(new MemoryStore()).Where(string.Empty, null);
            })
                .Verify().Throws<ValidationException>();
        }

    }
}

namespace SmartConfig.Core.Tests.Integration.ConfigurationTestConfigs
{
    [SmartConfig]
    public static class EmptyConfig { }

    [SmartConfig]
    [TypeConverter(typeof(JsonToObjectConverter<List<Int32>>))]
    public static class FullConfig
    {
        public static SByte SByte { get; set; }
        public static Byte Byte { get; set; }
        public static Char Char { get; set; }
        public static Int16 Int16 { get; set; }
        public static Int32 Int32 { get; set; }
        public static Int64 Int64 { get; set; }
        public static UInt16 UInt16 { get; set; }
        public static UInt32 UInt32 { get; set; }
        public static UInt64 UInt64 { get; set; }
        public static Single Single { get; set; }
        public static Double Double { get; set; }
        public static Decimal Decimal { get; set; }

        public static String String { get; set; }
        public static bool False { get; set; }
        public static bool True { get; set; }
        public static DateTime DateTime { get; set; }
        public static TestEnum Enum { get; set; }

        public static Color ColorName { get; set; }
        public static Color ColorDec { get; set; }
        public static Color ColorHex { get; set; }

        public static List<int> JsonArray { get; set; }

        [Reusable.Data.Annotations.Optional]
        public static string Optional { get; set; } = "Waldo";

        [Itemized]
        public static int[] ItemizedArray { get; set; }

        [Itemized]
        public static Dictionary<string, int> ItemizedDictionary { get; set; }

        public static class NestedConfig
        {
            public static string NestedString { get; set; }
        }

        [Reusable.Data.Annotations.Ignore]
        public static class IgnoredConfig
        {
            public static string IgnoredString { get; set; } = "Grault";
        }
    }

    [SmartConfig]
    public static class SettingNotFoundConfig
    {
        public static string MissingSetting { get; set; }
    }

    public enum TestEnum
    {
        TestValue1,
        TestValue2,
        TestValue3
    }



    [SmartConfig]
    public class NonStaticConfig { }

    public static class ConfigNotDecorated { }

    [SmartConfig]
    public static class RequiredSettings
    {
        public static int Int32Setting { get; set; }
    }

    [SmartConfig]
    internal static class TestConfig
    {
        [Optional]
        public static string Foo { get; set; } = "Bar";
    }
}