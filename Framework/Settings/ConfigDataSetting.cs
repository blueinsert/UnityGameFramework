using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.ConfigData
{
    public enum ConfigDataSerializeType
    {
        Bin,
        Xml,
    }

    [System.Serializable]
    public class ConfigDataSetting : ScriptableObjectSingleton<ConfigDataSetting>
    {
        //反射引用
        private static string AssetPath = "Assets/GameProject/Resources/ConfigDataSettingAsset.asset";

        [Header("配置表数据序列化类型")]
        public ConfigDataSerializeType m_configDataSerializeType = ConfigDataSerializeType.Bin;
        [Header("源代码文件路径")]
        public string m_sourceCodeFilePath;
        [Header("目标代码文件路径")]
        public string m_targetCodeFilePath;
        [Header("源序列化资源文件路径")]
        public string m_sourceSerializedFilePath;
        [Header("目标序列化资源文件路径")]
        public string m_targetSerializedFilePath;

    }
}
