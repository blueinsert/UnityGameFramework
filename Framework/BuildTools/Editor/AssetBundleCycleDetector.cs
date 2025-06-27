using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Build
{
    public static class AssetBundleCycleReferenceDetector
    {

        public static readonly List<List<string>> cycles = new List<List<string>>();
        public static readonly Dictionary<string, List<string>> bundleDependencies = new Dictionary<string, List<string>>();

        [MenuItem("Framework/Build/检测资源循环引用")]
        public static void DetectBundleCycles()
        {
            cycles.Clear();
            bundleDependencies.Clear();

            try
            {
                // Get all asset bundles
                string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
                if (allBundles.Length == 0)
                {
                    Debug.LogWarning("No asset bundles found in project");
                    return;
                }

                // Build dependency graph between bundles
                Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();
                Dictionary<string, HashSet<string>> bundleAssets = new Dictionary<string, HashSet<string>>();

                // Step 1: Map assets to their bundles
                foreach (string bundle in allBundles)
                {
                    string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
                    bundleAssets[bundle] = new HashSet<string>(assets);
                }

                // Step 2: Build bundle dependency graph
                //方法一：通过AssetDatabase.GetAssetBundleDependencies(bundle, false)直接获得
                foreach (var bundle in allBundles)
                {
                    // Get all dependencies recursively
                    string[] directDependencies = AssetDatabase.GetAssetBundleDependencies(bundle, false);
                    bundleDependencies[bundle] = directDependencies.ToList();
                    graph[bundle] = directDependencies.ToList();
                }
                //方法一：遍历当前包的所有资源，查找每个资源的直接依赖包
                //优点：更加精细可输出更多debug信息
                if (false)
                {
                    foreach (string bundle in allBundles)
                    {
                        graph[bundle] = new List<string>();
                        HashSet<string> directDependencies = new HashSet<string>();

                        foreach (string asset in bundleAssets[bundle])
                        {
                            string[] dependencies = AssetDatabase.GetDependencies(asset, false);
                            foreach (string dep in dependencies)
                            {
                                // Skip self and meta files
                                if (dep == asset || dep.EndsWith(".cs") || dep.EndsWith(".shader"))
                                    continue;

                                // Find which bundle this dependency belongs to
                                string depBundle = AssetDatabase.GetImplicitAssetBundleName(dep);
                                if (string.IsNullOrEmpty(depBundle))
                                    continue;

                                // Skip dependencies within the same bundle
                                if (depBundle != bundle)
                                {
                                    directDependencies.Add(depBundle);
                                }
                            }
                        }

                        // Add direct dependencies
                        graph[bundle] = directDependencies.ToList();
                        bundleDependencies[bundle] = directDependencies.ToList();
                    }
                }

                // Step 3: Detect cycles in bundle dependency graph
                Dictionary<string, int> visited = new Dictionary<string, int>();
                Dictionary<string, string> parentMap = new Dictionary<string, string>();

                foreach (string bundle in graph.Keys)
                {
                    visited[bundle] = 0;
                }

                foreach (string bundle in graph.Keys)
                {
                    if (visited[bundle] == 0)
                    {
                        DFSBundle(bundle, graph, visited, parentMap);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Cycle-bundle cycle detection failed: {e.Message}\n{e.StackTrace}");
            }
            StringBuilder sb = new StringBuilder();
            if (cycles.Count > 0)
            {
                foreach (var cycle in cycles)
                {
                    for (int i = 0; i < cycle.Count; i++)
                    {
                        string arrow = i < cycle.Count - 1 ? "→" : "↺";
                        sb.Append($"{cycle[i]} {arrow}");
                    }
                    sb.Append('\n');
                }
                Debug.LogError($"存在循环引用：\n{sb.ToString()}");
            }else
            {
                Debug.Log($"未检查到循环引用");
            }
        }

        public static void DFSBundle(string current,
                              Dictionary<string, List<string>> graph,
                              Dictionary<string, int> visited,
                              Dictionary<string, string> parentMap)
        {
            visited[current] = 1;  // Mark as visiting

            if (!graph.ContainsKey(current)) return;

            foreach (string neighbor in graph[current])
            {
                if (!graph.ContainsKey(neighbor)) continue;

                if (visited[neighbor] == 0) // Not visited
                {
                    parentMap[neighbor] = current;
                    DFSBundle(neighbor, graph, visited, parentMap);
                }
                else if (visited[neighbor] == 1) // Cycle detected
                {
                    RecordBundleCycle(current, neighbor, parentMap);
                }
            }

            visited[current] = 2; // Mark as visited
        }

        public static void RecordBundleCycle(string start, string end, Dictionary<string, string> parentMap)
        {
            List<string> cycle = new List<string>();
            string node = start;

            // Backtrack to build cycle
            while (node != end)
            {
                cycle.Add(node);
                node = parentMap[node];
            }
            cycle.Add(end);
            cycle.Add(start); // Close the cycle
            cycle.Reverse();

            // Add to results (avoid duplicates)
            if (!CycleExists(cycle))
            {
                cycles.Add(cycle);
            }
        }

        public static bool CycleExists(List<string> newCycle)
        {
            // Normalize the cycle by rotating to start with smallest name
            int minIndex = 0;
            string minName = newCycle[0];
            for (int i = 1; i < newCycle.Count; i++)
            {
                if (string.Compare(newCycle[i], minName) < 0)
                {
                    minName = newCycle[i];
                    minIndex = i;
                }
            }

            List<string> normalized = new List<string>();
            for (int i = 0; i < newCycle.Count; i++)
            {
                normalized.Add(newCycle[(minIndex + i) % newCycle.Count]);
            }

            // Check against existing cycles
            foreach (var existing in cycles)
            {
                if (existing.Count != normalized.Count) continue;

                bool match = true;
                for (int i = 0; i < existing.Count; i++)
                {
                    if (existing[i] != normalized[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match) return true;
            }

            return false;
        }
    }

    public class AssetBundleCycleReferenceDetectorWindow : EditorWindow
    {
        private List<List<string>> cycles
        {
            get
            {
                return AssetBundleCycleReferenceDetector.cycles;
            }
        }
        private Dictionary<string, List<string>> bundleDependencies
        {
            get
            {
                return AssetBundleCycleReferenceDetector.bundleDependencies;
            }
        }


        private Vector2 scrollPos;
        private bool showDependencyGraph = false;
        private GUIStyle cycleStyle;
        private string msg = "";

        [MenuItem("Framework/Build/打开检测资源循环引用窗口")]
        public static void ShowWindow()
        {
            GetWindow<AssetBundleCycleReferenceDetectorWindow>("Bundle Cycle-Reference Detector");
        }

        void OnEnable()
        {
            cycleStyle = new GUIStyle(EditorStyles.label);
            cycleStyle.normal.textColor = Color.red;
            cycleStyle.fontStyle = FontStyle.Bold;
        }

        void OnGUI()
        {
            GUILayout.Label("AssetBundle Cycle-Reference Detector", EditorStyles.boldLabel);

            // Options
            EditorGUILayout.BeginVertical("box");
            showDependencyGraph = EditorGUILayout.Toggle("Show Dependency Graph", showDependencyGraph);
            EditorGUILayout.EndVertical();

            // Detection button
            if (GUILayout.Button("Detect Cycle-Bundle Cycles", GUILayout.Height(40)))
            {
                AssetBundleCycleReferenceDetector.DetectBundleCycles();
                if (cycles.Count == 0)
                {
                    msg = "No Cycle-bundle cycles found or detection not run";
                }
                else
                {
                    msg = $"Found {cycles.Count} Cycle-bundle cycle(s):";
                }
            }

            // Display results
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (showDependencyGraph && bundleDependencies.Count > 0)
            {
                GUILayout.Label("Dependency Graph:", EditorStyles.boldLabel);
                foreach (var bundle in bundleDependencies.Keys)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"{bundle} →");

                    if (bundleDependencies[bundle].Count > 0)
                    {
                        foreach (var dep in bundleDependencies[bundle])
                        {
                            EditorGUILayout.LabelField($"   - {dep}");
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("   (No dependencies)");
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            GUILayout.Label(msg);
            if (cycles.Count > 0)
            {
                foreach (var cycle in cycles)
                {
                    EditorGUILayout.BeginVertical("box");
                    for (int i = 0; i < cycle.Count; i++)
                    {
                        string arrow = i < cycle.Count - 1 ? "→" : "↺";
                        EditorGUILayout.LabelField($"{cycle[i]} {arrow}", cycleStyle);
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }


    }
}