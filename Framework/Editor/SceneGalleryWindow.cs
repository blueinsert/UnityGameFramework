#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

namespace bluebean.UGFramework
{
    public class SceneGalleryWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private List<string> scenePaths = new List<string>();
        private List<Texture2D> sceneIcons = new List<Texture2D>();
        private int iconSize = 64;
        private int minIconSize = 32;
        private int maxIconSize = 256;
        private int padding = 12;
        private int labelHeight = 18;
        private bool isBatchGenerating = false;
        private string batchStatus = "";
        private int batchIndex = 0;
        private string originalScenePath = null;
        private string searchText = "";

         #region 单例模式
        private static SceneGalleryWindow m_instance;
        public static SceneGalleryWindow Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GetWindow<SceneGalleryWindow>(typeof(SceneGalleryWindow).Name);
                    m_instance.maxSize = new Vector2(480, 320);
                }
                return m_instance;
            }
        }
        #endregion


        [MenuItem("Framework/Tools/SceneGallery")]
        public static void ShowWindow()
        {
             Instance.Show();
        }

        private void OnEnable()
        {
            RefreshScenes();
        }

        private void RefreshScenes()
        {
            scenePaths.Clear();
            sceneIcons.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                scenePaths.Add(path);

                // 自动缩略图支持
                string sceneName = Path.GetFileNameWithoutExtension(path);
                string thumbPath = $"Assets/SceneThumbnails/{sceneName}.png";
                Texture2D thumb = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbPath);
                if (thumb != null)
                    sceneIcons.Add(thumb);
                else
                    sceneIcons.Add(EditorGUIUtility.FindTexture("SceneAsset Icon"));
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            // 搜索框
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("搜索", GUILayout.Width(40));
            string newSearch = EditorGUILayout.TextField(searchText, GUILayout.Width(200));
            if (newSearch != searchText)
            {
                searchText = newSearch;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            // 缩略图大小滑杆
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("缩略图大小", GUILayout.Width(60));
            iconSize = (int)GUILayout.HorizontalSlider(iconSize, minIconSize, maxIconSize, GUILayout.Width(150));
            GUILayout.Label(iconSize.ToString(), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("刷新场景列表", GUILayout.Height(24)))
            {
                RefreshScenes();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("自动批量生成场景缩略图", GUILayout.Height(24)) && !isBatchGenerating)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("批量生成场景缩略图", "此操作将依次打开所有场景并截图，过程会自动保存并在结束后回到当前场景。\n\n是否继续？", "确定", "取消"))
                {
                    EditorApplication.update += BatchGenerateThumbnails;
                    isBatchGenerating = true;
                    batchStatus = "正在生成缩略图...";
                    batchIndex = 0;
                    originalScenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
                }
            }
            if (isBatchGenerating)
            {
                EditorGUILayout.HelpBox(batchStatus, MessageType.Info);
            }
            // 过滤场景
            List<int> filteredIdx = new List<int>();
            for (int i = 0; i < scenePaths.Count; i++)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePaths[i]);
                if (string.IsNullOrEmpty(searchText) || sceneName.ToLower().Contains(searchText.ToLower()))
                {
                    filteredIdx.Add(i);
                }
            }
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (iconSize <= 48)
            {
                // 列表模式
                for (int f = 0; f < filteredIdx.Count; f++)
                {
                    int idx = filteredIdx[f];
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(sceneIcons[idx], GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                    {
                        if (UnityEditor.EditorUtility.DisplayDialog("打开场景", $"确定要打开场景：{Path.GetFileNameWithoutExtension(scenePaths[idx])} 吗？\n当前场景如有未保存内容将提示保存。", "确定", "取消"))
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                EditorSceneManager.OpenScene(scenePaths[idx]);
                            }
                        }
                    }
                    GUILayout.Label(Path.GetFileName(scenePaths[idx]), GUILayout.Height(iconSize), GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }
            }
            else
            {
                // 网格模式
                int columns = Mathf.Max(1, (int)((position.width - padding) / (iconSize + padding)));
                int rows = Mathf.CeilToInt((float)filteredIdx.Count / columns);
                int idx = 0;
                for (int row = 0; row < rows; row++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(padding);
                    for (int col = 0; col < columns && idx < filteredIdx.Count; col++, idx++)
                    {
                        int realIdx = filteredIdx[idx];
                        GUILayout.BeginVertical(GUILayout.Width(iconSize));
                        if (GUILayout.Button(sceneIcons[realIdx], GUILayout.Width(iconSize), GUILayout.Height(iconSize)))
                        {
                            if (UnityEditor.EditorUtility.DisplayDialog("打开场景", $"确定要打开场景：{Path.GetFileNameWithoutExtension(scenePaths[realIdx])} 吗？\n当前场景如有未保存内容将提示保存。", "确定", "取消"))
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.OpenScene(scenePaths[realIdx]);
                                }
                            }
                        }
                        string sceneName = Path.GetFileNameWithoutExtension(scenePaths[realIdx]);
                        GUILayout.Label(sceneName, EditorStyles.miniLabel, GUILayout.Width(iconSize), GUILayout.Height(labelHeight));
                        GUILayout.EndVertical();
                        GUILayout.Space(padding);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(padding / 2);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void BatchGenerateThumbnails()
        {
            if (batchIndex >= scenePaths.Count)
            {
                // 回到原场景
                if (!string.IsNullOrEmpty(originalScenePath))
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalScenePath);
                EditorApplication.update -= BatchGenerateThumbnails;
                isBatchGenerating = false;
                batchStatus = "缩略图生成完成！";
                RefreshScenes();
                return;
            }
            string scenePath = scenePaths[batchIndex];
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            // 等待一帧再截图
            EditorApplication.delayCall += () => {
                TakeSceneThumbnail(scenePath);
                batchIndex++;
            };
        }
        void TakeSceneThumbnail(string scenePath)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                // 找不到主摄像机，跳过
                batchStatus = $"{Path.GetFileName(scenePath)}: 无主摄像机，跳过";
                return;
            }
            int width = 256;
            int height = 144;
            RenderTexture rt = new RenderTexture(width, height, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            // 保存图片
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            string dir = "Assets/SceneThumbnails";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, sceneName + ".png");
            File.WriteAllBytes(filePath, screenShot.EncodeToPNG());
            AssetDatabase.ImportAsset(filePath);
            batchStatus = $"{sceneName} 缩略图已生成";
        }
    }
}
#endif