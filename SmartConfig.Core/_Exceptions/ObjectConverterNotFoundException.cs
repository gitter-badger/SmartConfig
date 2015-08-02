﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartConfig
{
    /// <summary>
    /// Occurs when an object converter could not be found.
    /// </summary>
    public class ObjectConverterNotFoundException : Exception
    {
        internal ObjectConverterNotFoundException(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the type for which a converter could not be found.
        /// </summary>
        public Type Type { get; private set; }
    }
}