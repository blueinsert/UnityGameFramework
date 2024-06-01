using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using System.Threading;
using bluebean.UGFramework.Asset;

namespace bluebean.UGFramework.ConfigData
{

    public class ConfigDataEditorWindow : EditorWindow
    {

        [MenuItem("Framework/ConfigData/OpenWindow")]
        public static void OpenConfigDataEditorWindow()
        {
            Instance.Show();
        }

        public ConfigDataSetting ConfigDataSetting
        {
            get
            {
                return GameClientSetting.Instance.m_configDataSetting;
            }
        }

        #region 单例模式
        private static ConfigDataEditorWindow m_instance;
        public static ConfigDataEditorWindow Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GetWindow<ConfigDataEditorWindow>(typeof(ConfigDataEditorWindow).Name);
                    m_instance.maxSize = new Vector2(480, 320);
                }
                return m_instance;
            }
        }
        #endregion

        /// <summary>
        /// 配置表信息字典
        /// </summary>
        private CoroutineScheduler m_coroutineManager = new CoroutineScheduler();

        void OnEnable()
        {
        }

        private void OnDestroy()
        {
            Debug.Log("ConfigDataEditorWindow OnDestroy");
        }

        private void Update()
        {
            m_coroutineManager.Tick();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("BuildAll"))
            {
                m_coroutineManager.StartCorcoutine(BuildAll());
            }
            if (GUILayout.Button("RunTest"))
            {
                RunTest();
            }
        }

        private IEnumerator BuildAll()
        {
            UnityEditor.EditorUtility.DisplayProgressBar("BuildAll", "CopyCodeFiles", 0.1f);
            CopyTypeDefineCodes2Project();
            UnityEditor.EditorUtility.DisplayProgressBar("BuildAll", "CopyCodeFiles", 0.5f);
            ImportConfigDataAssetFiles();
            UnityEditor.EditorUtility.DisplayProgressBar("BuildAll", "CopyCodeFiles", 1.0f);
            yield return null;
            UnityEditor.EditorUtility.ClearProgressBar();
            yield return null;
            AssetDatabase.Refresh();
            Debug.Log("BuildAll Success!");
            ShowNotification(new GUIContent("BuildAll Success!"));
        }

        public void StartBuildAll()
        {
            m_coroutineManager.StartCorcoutine(BuildAll());
        }

        private void CopyTypeDefineCodes2Project()
        {
            var srcPath = ConfigDataSetting.m_sourceCodeFilePath;
            if (!Directory.Exists(srcPath))
            {
                Debug.LogError(string.Format("源代码文件目录{0}不存在", srcPath));
                return;
            }
            var tarPath = ConfigDataSetting.m_targetCodeFilePath;
            if (!Directory.Exists(tarPath))
            {
                Debug.LogError(string.Format("目标代码文件目录{0}不存在", tarPath));
                return;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(srcPath);
            //DirectoryInfo tarDirectoryInfo = new DirectoryInfo(tarPath);
            foreach (var fileInfo in directoryInfo.GetFiles("*.cs"))
            {
                File.Copy(fileInfo.FullName, string.Format("{0}{1}{2}", ConfigDataSetting.m_targetCodeFilePath, "/", fileInfo.Name),true);
            }
        }

        private void ImportConfigDataAssetFiles()
        {
            var srcPath = ConfigDataSetting.m_sourceSerializedFilePath;
            if (!Directory.Exists(srcPath))
            {
                Debug.LogError(string.Format("源序列化资源文件目录{0}不存在", srcPath));
                return;
            }
            var tarPath = ConfigDataSetting.m_targetSerializedFilePath;
            if (Directory.Exists(tarPath))
            {
                Directory.Delete(tarPath, true);
            }
            Directory.CreateDirectory(tarPath);
            DirectoryInfo directoryInfo = new DirectoryInfo(srcPath);
            //DirectoryInfo tarDirectoryInfo = new DirectoryInfo(tarPath);
            foreach (var fileInfo in directoryInfo.GetFiles("*"))
            {
                var data = File.ReadAllBytes(fileInfo.FullName);
                //将bytes[]包装进AssetObject
                AssetObject bytesScriptableObjectMD5 = ScriptableObject.CreateInstance<AssetObject>();
                bytesScriptableObjectMD5.m_bytes = data;
                bytesScriptableObjectMD5.m_MD5 = "undefined";//todo
                AssetDatabase.CreateAsset(bytesScriptableObjectMD5, tarPath + "/" +   PathUtility.RemoveExtension(fileInfo.Name) + ".asset");
            }
        }

        private void RunTest()
        {
            if(AssetLoader.Instance == null)
            {
                AssetLoader.CreateInstance();
            }
            if(ConfigDataLoader.Instance == null)
            {
                ConfigDataLoader.CreateInstance();
            }
            m_coroutineManager.StartCorcoutine(ConfigDataLoader.Instance.LoadAllConfigData((res) =>
            {
                if (res)
                    ShowNotification(new GUIContent("RunTest OK"));
            }));
        }
    }
}
