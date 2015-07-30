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
    internal static class FieldInfoExtensions
    {
        public static bool HasAttribute<T>(this FieldInfo fieldInfo, bool inherit) where T : Attribute
        {
            return fieldInfo.GetCustomAttribute<T>(inherit) != null;
        }

#if NET40

        public static bool GetCustomAttributes<T>(this FieldInfo fieldInfo, bool inherit) where T : Attribute
        {
            return fieldInfo.GetCustomAttributes(typeof(T), inherit).SingleOrDefault() != null;
        }    


        public static TAttribute GetCustomAttribute<TAttribute>(this FieldInfo fieldInfo, bool inherit) where TAttribute : Attribute
        {
            return fieldInfo.GetCustomAttributes<TAttribute>(inherit).SingleOrDefault();
        }   

#endif

        #region Constraint shortcuts

        public static IEnumerable<ValueConstraintAttribute> Contraints(this FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttributes<ValueConstraintAttribute>(true);
        }

        #endregion

        #region 

        public static bool IsNullable(this FieldInfo fieldInfo)
        {
            var isNullable =
                (fieldInfo.FieldType.IsValueType && fieldInfo.FieldType.IsNullable())
                || fieldInfo.GetCustomAttribute<NullableAttribute>() != null;
            return isNullable;
        }

        public static bool IsOptional(this FieldInfo fieldInfo)
        {
            var isOptional = fieldInfo.GetCustomAttribute<OptionalAttribute>() != null;
            return isOptional;
        }

        #endregion
    }
}
