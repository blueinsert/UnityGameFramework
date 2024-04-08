using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
	public static class AssetBundleUtility
	{

        private static Dictionary<string, string> m_dnNameDict = new Dictionary<string, string>();

        /// <summary>
        /// 获取路径对应的唯一名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetAssetDNNameFromPath(string path)
        {
            if (m_dnNameDict.ContainsKey(path))
                return m_dnNameDict[path];

            string dnname = path.Replace('/', '_');
            if (dnname.Contains("\\\\"))
                dnname = dnname.Replace("\\\\", "_");
            if (dnname.Contains("\\"))
                dnname = dnname.Replace("\\", "_");
            if (dnname.Contains("."))
                dnname = dnname.Replace(".", "_");

            dnname += ".b";
            const string removePrefix = "assets_gameproject_runtimeassets_";
            dnname = dnname.StartsWith(removePrefix, StringComparison.OrdinalIgnoreCase) ?
                dnname.Substring(removePrefix.Length) : dnname;

            m_dnNameDict[path] = dnname;
            return dnname;
        }

        /// <summary>
        /// 获取路径对应的bundlename
        /// </summary>
        /// <param name="path"></param>
        /// <param name="replaceLastFolderNameStr"></param>
        /// <param name="autoExtension"></param>
        /// <returns></returns>
        public static string GetBundleNameByAssetPath(string path)
        {
            return GetAssetDNNameFromPath(path).ToLower();
        }
    }
}