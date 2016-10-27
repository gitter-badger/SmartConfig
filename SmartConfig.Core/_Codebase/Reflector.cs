﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable;
using Reusable.Validations;
using SmartConfig.DataAnnotations;

namespace SmartConfig
{
    internal static class Reflector
    {
        // gets either custom or default member name
        public static string GetCustomNameOrDefault(this MemberInfo member)
        {
            member.Validate(nameof(member)).IsNotNull();
            return member.GetCustomAttribute<RenameAttribute>()?.Name ?? member.Name;
        }

        public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttributes<T>(false).Any();
        }

        public static IEnumerable<string> GetSettingPath(this PropertyInfo propertyInfo)
        {
            var path = new LinkedList<string>();
            path.AddFirst(propertyInfo.GetCustomNameOrDefault());

            var type = propertyInfo.DeclaringType;

            while (type != null && !type.HasAttribute<SmartConfigAttribute>())
            {
                path.AddFirst(type.GetCustomNameOrDefault());
                type = type.DeclaringType;
            }

            // This should never happen because the type is already checked.
            if (type == null)
            {
                throw new SmartConfigAttributeNotFoundException(propertyInfo);
            }

            // add config name if available
            var smartConfigAttribute = type.GetCustomAttribute<SmartConfigAttribute>();
            if (smartConfigAttribute.NameOption == ConfigNameOption.AsPath)
            {
                if (!string.IsNullOrEmpty(smartConfigAttribute.Name))
                {
                    path.AddFirst(smartConfigAttribute.Name);
                }
            }
            
            return path;
        }
    }
}
