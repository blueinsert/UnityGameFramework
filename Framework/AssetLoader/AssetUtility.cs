using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace bluebean.UGFramework.Asset
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

        public static string MakeAssetPath(string path)
        {
            if (path.StartsWith(RuntimeAssetsPath))
            {
                return path;
            }
            return string.Format("{0}{1}", RuntimeAssetsPath, path);
        }

        public static string MakeSpriteAssetPath(string path)
        {
            if (path.Contains("@"))
            {
                return path;
            }
            return string.Format("{0}{1}@{2}", RuntimeAssetsPath, path, Path.GetFileNameWithoutExtension(path));
        }


        /// <summary>
        /// 将资源名称加到列表，如果已经在列表中就不加
        /// </summary>
        /// <param name="name"></param>
        /// <param name="list"></param>
        public static void AddAssetToList(string assetName, List<string> list)
        {
            if (string.IsNullOrEmpty(assetName))
                return;
            if (list == null)
                return;

            if (assetName == "NULL")
            {
                Debug.LogWarning("AddAssetToList, Wrong asset name NULL");
                return;
            }
            if (list.Contains(assetName))
                return;
            list.Add(assetName);
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
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T GetAsset<T>(string assetPath) where T : UnityEngine.Object
        {

            if (string.IsNullOrEmpty(assetPath))
                return null;
            if (!assetPath.StartsWith(RuntimeAssetsPath))
            {
                assetPath = MakeAssetPath(assetPath);
            }
            UnityEngine.Object o = _GetAsset(assetPath);
            T ret = o as T;
            if (ret == null)
                Debug.LogError(string.Format("AssetUtility.GetAsset <{0}> {1} is null, {2}.",
                    typeof(T).Name,
                    assetPath,
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

        private UnityEngine.Object _GetAsset(string aname)
        {
            if (string.IsNullOrEmpty(aname))
                return null;

            foreach (var assetcache in m_dynamicAssetCaches)
            {
                UnityEngine.Object a;
                if (assetcache.TryGetValue(aname, out a))
                    return a;
            }

            Debug.LogError("_GetAsset, " + aname + " not loaded");
            return null;
        }
    }
}
