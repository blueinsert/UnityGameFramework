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
    public class ConfigDataSetting : ScriptableObject
    {
        /// <summary>
        /// 配置路径
        /// </summary>
        private const string ConfigDataSettingAssetPath = "Assets/Framework/ConfigData/Editor/ConfigDataSettingAsset.asset";

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

        private static ConfigDataSetting GetAsset()
        {
            ConfigDataSetting setting = AssetDatabase.LoadAssetAtPath(ConfigDataSettingAssetPath, typeof(ConfigDataSetting)) as ConfigDataSetting;
            return setting;
        }

        private static ConfigDataSetting CreateAsset()
        {
            ConfigDataSetting setting = ScriptableObject.CreateInstance<ConfigDataSetting>();
            AssetDatabase.CreateAsset(setting, ConfigDataSettingAssetPath);
            return setting;
        }

        private static ConfigDataSetting m_instance;
        public static ConfigDataSetting Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GetAsset();
                    if (m_instance == null)
                    {
                        m_instance = CreateAsset();
                    }
                }
                return m_instance;
            }
        }
    }
}
