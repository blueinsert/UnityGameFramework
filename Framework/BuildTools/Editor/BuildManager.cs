using bluebean.UGFramework.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
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

        public static bool IsAppendHashToAssetBundleName
        {
            get
            {
                return BuildSetting.Instance.IsAppendHashToAssetBundleName;
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
            HashSet<string> assetPathHashSet = new HashSet<string>();
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
                        assetPathHashSet.Add(dependence);
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
            if(assetPathHashSet.Count > 0){
                 fileWr.WriteLine("not int assetBundle assets,summary:");
                 foreach (var assetPath in assetPathHashSet)
                 {
                    fileWr.WriteLine(string.Format("    {0} ", assetPath));
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

        [MenuItem("Assets/FrameworkUtil/清空选择文件夹下的所有资源标签", false, 1)]
        [MenuItem("Framework/Build/清空选择文件夹下的所有资源标签", false)]
        private static void ClearAllAssetLabelsInFolder(){
           // 获取选中的文件夹路径
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("请选择一个文件夹！");
                return;
            }
            Debug.Log($"ClearAllAssetLabelsInFolder:{folderPath}");

             var pathList = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

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
        /// 生成资源包描述数据,放置在runtimeAsset本身
        /// </summary>
        [MenuItem("Framework/Build/Step2_GenerateBundleData")]
        public static void GenerateBundleData()
        {
            //Assets/GameProject/RuntimeAssets/BundleData_AB/BundleData.asset
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
        /// build所有的bundle,导出在外边目录
        /// </summary>
        [MenuItem("Framework/Build/Step3_BuildAssetBundles")]
        public static bool BuildAssetBundles()
        {
            //Build/AssetBundles/
            var dir = AssetBundleDir;
            if (!EditorUtility.PrepareDirectory(dir))
            {
                UnityEngine.Debug.LogErrorFormat("Failed to prepareDirectory: {0}", dir);
                return false;
            }
            BuildAssetBundleOptions options = BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression;
            if (IsAppendHashToAssetBundleName)
            {
                options = options | BuildAssetBundleOptions.AppendHashToAssetBundleName;
            }
            var manifest = BuildPipeline.BuildAssetBundles(dir,
                options,
                EditorUserBuildSettings.activeBuildTarget
               );
            if (manifest == null || manifest.GetAllAssetBundles() == null || manifest.GetAllAssetBundles().Length == 0)
            {
                UnityEngine.Debug.LogErrorFormat("BuildPipeline.BuildAssetBundles() {0}, {1}",
                   dir, EditorUserBuildSettings.activeBuildTarget);
                return false;
            }

            // 刷新编辑器
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("BuildAssetBundles complete");

            AssetBundleCycleReferenceDetector.DetectBundleCycles();
            if (AssetBundleCycleReferenceDetector.cycles.Count != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Build BundleData的ab
        /// </summary>
        /// <returns></returns>
        private static bool BuildBundleDataAssetBundle()
        {
            //Build/AssetBundles/
            var dir = AssetBundleDir;
           
            BuildAssetBundleOptions options = BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression;

            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            // 2. 配置要构建的AssetBundle
            buildMap[0].assetBundleName = "bundledata_ab.b"; // 资源包名称
                                                             // 明确指定要打入此资源包的所有资源路径
            string[] assets = new string[] {
                "Assets/GameProject/RuntimeAssets/BundleData_AB/BundleData.asset",
             };
            buildMap[0].assetNames = assets;

            var manifest = BuildPipeline.BuildAssetBundles(dir, buildMap, options, EditorUserBuildSettings.activeBuildTarget);

            if (manifest == null || manifest.GetAllAssetBundles() == null || manifest.GetAllAssetBundles().Length == 0)
            {
                UnityEngine.Debug.LogErrorFormat("BuildPipeline.BuildBundleDataAssetBundle() {0}, {1}",
                   dir, EditorUserBuildSettings.activeBuildTarget);
                return false;
            }

            // 刷新编辑器
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("BuildBundleDataAssetBundle complete");

            Hash128 hash;
            uint crc;
            var bundlePath = string.Format("{0}/{1}", AssetBundleDir, "bundledata_ab.b");
            if (!BuildPipeline.GetHashForAssetBundle(bundlePath, out hash))
            {
                Debug.LogError(string.Format("Failed to GetHashFor: {0}", bundlePath));
            }
            UnityEngine.Debug.Log($"BundleData_ab.b hash: {hash}");

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

                if (IsAppendHashToAssetBundleName)
                    data.m_realBundleName = AssetPathHelper.GetRealBundleName(data.m_bundleName, hash.ToString(), true);
                else
                    data.m_realBundleName = data.m_bundleName;
            }

            if (anyChange)
            {
                UnityEditor.EditorUtility.SetDirty(bundleData);
                AssetDatabase.SaveAssets();
                // 为了BundleData本身build一次bundle
                //if (!BuildBundleDataAssetBundle())
                if (!BuildAssetBundles())
                {
                    UnityEngine.Debug.LogError("BuildBundleDataAssetBundle() Failed!");

                    UnityEngine.Debug.LogError("UpdateBundleData4BundleVersion() Failed to build asset bundle.");
                    return false;
                }
                if (IsAppendHashToAssetBundleName)
                {
                    Hash128 hash;
                    bundlePath = string.Format("{0}/{1}", AssetBundleDir, "bundledata_ab.b");
                    if (!BuildPipeline.GetHashForAssetBundle(bundlePath, out hash))
                    {
                        Debug.LogError(string.Format("Failed to GetHashFor: {0}", bundlePath));
                    }
                    UnityEngine.Debug.Log($"BundleData_ab.b hash: {hash}");
                    File.WriteAllText(Path.Combine(AssetBundleDir, "BundleData.json"), $"bundledata_ab_{hash}.b");
                }else
                {
                    File.WriteAllText(Path.Combine(AssetBundleDir, "BundleData.json"), $"bundledata_ab.b");
                }
            }

            UnityEngine.Debug.Log("UpdateBundleData4BundleVersion completed");
            return true;
        }

        /// <summary>
        /// 将asset bundles从外部目录复制回StreamingAssets目录
        /// </summary>
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
            Debug.Assert(bundleData != null, "bundleData != null"); 
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
            if(!BuildAllAssets())
            {
                Debug.LogError("Build Failed");
                return;
            }
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
            if (!BuildAllAssets())
            {
                Debug.LogError("Build Failed");
                return;
            }
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

        [MenuItem("Framework/Build/BuildWebGL_AssetBundles")]
        static void BuildWebGL()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            Debug.Log(string.Format("Start BuildWebGL_AssetBundles,{0}", EditorUserBuildSettings.activeBuildTarget));
            if (!BuildAllAssets())
            {
                Debug.LogError("Build Failed");
                return;
            }
            CopyAssetBundels2StreamingAssets();
           
            Debug.Log(string.Format("BuildWebGL_AssetBundles Success!"));

        }
    }
}
