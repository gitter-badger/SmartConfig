﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable;

namespace SmartConfig.Services
{
   
    internal class CollectionItemizer
    {
        private static readonly Func<object, IEnumerable<KeyValuePair<object, object>>>[] Itemizers =
        {
            ItemizeArray,
            ItemizeList,
            ItemizeHashSet,
            ItemizeDictionary,
        };

        public static IEnumerable<KeyValuePair<object, object>> ItemizeCollection(object obj)
        {
            return
                Itemizers
                    .Select(itemize => itemize(obj))
                    .FirstOrDefault(collection => collection != null);
        }

        public static IEnumerable<KeyValuePair<object, object>> ItemizeArray(object obj)
        {
            if (!obj.GetType().IsArray) { return null; }

            var result =
                ((IEnumerable)obj)
                .Cast<object>()
                .Select((value, key) => new KeyValuePair<object, object>(key, value));

            return result;
        }

        public static IEnumerable<KeyValuePair<object, object>> ItemizeList(object obj)
        {
            if (!obj.GetType().IsList()) { return null; }

            var result =
                ((IEnumerable)obj)
                .Cast<object>()
                .Select((value, key) => new KeyValuePair<object, object>(key, value));

            return result;
        }

        public static IEnumerable<KeyValuePair<object, object>> ItemizeHashSet(object obj)
        {
            if (!obj.GetType().IsHashSet()) { return null; }
            
            var result =
                ((IEnumerable)obj)
                .Cast<object>()
                .Select((value, key) => new KeyValuePair<object, object>(key, value));

            return result;
        }

        public static IEnumerable<KeyValuePair<object, object>> ItemizeDictionary(object obj)
        {
            if (!obj.GetType().IsDictionary()) { return null; }

            var dictionary = (IDictionary)obj;

            var result = dictionary.Keys
                .Cast<object>()
                .Select(key => new KeyValuePair<object, object>(key: key, value: dictionary[key]));

            return result;
        }      
    }
}
