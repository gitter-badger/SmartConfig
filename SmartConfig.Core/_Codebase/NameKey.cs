﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartConfig
{
    public class NameKey
    {
        private readonly SettingKey _settingKey;

        public NameKey(SettingKey settingKey)
        {
            if (!(settingKey.Value is SettingPath))
            {
                throw new InvalidOperationException(
                    $"{nameof(settingKey)}.{nameof(SettingKey.Value)} must be of type {nameof(SettingPath)}.");
            }
            _settingKey = settingKey;
        }

        public string Name => nameof(Data.Setting.Name);

        public SettingPath Value => (SettingPath)_settingKey.Value;

        public static implicit operator string(NameKey nameKey)
        {
            Debug.Assert(nameKey._settingKey.Value is SettingPath, "NameKey.Value must be of type SettingPath.");
            return nameKey._settingKey.Value.ToString();
        }
    }
}