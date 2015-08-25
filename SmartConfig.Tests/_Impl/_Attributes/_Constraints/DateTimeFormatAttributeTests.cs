﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConfig.Tests
{
    [TestClass()]
    public class DateTimeFormatAttributeTests
    {
        [TestMethod()]
        public void ctor_DateTimeFormatAttribute()
        {
            const string format = "ddMMyyyy";
            var attr = new DateTimeFormatAttribute(format);
            Assert.AreEqual(format, attr.Format);
            Assert.AreEqual(format, attr);
            Assert.AreEqual(format, attr.ToString());
        }

        [TestMethod()]
        public void TryParseExact_True()
        {
            const string format = "ddMMyyyy";
            var attr = new DateTimeFormatAttribute(format);
            DateTime result;
            Assert.IsTrue(attr.TryParseExact("12092015", out result));
        }

        [TestMethod()]
        public void TryParseExact_False()
        {
            const string format = "ddMMMyyyy";
            var attr = new DateTimeFormatAttribute(format);
            DateTime result;
            Assert.IsFalse(attr.TryParseExact("12092015", out result));
        }
    }
}