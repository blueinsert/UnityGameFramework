using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework.ConfigData;
using System;

namespace bluebean.UGFramework
{
    [Serializable]
    public class LogConfig
    {
        [SerializeField]
        public string LogPrefix = "Log_";
    }

    public class AppClientSetting : ScriptableObjectSingleton<AppClientSetting>
    {
        //反射引用
        private static string AssetPath = "Assets/GameProject/Resources/AppClientSettingAsset.asset";

        [SerializeField]
        public ConfigDataSetting m_configDataSetting;
        [SerializeField]
        public bool m_useAssetBundleInEditor = false;
        [SerializeField]
        public LogConfig m_logConfg = null;

    }
}
