using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class BuildSetting : ScriptableObjectSingleton<BuildSetting>
    {
        //反射引用
        private readonly static string AssetPath = "Assets/GameProject/Resources/BuildSettingAsset.asset";

        [Header("构建临时文件存放目录")]
        [SerializeField]
        public string BuildPath = "../Build/";
        [SerializeField]
        public string LogPath = "../Build/Logs/";
        [Header("BuildDataPath")]
        public string BuildDataPath = "Assets/StreamingAssets/BundleData/";
        [Header("资源包存放目录")]
        [SerializeField]
        public string AssetBundleDir = "StreamingAssets/AssetBundles/";
        [Header("Exe打包目录")]
        [SerializeField]
        public string WinExeDir = "../Build/Win/";
        [SerializeField]
        public string WinExeName = "YaYa内镜模拟.exe";
        [Header("Apk打包目录")]
        [SerializeField]
        public string AndroidApkDir = "../Build/Apk/";
        [SerializeField]
        public string AndroidApkName = "YaYa内镜模拟.apk";

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Framework/Setting/CreateBuildSetting")]
        public static void CreateBuildSetting()
        {
            CreateAsset();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif

    }
}
