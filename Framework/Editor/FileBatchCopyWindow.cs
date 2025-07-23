using UnityEngine;
using UnityEditor;
using System.IO;

namespace bluebean.UGFramework
{
    public class FileBatchCopyWindow : EditorWindow
    {
        private string filePathsText = "";
        private string targetFolderPath = "";
        private Vector2 scrollPos;
        private bool isCut = false; // 是否剪切

        [MenuItem("Framework/Tools/批量文件复制工具")]
        public static void ShowWindow()
        {
            GetWindow<FileBatchCopyWindow>("批量文件复制");
        }

        private void OnGUI()
        {
            GUILayout.Label("源文件路径（每行一个，支持绝对或Assets/相对路径）", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(120));
            filePathsText = EditorGUILayout.TextArea(filePathsText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            GUILayout.Label("目标文件夹路径（绝对路径或Assets/相对路径）", EditorStyles.boldLabel);
            targetFolderPath = EditorGUILayout.TextField(targetFolderPath);

            GUILayout.Space(10);
            isCut = EditorGUILayout.Toggle("是否剪切（移动）文件", isCut);

            GUILayout.Space(20);
            if (GUILayout.Button("复制文件到目标文件夹", GUILayout.Height(30)))
            {
                CopyFiles();
            }
        }

        private void CopyFiles()
        {
            if (string.IsNullOrEmpty(filePathsText) || string.IsNullOrEmpty(targetFolderPath))
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", "请填写源文件路径和目标文件夹路径！", "确定");
                return;
            }

            string[] lines = filePathsText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            int success = 0, fail = 0;
            string absTargetFolder = targetFolderPath;
            if (targetFolderPath.StartsWith("Assets"))
                absTargetFolder = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + targetFolderPath;
            if (!Directory.Exists(absTargetFolder))
            {
                Directory.CreateDirectory(absTargetFolder);
            }

            foreach (var line in lines)
            {
                string src = line.Trim();
                if (string.IsNullOrEmpty(src)) continue;
                string absSrc = src;
                if (src.StartsWith("Assets"))
                    absSrc = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + src;
                if (!File.Exists(absSrc))
                {
                    Debug.LogWarning($"文件不存在: {src}");
                    fail++;
                    continue;
                }
                string fileName = Path.GetFileName(absSrc);
                string dst = Path.Combine(absTargetFolder, fileName);
                try
                {
                    if (isCut)
                    {
                        if (File.Exists(dst))
                            File.Delete(dst);
                        File.Move(absSrc, dst);
                        // 检查并移动.meta文件
                        string metaSrc = absSrc + ".meta";
                        string metaDst = dst + ".meta";
                        if (File.Exists(metaSrc))
                        {
                            if (File.Exists(metaDst))
                                File.Delete(metaDst);
                            File.Move(metaSrc, metaDst);
                        }
                    }
                    else
                    {
                        File.Copy(absSrc, dst, true);
                    }
                    success++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"{(isCut ? "剪切" : "复制")}失败: {src} -> {dst}\n{ex.Message}");
                    fail++;
                }
            }
            AssetDatabase.Refresh();
            UnityEditor.EditorUtility.DisplayDialog("完成", $"{(isCut ? "剪切" : "复制")}成功: {success} 个，失败: {fail} 个。", "确定");
        }
    }
}