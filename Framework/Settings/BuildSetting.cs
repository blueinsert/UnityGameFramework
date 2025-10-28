using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class BuildSetting : ScriptableObjectSingleton<BuildSetting>
    {
        //反射引用
        private readonly static string AssetPath = "Assets/GameProject/Resources/BuildSettingAsset.asset";

        [Header("构建临时文件存放目录,相对于工程路径")]
        [SerializeField]
        public string BuildPath = "Build/";
        [SerializeField]
        public string LogPath = "Build/Logs/";
        [SerializeField]
        public string AssetBundleDir = "Build/AssetBundles/";

        [Header("BuildDataPath")]
        public string BuildDataPath = "Assets/GameProject/RuntimeAssets/BundleData_AB/";
        
        [Header("Exe打包目录")]
        [SerializeField]
        public string WinExeDir = "../Build/Win/";
        [SerializeField]
        public string WinExeName = "demo.exe";
       
        [Header("Apk打包目录")]
        [SerializeField]
        public string AndroidApkDir = "../Build/Apk/";
        [SerializeField]
        public string AndroidApkName = "demo.apk";

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
