﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SmartConfig.Data
{
    /// <summary>
    /// Exposes a version for a filter.
    /// </summary>
    public interface ISemanticVersion
    {
        string SemanticVersion { get; }
    }
}
