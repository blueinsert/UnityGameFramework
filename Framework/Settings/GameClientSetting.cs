using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework.ConfigData;

namespace bluebean.UGFramework
{

    public class GameClientSetting : ScriptableObject
    {
        private const string AssetPath = "Assets/GameProject/Resources/GameClientSettingAsset.asset";

        [SerializeField]
        public ConfigDataSetting m_configDataSetting;

        public bool m_useAssetBundleInEditor = false;

        private static GameClientSetting GetAsset()
        {
            var subPath = PathUtility.GetSubPathInResources(AssetPath);
            subPath = PathUtility.RemoveExtension(subPath);
            GameClientSetting setting = Resources.Load<GameClientSetting>(subPath);
            return setting;
        }

        private static GameClientSetting CreateAsset()
        {
            GameClientSetting setting = ScriptableObject.CreateInstance<GameClientSetting>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(setting, AssetPath);
#endif
            return setting;
        }

        private static GameClientSetting m_instance;
        public static GameClientSetting Instance
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
