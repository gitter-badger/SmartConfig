﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace SmartConfig.Converters
{
    /// <summary>
    /// Converts numeric types from and to a string.
    /// </summary>
    public class NumericConverter : ObjectConverter
    {
        public NumericConverter()
            : base(new[]
            {
                // integral types
                typeof(sbyte),
                typeof(byte),
                typeof(char),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                // floating-point types
                typeof(float),
                typeof(double),
                // decimal
                typeof(decimal),
            })
        {
        }

        public override object DeserializeObject(object value, Type type, IEnumerable<ConstraintAttribute> constraints)
        {
            if (value is string)
            {
                var parseMethod = type.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
                if (parseMethod != null)
                {
                    value = parseMethod.Invoke(null, new[] { value, CultureInfo.InvariantCulture });
                }
                else
                {
                    parseMethod = type.GetMethod("Parse", new[] { typeof(string) });
                    if (parseMethod != null)
                    {
                        value = parseMethod.Invoke(null, new[] { value });
                    }
                }
            }

            constraints.Check<RangeAttribute>(range =>
            {
                if (!range.IsValid((IComparable)value)) throw new RangeViolationException
                {
                    Range = range.ToString(),
                    Value = value.ToString()
                };
            });

            return value;
        }

        public override object SerializeObject(object value, Type type, IEnumerable<ConstraintAttribute> constraints)
        {
            constraints.Check<RangeAttribute>(range =>
            {
                if (!range.IsValid((IComparable)value)) throw new RangeViolationException
                {
                    Range = range.ToString(),
                    Value = value.ToString()
                };
            });

            if (value.GetType() == type) { return value; }

            var toStringMethod = type.GetMethod("ToString", new[] { typeof(IFormatProvider) });
            if (toStringMethod != null)
            {
                var result = toStringMethod.Invoke(value, new object[] { CultureInfo.InvariantCulture });
                return (string)result;
            }
            else
            {
                toStringMethod = type.GetMethod("ToString", new Type[] { });
                if (toStringMethod != null)
                {
                    var result = toStringMethod.Invoke(value, null);
                    return (string)result;
                }
            }

            throw new Exception("ToString method not found.");
        }
    }
}