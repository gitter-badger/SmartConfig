﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartConfig.Data
{
    /// <summary>
    /// Implements and extends the <c>IDataSource</c> interface.
    /// </summary>
    public abstract class DataSource<TConfigElement> : IDataSource where TConfigElement : ConfigElement, new()
    {
        protected IDictionary<string, CustomKey<TConfigElement>> _customKeys;

        protected DataSource()
        {
            OrderedKeyNames = GetOrderedKeyNames();
            _customKeys = new Dictionary<string, CustomKey<TConfigElement>>();
        }

        public IEnumerable<CustomKey<TConfigElement>> CustomKeys
        {
            get { return _customKeys.Values; }
            set
            {
                if (value == null) throw new ArgumentNullException("CustomKeys");
                _customKeys = value.ToDictionary(k => k.Name);
            }
        }

        IDictionary<string, string> IDataSource.CustomKeys
        {
            get { return _customKeys.ToDictionary(x => x.Key, x => x.Value.Value); }
        }

        /// <summary>
        /// Gets an ordered enumeration of key names.
        /// The first element is always the Name then follow other keys in alphabetical order.
        /// </summary>
        public IEnumerable<string> OrderedKeyNames { get; protected set; }

        public bool InitializationEnabled { get; set; }

        public virtual void Initialize(IDictionary<string, string> values) { }

        public abstract string Select(IDictionary<string, string> keys);

        public abstract void Update(IDictionary<string, string> keys, string value);

        /// <summary>
        /// Applies all of the specified filters.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected IEnumerable<TConfigElement> ApplyFilters(IEnumerable<TConfigElement> elements, IDictionary<string, string> keys)
        {
            elements = keys
                .Where(x => x.Key != SmartConfig.KeyNames.DefaultKeyName)
                .Aggregate(elements, (current, item) => _customKeys[item.Key].Filter(current, item));
            return elements;
        }

        private IEnumerable<string> GetOrderedKeyNames()
        {
            var result = new List<string> { KeyNames.DefaultKeyName };

            var propertyNames = 
                typeof(TConfigElement)
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.Name)
                .OrderBy(n => n);
            result.AddRange(propertyNames);
            return result;
        }

        public DataSource<TConfigElement> AddCustomKey(Action<CustomKey<TConfigElement>> configureKey)
        {
            var customKey = new CustomKey<TConfigElement>();
            configureKey(customKey);
            _customKeys[customKey.Name] = customKey;
            return this;
        }
    }
}
