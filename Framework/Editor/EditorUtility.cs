using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework
{
    public static class EditorUtility
    {
        /// <summary>
        /// 为指定的路径生成对应目录
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        static public bool PrepareDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
                return true;
            try
            {
                Directory.CreateDirectory(dirPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Can't create directory " + dirPath + ". Exception is " + e.ToString());
            }

            return Directory.Exists(dirPath);
        }

        static public T CreateScriptableObjectAsset<T>(string path) where T : ScriptableObject
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                PrepareDirectory(Path.GetDirectoryName(path));
            }
            ScriptableObject asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset as T;
        }
    }
}
