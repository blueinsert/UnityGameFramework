﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework.ConfigData;

namespace bluebean.UGFramework
{

    public class GameClientSetting : ScriptableObjectSingleton<GameClientSetting>
    {
        //反射引用
        private static string AssetPath = "Assets/GameProject/Resources/GameClientSettingAsset.asset";

        [SerializeField]
        public ConfigDataSetting m_configDataSetting;
        [SerializeField]
        public bool m_useAssetBundleInEditor = false;

       
    }
}
