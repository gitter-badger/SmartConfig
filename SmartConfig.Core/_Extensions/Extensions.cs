﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartConfig
{
    internal static class Extensions
    {

        #region Type extensions.

        public static bool IsNullable(this Type type)
        {
            var isNullable =
                type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return isNullable;
        }

        public static bool IsIEnumerable(this Type type)
        {
            var isIEnumerable =
                type != typeof(string)
                && type.GetInterfaces()
                .Any(t => t.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return isIEnumerable;
        }

        public static bool IsList(this Type type)
        {
            var isList =
                type != typeof(string)
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(List<>);
            return isList;
        }

        public static bool IsDictionary(this Type type)
        {
            var isList =
                type != typeof(string)
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            return isList;
        }

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), false).Any();
        }      

        public static T CustomAttribute<T>(this Type type) where T : Attribute
        {
#if NET40
            return (T)type.GetCustomAttributes(typeof(T), false).SingleOrDefault();
#else
            return type.GetCustomAttribute<T>(false);
#endif
        }

        #endregion

        public static bool HasAttribute<T>(this FieldInfo fieldInfo) where T : Attribute
        {
#if NET40
            return fieldInfo.GetCustomAttributes(typeof(T), false).SingleOrDefault() != null;
#else
            return fieldInfo.GetCustomAttribute<T>(false) != null;
#endif
        }

        #region Object extensions.

        public static string ToStringOrEmpty(this object obj)
        {
            return obj == null ? string.Empty : obj.ToString();
        }

        #endregion
    }
}
