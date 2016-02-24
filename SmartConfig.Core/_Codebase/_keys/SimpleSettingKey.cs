﻿using System;
using System.Diagnostics;
using SmartConfig.Data;
using SmartConfig.Filters;

namespace SmartConfig
{
    [DebuggerDisplay("Name = {Name} Value = {Value}")]
    public class SimpleSettingKey
    {        
        public SimpleSettingKey(string name, object value)
        {
            if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name)); }
            if (value == null) { throw new ArgumentNullException(nameof(value)); }

            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }

        public static bool operator ==(SimpleSettingKey x, SimpleSettingKey y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.Name == y.Name &&
                x.Value == y.Value;
        }

        public static bool operator !=(SimpleSettingKey x, SimpleSettingKey y)
        {
            return !(x == y);
        }

        protected bool Equals(SimpleSettingKey other)
        {
            return string.Equals(Name, other.Name) && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SimpleSettingKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ (Value?.GetHashCode() ?? 0);
            }
        }
    }
}