﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmartConfig.Tests
{
    [TestClass]
    public class FiltersTests
    {
        private readonly TestConfigElement[] _testItems = new[]
        {
            new TestConfigElement("ABC|*|name|value"),
            new TestConfigElement("XYZ|1.3.0|name|value"),
            new TestConfigElement("XYZ|2.4.0|name|value"),
            new TestConfigElement("*|3.0.0|name|value"),
            new TestConfigElement("JKL|4.1.8|name|value"),
        };

        [TestMethod]
        public void FilterByString()
        {           
            var result = Filters.FilterByString(_testItems, new KeyValuePair<string, string>("Environment", "ABC"));
            var item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[0], item);

            result = Filters.FilterByString(_testItems, new KeyValuePair<string, string>("Environment", "JKL"));
            item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[3], item);

            result = Filters.FilterByString(_testItems, new KeyValuePair<string, string>("Environment", "ASD"));
            item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[2], item);
        }

        [TestMethod]
        public void FilterByVersion()
        {
            var result = Filters.FilterByVersion(_testItems, new KeyValuePair<string, string>("Version", "2.4.1"));
            var item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[2], item);

            result = Filters.FilterByVersion(_testItems, new KeyValuePair<string, string>("Version", "1.4.0"));
            item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[1], item);

            result = Filters.FilterByVersion(_testItems, new KeyValuePair<string, string>("Version", "1.1.3"));
            item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[0], item);
        }

        [TestMethod]
        public void FilterByString_Then_FilterByVersion()
        {
            var result = Filters.FilterByString(_testItems, new KeyValuePair<string, string>("Environment", "XYZ"));
            result = Filters.FilterByVersion(result, new KeyValuePair<string, string>("Version", "1.4.0"));
            var item = result.FirstOrDefault();
            Assert.AreEqual(_testItems[1], item);
        }
    }
}