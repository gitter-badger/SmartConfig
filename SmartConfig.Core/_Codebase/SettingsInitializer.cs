﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartConfig.Collections;
using SmartConfig.Data;

namespace SmartConfig
{
    internal class SettingsInitializer
    {
        private readonly IConfigReflector _configReflector;

        private readonly SettingsUpdater _settingsUpdater;

        private readonly IDataSourceCollection _dataSources;

        public SettingsInitializer(IConfigReflector configReflector, SettingsUpdater settingsUpdater, IDataSourceCollection dataSources)
        {
            _configReflector = configReflector;
            _settingsUpdater = settingsUpdater;
            _dataSources = dataSources;
        }

        public bool InitializeSettings(Type configType)
        {
            var dataSource = _dataSources[configType];
            if (!CheckSettingsInitialized(dataSource))
            {
                return false;
            }

            var settingInfos = _configReflector.GetSettingInfos(configType);
            foreach (var settingInfo in settingInfos)
            {
                InitializeSetting(settingInfo);
            }

            //    var settingInitializedSettingInfo = SettingInfoFactory.CreateSettingsInitializedSettingInfo(configType);
            //    UpdateSetting(settingInitializedSettingInfo, true, true);

            return true;
        }

        private bool CheckSettingsInitialized(IDataSource dataSource)
        {
            var settingInitialized = dataSource.Select(KeyNames.Internal.SettingsInitializedKeyName);
            bool result;
            bool.TryParse(settingInitialized, out result);

            //Logger.LogTrace(() => $"{KeyNames.Internal.SettingsInitializedKeyName} = \"{result}\"");

            return result;
        }

        private void InitializeSetting(SettingInfo settingInfo)
        {
            Logger.LogTrace(() => $"Initializing SettingPath = \"{settingInfo.SettingPath}\"");
            _settingsUpdater.UpdateSetting(settingInfo, settingInfo.Value);
        }


    }
}