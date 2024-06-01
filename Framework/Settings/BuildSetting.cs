using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class BuildSetting : ScriptableObjectSingleton<BuildSetting>
    {
        //��������
        private readonly static string AssetPath = "Assets/GameProject/Resources/BuildSettingAsset.asset";

        [Header("������ʱ�ļ����Ŀ¼,����ڹ���·��")]
        [SerializeField]
        public string BuildPath = "Build/";
        [SerializeField]
        public string LogPath = "Build/Logs/";
        [SerializeField]
        public string AssetBundleDir = "Build/AssetBundles/";

        [Header("BuildDataPath")]
        public string BuildDataPath = "Assets/GameProject/RuntimeAssets/BundleData_AB/";
        
        [Header("Exe���Ŀ¼")]
        [SerializeField]
        public string WinExeDir = "../Build/Win/";
        [SerializeField]
        public string WinExeName = "YaYa�ھ�ģ��.exe";
        [Header("Apk���Ŀ¼")]
        [SerializeField]
        public string AndroidApkDir = "../Build/Apk/";
        [SerializeField]
        public string AndroidApkName = "YaYa�ھ�ģ��.apk";

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
