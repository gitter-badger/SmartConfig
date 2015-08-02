﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SmartConfig.Data
{
    /// <summary>
    /// Specifies a common criteria.
    /// </summary>
    public interface IVersion
    {
        string Version { get; }
    }
}