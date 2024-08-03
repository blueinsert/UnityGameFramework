using bluebean.UGFramework.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace bluebean.UGFramework.Build
{
    public enum BundleDirType
    {
        None,
        AB,    // 会放到streamingAssets目录中
    }

    public static class BuildManager
    {
        public static string LogPath
        {
            get {
                return BuildSetting.Instance.LogPath;
            }
        }

        public static string BundleDataPath
        {
            get
            {
                return BuildSetting.Instance.BuildDataPath + "BundleData.asset";
            }
        }

        public static string AssetBundleDir
        {
            get
            {
                return BuildSetting.Instance.AssetBundleDir + EditorUserBuildSettings.activeBuildTarget;
            }
        }

        public const string StreamingAssetsBundlePath = "Assets/StreamingAssets/AssetBundles/";

        /// <summary>
        /// 获取目录的bundle类型
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BundleDirType GetBundleDirType(string path)
        {
            if (path.EndsWith("_AB", true, null))
            {
                return BundleDirType.AB;
            }
            else
            {
                return BundleDirType.None;
            }
        }

        /// <summary>
        /// 为指定资源path设置bundlename
        /// </summary>
        private static void SetupAssetLabelInfo(string assetPath, string bundleName,
            string bundleVariant = "")
        {
            // 获取该路径资源对应的importSetting对象
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                UnityEngine.Debug.LogError("SetupAssetLabelInfo: Fail to Set AssetLabelInfo, Set Path: " + assetPath);
                return;
            }

            // 设置资源所属bundle
            if (importer.assetBundleName != bundleName ||
                importer.assetBundleVariant != bundleVariant)
            {
                importer.assetBundleName = bundleName;
                importer.assetBundleVariant = bundleVariant;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// 查看bundle中的资源引用的文件是否都在bundle中
        /// </summary>
        /// <param name="assetPathList"></param>
        /// <returns></returns>
        public static bool CheckAssetBundleAsset(List<string> assetPathList)
        {
            bool isright = true;
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            FileInfo fileResult = new FileInfo(LogPath + "BundleAssetCheckResult.txt");
            StreamWriter fileWr = fileResult.CreateText();
            foreach (var name in assetPathList)
            {
                // 排除场景文件
                if (name.EndsWith(".unity"))
                    continue;

                List<string> errorList = new List<string>();
                // 只看直接引用
                foreach (var dependence in AssetDatabase.GetDependencies(name, false))
                {
                    // 资源存在     
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dependence);

                    // 排除cs文件，dll，和shader
                    if (obj == null || obj as MonoScript != null || obj as Shader != null || obj as DefaultAsset != null)
                    {
                        continue;
                    }

                    var importer = AssetImporter.GetAtPath(dependence);
                    if (importer != null && string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        errorList.Add(dependence);
                    }
                }

                if (errorList.Count > 0)
                {
                    isright = false;
                    fileWr.WriteLine(string.Format("[{0} dependence obj not in any bundle:", name));
                    foreach (var error in errorList)
                    {
                        fileWr.WriteLine(string.Format("    {0} ", error));
                    }
                    fileWr.WriteLine("]");
                }
            }
            fileWr.Close();

            if (!isright)
            {
                Debug.LogError("Error:Not all needed asset in bundle,show detail in BundleAssetCheckResult.txt");
            }

            return isright;
        }

        /// <summary>
        /// 搜索所有路径,对文件夹和文件设置bundle标签信息
        /// </summary>
        [MenuItem("Framework/Build/Step1_SetAllAssetBundleLabels")]
        public static bool SetAllAssetBundleLabels()
        {
            var pathList = Directory.GetDirectories("Assets/GameProject/RuntimeAssets", "*", SearchOption.AllDirectories);

            List<string> assetPathList = new List<string>();
            for (int index = 0; index < pathList.Length; ++index)
            {
                var path = pathList[index];

                UnityEditor.EditorUtility.DisplayProgressBar("TickBuildProcess bundle info in asset database...", path, (float)(index + 1) / pathList.Length);

                if (GetBundleDirType(path) != BundleDirType.None)
                {
                    // 出现ooxx_AB/ooxx2_AB/这样的路径，需要特殊处理
                    if (path.ToUpper().IndexOf("_AB") < path.Length - 5)
                    {
                        UnityEngine.Debug.LogError("Can not set _AB int dir tree multi times " + path);
                        continue;
                    }

                    // 获取bundle名字和bundle变体的名字           
                    string bundleName;
                    string bundleVariant;

                    bundleName = AssetBundleUtility.GetBundleNameByAssetPath(path);
                    bundleVariant = "";

                    // 遍历每一个文件，非.meta结尾的文件设置bundle name
                    string[] fileNames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach (string name in fileNames)
                    {
                        // .meta结尾,(DS_Store mac 下的系统文件),跳过
                        if (name.EndsWith(".meta") || name.EndsWith("DS_Store"))
                        {
                            continue;
                        }

                        SetupAssetLabelInfo(name, bundleName, bundleVariant);
                        assetPathList.Add(name);
                    }
                }
                else
                {
                    SetupAssetLabelInfo(path, "");
                }
            }

            //刷新编辑器
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.RemoveUnusedAssetBundleNames();
            UnityEngine.Debug.Log("SetAllAssetBundleLabels completed");
            UnityEditor.EditorUtility.ClearProgressBar();
            return CheckAssetBundleAsset(assetPathList);
        }

        [MenuItem("Framework/Build/ClearAllAssetBundleLabels")]
        public static void ClearAllAssetBundleLabels()
        {
            var pathList = Directory.GetDirectories("Assets/GameProject/RuntimeAssets", "*", SearchOption.AllDirectories);

            foreach (var path in pathList)
            {
                UnityEngine.Debug.Log(string.Format("ClearAssetLabelsInFolder {0}", path));
                foreach (string name in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    // .meta结尾,(DS_Store mac 下的系统文件),跳过
                    if (name.EndsWith(".meta") || name.EndsWith("DS_Store"))
                    {
                        continue;
                    }
                    SetupAssetLabelInfo(name, "");
                }
            }


            //刷新编辑器
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.RemoveUnusedAssetBundleNames();
            UnityEngine.Debug.Log("ClearAllAssetBundleLabels completed");
        }

        /// <summary>
        /// 生成资源包描述数据
        /// </summary>
        [MenuItem("Framework/Build/Step2_GenerateBundleData")]
        public static void GenerateBundleData()
        {
            var bundleDataPath = BundleDataPath;
            var obj = AssetDatabase.LoadAssetAtPath<BundleData>(bundleDataPath);
            var bundleData = obj as BundleData;
            if(bundleData == null)
            {
                bundleData = EditorUtility.CreateScriptableObjectAsset<BundleData>(bundleDataPath);
            }
            bundleData.m_bundleList.Clear();
            // 获取所有的bundle名称
            var bundleNames = new List<string>(AssetDatabase.GetAllAssetBundleNames());

            // 通过AssetDatabase遍历所有的bundle,填充m_assetList
            foreach (var bundleName in bundleNames)
            {
                // 跳过bundledata自己
                if (bundleName.IndexOf("BundleData_AB".ToLower()) != -1)
                {
                    continue;
                }

                BundleData.SingleBundleData data = new BundleData.SingleBundleData();
                // 将SingleBundleData添加到列表中 
                bundleData.m_bundleList.Add(data);

                data.m_bundleName = bundleName;
                // 获取bundle不带后缀的名字和后缀
                int index = bundleName.LastIndexOf(".", StringComparison.Ordinal);
                string bundleNameWithoutExtension;
                string bundleNameExtension;
                if (index > 0)
                {
                    bundleNameWithoutExtension = bundleName.Substring(0, index);
                    bundleNameExtension = bundleName.Substring(index + 1, bundleName.Length - index - 1);
                }
                else
                {
                    bundleNameWithoutExtension = bundleName;
                    bundleNameExtension = "";
                }

                // 获取一个bundle包含的所有资源
                List<string> tempAssetPathList = new List<string>();

                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

                foreach (var path in assetPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            // .meta结尾,(DS_Store mac 下的系统文件),跳过
                            if (file.EndsWith(".meta") || file.EndsWith("DS_Store"))
                            {
                                continue;
                            }
                            tempAssetPathList.Add(file.Replace(Application.dataPath, "").Replace('\\', '/'));
                        }
                    }
                    else
                    {
                        tempAssetPathList.Add(path);
                    }
                }


                // 为一个bundle填充asset包含数据
                data.m_assetList.Clear();
                data.m_assetList.AddRange(tempAssetPathList);
                data.m_assetList.Sort();
            }

            // 对列表排序,避免排序不稳定带来的内容变化
            bundleData.m_bundleList.Sort((b1, b2) => { return String.CompareOrdinal(b1.m_bundleName, b2.m_bundleName); });

            // 保存
            UnityEditor.EditorUtility.SetDirty(bundleData);
            AssetDatabase.SaveAssets();

            // 设置bundleData自身的assetLabel
            var bundleNameForBundleData = AssetBundleUtility.GetBundleNameByAssetPath("BundleData_AB");
            SetupAssetLabelInfo(bundleDataPath, bundleNameForBundleData);
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("GenerateBundleData completed");
        }

        /// <summary>
        /// build所有的bundle
        /// </summary>
        [MenuItem("Framework/Build/Step3_BuildAssetBundles")]
        public static bool BuildAssetBundles()
        {
            var dir = AssetBundleDir;
            if (!EditorUtility.PrepareDirectory(dir))
            {
                UnityEngine.Debug.LogErrorFormat("Failed to prepareDirectory: {0}", dir);
                return false;
            }

            var manifest = BuildPipeline.BuildAssetBundles(dir,
                BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
            if (manifest == null || manifest.GetAllAssetBundles() == null || manifest.GetAllAssetBundles().Length == 0)
            {
                UnityEngine.Debug.LogErrorFormat("BuildPipeline.BuildAssetBundles() {0}, {1}",
                   dir, EditorUserBuildSettings.activeBuildTarget);
                return false;
            }

            // 刷新编辑器
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("BuildAssetBundles complete");

            return true;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetFileSize(string path)
        {
            if (!File.Exists(path))
                return 0;
            var fi = new FileInfo(path);
            return fi.Length;
        }

        /// <summary>
        /// 更新bundledata中的版本信息
        /// 需要在产生AssetBundles之后进行
        /// </summary>
        [MenuItem("Framework/Build/Step4_UpdateBundleDataAfterGenerated")]
        public static bool UpdateBundleDataAfterGenerated()
        {
            var bundleDataPath = BundleDataPath;
            var assetBundlesDir = AssetBundleDir;
            
            bool anyChange = false;
            var bundleData = AssetDatabase.LoadAssetAtPath(bundleDataPath, typeof(BundleData)) as BundleData;
            string bundlePath;

            System.Diagnostics.Debug.Assert(bundleData != null, "bundleData != null");
            // 遍历所有bundle
            foreach (var data in bundleData.m_bundleList)
            {
                // 如果hash产生变化，说明版本更新
                Hash128 hash;
                uint crc;
                bundlePath = string.Format("{0}/{1}", assetBundlesDir, data.m_bundleName);
                if (!BuildPipeline.GetHashForAssetBundle(bundlePath, out hash))
                {
                    Debug.LogError(string.Format("Failed to GetHashFor: {0}", bundlePath));
                    continue;
                }
                if (!BuildPipeline.GetCRCForAssetBundle(bundlePath, out crc))
                {
                    Debug.LogError(string.Format("Failed to GetCRCFor: {0}", bundlePath));
                    continue;
                }

                if (String.CompareOrdinal(data.m_bundleHash, hash.ToString()) != 0 ||
                    data.m_bundleCRC != crc)
                {
                    data.m_bundleHash = hash.ToString();
                    data.m_bundleCRC = crc;
                    data.m_size = GetFileSize(bundlePath);
                    anyChange = true;
                }
            }

            if (anyChange)
            {
                UnityEditor.EditorUtility.SetDirty(bundleData);
                AssetDatabase.SaveAssets();
                // 为了BundleData本身build一次bundle todo remove
                if (!BuildAssetBundles())
                {
                    UnityEngine.Debug.LogError("UpdateBundleData4BundleVersion() Failed to build asset bundle.");
                    return false;
                }
            }

            UnityEngine.Debug.Log("UpdateBundleData4BundleVersion completed");
            return true;
        }

        [MenuItem("Framework/Build/Step5_CopyAssetBundels2StreamingAssets")]
        static void CopyAssetBundels2StreamingAssets()
        {
          
            string sourcePath = null;
            string targetPath = null;
           
            var assetBundlesDir = AssetBundleDir;
            EditorUtility.PrepareDirectory(StreamingAssetsBundlePath);
            
            foreach (var filePath in Directory.GetFiles(assetBundlesDir))
            {
                sourcePath = filePath;
                targetPath = string.Format("{0}/{1}", StreamingAssetsBundlePath, Path.GetFileName(sourcePath));
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetPath, true);
                    UnityEngine.Debug.Log(string.Format("Copy {0} => {1}", sourcePath, targetPath));
                }
            }

            var bundleDataPath = BundleDataPath;
            var bundleData = AssetDatabase.LoadAssetAtPath(bundleDataPath, typeof(BundleData)) as BundleData;
            System.Diagnostics.Debug.Assert(bundleData != null, "bundleData != null");
            sourcePath = string.Format("{0}", bundleDataPath);
            targetPath = string.Format("{0}/{1}", StreamingAssetsBundlePath, "BundleData.asset");
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, targetPath, true);
                UnityEngine.Debug.Log(string.Format("Copy {0} => {1}", sourcePath, targetPath));
            }
            UnityEngine.Debug.Log(string.Format("Copy Complete!"));
            AssetDatabase.Refresh();
        }

        private static bool BuildAllAssets()
        {
            SetAllAssetBundleLabels();
            GenerateBundleData();
            if (!BuildAssetBundles())
            {
                return false;
            }
            if (!UpdateBundleDataAfterGenerated())
            {
                UnityEngine.Debug.LogErrorFormat("Failed to UpdateBundleData4BundleVersion.");
                return false;
            }
            return true;
        }

        [MenuItem("Framework/Build/BuildWindowsExe")]
        static void BuildWindowsExe()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            Debug.Log(string.Format("Start BuildWindowsExe,{0}", EditorUserBuildSettings.activeBuildTarget));
            BuildAllAssets();
            CopyAssetBundels2StreamingAssets();
            var buildSetting = BuildSetting.Instance;
            var outputDir = string.Format("{0}/{1}/{2}", buildSetting.WinExeDir, EditorUserBuildSettings.activeBuildTarget, buildSetting.WinExeName);
            string[] scenes = { "Assets/GameProject/Resources/AppEntry.unity" };
            BuildOptions o = BuildOptions.StrictMode;
            var result = BuildPipeline.BuildPlayer(scenes, outputDir, BuildTarget.StandaloneWindows, o);
            if (result!=null)
            {
                Debug.Log(string.Format("BuildWindowsExe BuildPipeline.BuildPlay finish: {0}",
                    result.summary.ToString()));
              
                return;
            }
        }

        [MenuItem("Framework/Build/BuildAndroidApk")]
        static void BuildAndroidApk()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            Debug.Log(string.Format("Start BuildAndroidApk,{0}", EditorUserBuildSettings.activeBuildTarget));
            BuildAllAssets();
            CopyAssetBundels2StreamingAssets();
            var buildSetting = BuildSetting.Instance;
            var outputDir = string.Format("{0}/{1}/{2}", buildSetting.WinExeDir, EditorUserBuildSettings.activeBuildTarget, buildSetting.AndroidApkName);
            string[] scenes = { "Assets/GameProject/Resources/AppEntry.unity" };
            BuildOptions o = BuildOptions.StrictMode;
            try
            {
                var result = BuildPipeline.BuildPlayer(scenes,
                                        outputDir,
                                        BuildTarget.Android, o);
                if (result != null)
                {
                    Debug.Log(string.Format("BuildAndroidApk BuildPipeline.BuildPlay finish: {0}",
                        result.summary.ToString()));

                    return;
                }
            } catch (Exception e)
            {
                Debug.LogError(string.Format("BuildAndroidApk BuildPipeline.BuildPlay failed, error: {0}",
                       e.ToString()));
            }
            Debug.Log(string.Format("BuildAndroidApk Success!"));

        }
    }
}
