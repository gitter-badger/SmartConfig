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
    public abstract class DataSource<TSetting> : IDataSource where TSetting : Setting, new()
    {
        private KeyNames _keyNames;

        private IDictionary<string, KeyProperties> _keyProperties = new Dictionary<string, KeyProperties>();       

        public KeyNames KeyNames
        {
            get
            {
                if (_keyNames != null)
                {
                    return _keyNames;
                }
                _keyNames = KeyNames.From<TSetting>();
                return _keyNames;
            }
        }

        public IEnumerable<string> KeyNamesWithoutDefault
        {
            get { return KeyNames.Where(k => k != KeyNames.DefaultKeyName); }
        }

        public IDictionary<string, KeyProperties> KeyProperties
        {
            get { return _keyProperties; }
            set { _keyProperties = value; }
        }

        public bool SettingsInitializationEnabled { get; set; }

        public abstract string Select(string defaultKeyValue);

        public abstract void Update(string defaultKeyValue, string value);

        /// <summary>
        /// Applies all of the specified filters.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected IEnumerable<TSetting> ApplyFilters(IEnumerable<TSetting> elements, IDictionary<string, string> keys)
        {
            elements = keys
                .Where(x => x.Key != KeyNames.DefaultKeyName)
                .Aggregate(elements, (current, item) => KeyProperties[item.Key].Filter(current, item).Cast<TSetting>());
            return elements;
        }

        protected CompositeKey CreateCompositeKey(string defaultKeyValue)
        {
            return new CompositeKey(defaultKeyValue, KeyNames, KeyProperties);
        }
    }
}