using System.Collections;
using System.Collections.Generic;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace bluebean.UGFramework
{

    /// <summary>
    /// 路径辅助类
    /// </summary>
    public static class PathUtility
    {

        /// <summary>
        /// 从Hierarchy窗口中获取路径
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="relativeRoot"></param>
        /// <returns></returns>
        static public string GetRelativeHierarchy(GameObject obj, GameObject relativeRoot)
        {
            if (obj == null)
                return "Empty Object";

            StringBuilder sb = new StringBuilder();
            sb.Append(obj.name);

            while (obj.transform.parent != null && obj.transform.parent.gameObject != relativeRoot)
            {
                obj = obj.transform.parent.gameObject;
                sb.Insert(0, "/");
                sb.Insert(0, obj.name);
            }

            return sb.ToString();
        }

#if UNITY_EDITOR

        /// <summary>
        /// 在编辑器中获取路径，对象Hierarchy路径或者，asset的资源路径
        /// </summary>
        [MenuItem("Assets/BlackJack/Get Path", false, 1)]
        [MenuItem("BlackJack/Util/Get Path %k", false, 4)]
        static void GetPath()
        {
            if (Selection.activeTransform != null)
            {
                GameObject sel = Selection.activeTransform.gameObject;
                EditorGUIUtility.systemCopyBuffer = GetRelativeHierarchy(sel, null);
            }
            else if (Selection.activeObject != null)
            {
                UnityEngine.Object sel = Selection.activeObject;
                if (AssetDatabase.Contains(sel))
                {
                    EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(sel);
                }
            }
            Debug.Log("GetPath finish!");
        }
#endif
        /// <summary>
        /// 去掉文件扩展名
        /// </summary>
        /// <param name="assetPath"></param>
        public static string RemoveExtension(string assetPath)
        {
            int dotIndex = assetPath.LastIndexOf(".");
            if (dotIndex == -1)
            {
                Debug.LogError("RemoveExtension error: can't find '.'");
                return null;
            }
            string path = assetPath.Substring(0, dotIndex);
            return path;
        }

        /// <summary>
        /// 解析出相对Resources的路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetSubPathInResources(string assetPath)
        {
            int index = assetPath.LastIndexOf("Resources/");
            if (index == -1)
            {
                Debug.LogError(string.Format("GetSubPathInResources error: can't find 'Resources' path:{0}",assetPath));
                return null;
            }
            index += "Resources/".Length;
            string path = assetPath.Substring(index);
            return path;
        }

        /// <summary>
        /// 获取完整路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetFullPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets"))
            {
                Debug.LogError(string.Format("GetFullPath assetPath:{0} is not start with 'Assets'", assetPath));
                return null;
            }
            assetPath = assetPath.Substring("Assets".Length);
            return Application.dataPath + assetPath;
        }

    }
}
