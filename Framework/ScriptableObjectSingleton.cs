using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T>
    {
        private static string AssetPath
        {
            get
            {
                var field = typeof(T).GetField("AssetPath",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
                var assetPath = field.GetValue(null) as string;
                return assetPath;
            }
        }

        private static T GetAsset()
        {
            var subPath = PathUtility.GetSubPathInResources(AssetPath);
            subPath = PathUtility.RemoveExtension(subPath);
            T setting = Resources.Load<T>(subPath);
            return setting;
        }

        protected static T CreateAsset()
        {
            T setting = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(setting, AssetPath);
#endif
            return setting;
        }

        private static T m_instance;
        public static T Instance
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
