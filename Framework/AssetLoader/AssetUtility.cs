using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class AssetUtility
    {
        public static AssetUtility Instance 
        { set { s_instance = value; } 
            get {
                if (s_instance == null)
                    s_instance = new AssetUtility();
                return s_instance; 
            } 
        }

        static AssetUtility s_instance;

        List<Dictionary<string, UnityEngine.Object>> m_dynamicAssetCaches = new List<Dictionary<string, UnityEngine.Object>>();

        public const string RuntimeAssetsPath = "Assets/GameProject/RuntimeAssets/";

        private AssetUtility()
        {
        }

        #region public static

        public static string MakeSpriteAssetPath(string path)
        {
            if (path.Contains("@"))
            {
                return path;
            }
            return string.Format("{0}@{1}", path, Path.GetFileNameWithoutExtension(path));
        }


        /// <summary>
        /// 将资源名称加到列表，如果已经在列表中就不加
        /// </summary>
        /// <param name="name"></param>
        /// <param name="list"></param>
        public static void AddAssetToList(string name, List<string> list)
        {
            if (string.IsNullOrEmpty(name))
                return;
            if (list == null)
                return;

            if (name == "NULL")
            {
                Debug.LogWarning("AddAssetToList, Wrong asset name NULL");
                return;
            }

            string assetName = AssetUtility.RuntimeAssetsPath + name;
            if (list.Contains(assetName))
                return;
            list.Add(assetName);
        }

        /// <summary>
        /// 将Sprite资源名称加到列表，如果已经在列表中就不加
        /// </summary>
        /// <param name="name"></param>
        /// <param name="list"></param>
        public static void AddSpriteAssetToList(string name, List<string> list)
        {
            if (string.IsNullOrEmpty(name))
                return;
            if (list == null)
                return;

            if (name == "NULL")
            {
                Debug.LogWarning("AddSpriteAssetToList, Wrong sprite name NULL");
                return;
            }

            if (name.IndexOf('@') >= 0)
                AddAssetToList(name, list);
            else
                AddAssetToList(AssetUtility.MakeSpriteAssetPath(name), list);
        }


        #endregion

        /// <summary>
        /// 注册UITask的DynamicResCacheDict
        /// </summary>
        /// <param name="assetCache"></param>
        public void RegisterDynamicAssetCache(Dictionary<string, UnityEngine.Object> assetCache)
        {
            if (assetCache == null)
                return;

            if (m_dynamicAssetCaches.Contains(assetCache))
            {
                Debug.LogError("RegisterDynamicAssetCache alreasy exist");
                return;
            }

            m_dynamicAssetCaches.Insert(0, assetCache);
        }

        /// <summary>
        /// 注销UITask的DynamicResCacheDict
        /// </summary>
        /// <param name="assetCache"></param>
        public void UnregisterDynamicAssetCache(Dictionary<string, UnityEngine.Object> assetCache)
        {
            if (assetCache == null)
                return;

            if (!m_dynamicAssetCaches.Contains(assetCache))
            {
                Debug.LogError("UnregisterDynamicAssetCache not exist");
                return;
            }

            m_dynamicAssetCaches.Remove(assetCache);
        }

        /// <summary>
        /// 获取资源（资源必须已经从UITask或MapSceneTask加载）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetAsset<T>(string name) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(name))
                return null;
            UnityEngine.Object o = _GetAsset(name);
            T ret = o as T;
            if (ret == null)
                Debug.LogError(string.Format("AssetUtility.GetAsset <{0}> {1} is null, {2}.",
                    typeof(T).Name,
                    name,
                    o == null ? string.Empty : string.Format("the actual type is: {0}", o.GetType().Name)));
            return ret;
        }

        /// <summary>
        /// 获取Sprite资源（资源必须已经从UITask或MapSceneTask加载）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite GetSprite(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (name.IndexOf('@') >= 0)
                return GetAsset<Sprite>(name);
            else
                return GetAsset<Sprite>(AssetUtility.MakeSpriteAssetPath(name));
        }

        UnityEngine.Object _GetAsset(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string aname = AssetUtility.RuntimeAssetsPath + name;

            foreach (var assetcache in m_dynamicAssetCaches)
            {
                UnityEngine.Object a;
                if (assetcache.TryGetValue(aname, out a))
                    return a;
            }

            Debug.LogError("_GetAsset, " + name + " not loaded");
            return null;
        }
    }
}
