using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace bluebean.Framework.CurveEditor
{
    public enum CurveInterpolationType
    {
        Linear,
        Bezier,
        CatmullRom
    }

    public class CurveEditorWindow : EditorWindow
    {
        private CurveData curveData;
        private CurveInterpolationType interpolationType = CurveInterpolationType.Bezier;
        private int hoveredPoint = -1;
        private int hoveredSegment = -1;
        private const float pointPickSize = 0.15f;
        private const float segmentPickDistance = 0.1f;
        private bool showTangents = false;
        private GameObject particlePrefab = null;
        private float discretizeSpacing = 1.0f;
        private List<GameObject> placedParticles = new List<GameObject>();
        private GameObject particlesRoot = null;
        private const string particlesRootName = "CurveParticlesRoot";
        private int currentCurveIndex = 0;
        private const float curveLineWidth = 4f;
        private HashSet<int> selectedPoints = new HashSet<int>();
        private double lastClickTime = 0;
        private int lastClickedPoint = -1;
        private const float doubleClickInterval = 0.3f;
        private double lastSegmentClickTime = 0;
        private int lastClickedSegment = -1;
        // 移除 isBoxSelecting, boxStart, boxEnd, boxSelectThreshold 相关字段和所有框选相关逻辑
        private bool mouseDownOnEmpty = false;
        private bool isEnable = true;

        // EditorPrefs缓存Key
        private const string PrefKey_CurveData = "CurveEditorWindow_CurveData";
        private const string PrefKey_ParticlePrefab = "CurveEditorWindow_ParticlePrefab";
        private const string PrefKey_InterpolationType = "CurveEditorWindow_InterpolationType";
        private const string PrefKey_DiscretizeSpacing = "CurveEditorWindow_DiscretizeSpacing";

        [MenuItem("Framework/Tools/Curve Editor")]
        public static void ShowWindow()
        {
            GetWindow<CurveEditorWindow>("Curve Editor");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            // 恢复缓存
            string curveDataPath = EditorPrefs.GetString(PrefKey_CurveData, "");
            if (!string.IsNullOrEmpty(curveDataPath))
                curveData = AssetDatabase.LoadAssetAtPath<CurveData>(curveDataPath);
            string prefabPath = EditorPrefs.GetString(PrefKey_ParticlePrefab, "");
            if (!string.IsNullOrEmpty(prefabPath))
                particlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            interpolationType = (CurveInterpolationType)EditorPrefs.GetInt(PrefKey_InterpolationType, (int)CurveInterpolationType.Bezier);
            discretizeSpacing = EditorPrefs.GetFloat(PrefKey_DiscretizeSpacing, 1.0f);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            // 保存缓存
            if (curveData != null)
                EditorPrefs.SetString(PrefKey_CurveData, AssetDatabase.GetAssetPath(curveData));
            if (particlePrefab != null)
                EditorPrefs.SetString(PrefKey_ParticlePrefab, AssetDatabase.GetAssetPath(particlePrefab));
            EditorPrefs.SetInt(PrefKey_InterpolationType, (int)interpolationType);
            EditorPrefs.SetFloat(PrefKey_DiscretizeSpacing, discretizeSpacing);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            isEnable = EditorGUILayout.ToggleLeft("启用曲线编辑", isEnable, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            if (!isEnable)
            {
                EditorGUILayout.HelpBox("CurveEditorWindow已禁用。", MessageType.Info);
                return;
            }
            curveData = (CurveData)EditorGUILayout.ObjectField("Curve Data", curveData, typeof(CurveData), false);
            if (curveData != null)
            {
                // 曲线选择与添加
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"当前曲线: {currentCurveIndex + 1}/{curveData.curves.Count}", GUILayout.Width(120));
                if (GUILayout.Button("上一条", GUILayout.Width(60)))
                {
                    if (curveData.curves.Count > 0)
                    {
                        currentCurveIndex = (currentCurveIndex - 1 + curveData.curves.Count) % curveData.curves.Count;
                        GUI.FocusControl(null);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                if (GUILayout.Button("下一条", GUILayout.Width(60)))
                {
                    if (curveData.curves.Count > 0)
                    {
                        currentCurveIndex = (currentCurveIndex + 1) % curveData.curves.Count;
                        GUI.FocusControl(null);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                if (GUILayout.Button("添加曲线", GUILayout.Width(80)))
                {
                    Undo.RecordObject(curveData, "Add Curve");
                    var newCurve = new CurveSegment();
                    newCurve.points.Add(Vector3.zero);
                    newCurve.points.Add(Vector3.right);
                    curveData.curves.Add(newCurve);
                    currentCurveIndex = curveData.curves.Count - 1;
                    EditorUtility.SetDirty(curveData);
                    GUI.FocusControl(null);
                    SceneView.RepaintAll();
                    Repaint();
                }
                if (GUILayout.Button("复制当前曲线", GUILayout.Width(100)))
                {
                    if (curveData.curves.Count > 0)
                    {
                        Undo.RecordObject(curveData, "Copy Curve");
                        var src = curveData.curves[currentCurveIndex].points;
                        var newCurve = new CurveSegment();
                        newCurve.points.AddRange(src);
                        curveData.curves.Add(newCurve);
                        currentCurveIndex = curveData.curves.Count - 1;
                        selectedPoints.Clear();
                        for (int i = 0; i < newCurve.points.Count; i++)
                            selectedPoints.Add(i);
                        EditorUtility.SetDirty(curveData);
                        GUI.FocusControl(null);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                if (GUILayout.Button("删除当前曲线", GUILayout.Width(100)))
                {
                    if (curveData.curves.Count > 0)
                    {
                        Undo.RecordObject(curveData, "Delete Curve");
                        curveData.curves.RemoveAt(currentCurveIndex);
                        if (curveData.curves.Count == 0)
                        {
                            currentCurveIndex = 0;
                        }
                        else if (currentCurveIndex >= curveData.curves.Count)
                        {
                            currentCurveIndex = curveData.curves.Count - 1;
                        }
                        selectedPoints.Clear();
                        EditorUtility.SetDirty(curveData);
                        GUI.FocusControl(null);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                // 当前曲线隐藏toggle
                if (curveData.curves.Count > 0)
                {
                    bool isHidden = curveData.curves[currentCurveIndex].isHidden;
                    bool newHidden = EditorGUILayout.ToggleLeft("隐藏当前曲线", isHidden, GUILayout.Width(100));
                    if (newHidden != isHidden)
                    {
                        Undo.RecordObject(curveData, "Toggle Curve Hidden");
                        curveData.curves[currentCurveIndex].isHidden = newHidden;
                        EditorUtility.SetDirty(curveData);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (curveData.curves.Count == 0)
                {
                    EditorGUILayout.HelpBox("请先添加曲线", MessageType.Info);
                    return;
                }
            }
            interpolationType = (CurveInterpolationType)EditorGUILayout.EnumPopup("插值类型", interpolationType);
            showTangents = EditorGUILayout.Toggle("可视化切线", showTangents);
            particlePrefab = (GameObject)EditorGUILayout.ObjectField("离散化粒子Prefab", particlePrefab, typeof(GameObject), false);
            discretizeSpacing = EditorGUILayout.FloatField("离散化间距", discretizeSpacing);
            if (GUILayout.Button("离散化曲线并放置粒子") && particlePrefab != null && curveData != null && curveData.curves.Count > 0 && discretizeSpacing > 0)
            {
                DeleteAllPlacedParticles();
                DiscretizeAllCurvesBySpacingAndPlaceParticles();
            }
            if (GUILayout.Button("删除所有离散化粒子"))
            {
                DeleteAllPlacedParticles();
            }

            if (curveData != null)
            {
                if (GUILayout.Button("Add Point"))
                {
                    Undo.RecordObject(curveData, "Add Point");
                    Vector3 newPoint = Vector3.zero;
                    if (curveData.curves[currentCurveIndex].points.Count > 0)
                    {
                        Vector3 last = curveData.curves[currentCurveIndex].points[curveData.curves[currentCurveIndex].points.Count - 1];
                        Vector3 tangent = Vector3.forward;
                        if (curveData.curves[currentCurveIndex].points.Count > 1)
                        {
                            tangent = (last - curveData.curves[currentCurveIndex].points[curveData.curves[currentCurveIndex].points.Count - 2]).normalized;
                            if (tangent == Vector3.zero) tangent = Vector3.forward;
                        }
                        newPoint = last + tangent * 0.1f;
                    }
                    curveData.curves[currentCurveIndex].points.Add(newPoint);
                    selectedPoints.Clear();
                    selectedPoints.Add(curveData.curves[currentCurveIndex].points.Count - 1);
                    EditorUtility.SetDirty(curveData);
                }
                if (GUILayout.Button("Clear Points"))
                {
                    Undo.RecordObject(curveData, "Clear Points");
                    curveData.curves[currentCurveIndex].points.Clear();
                    EditorUtility.SetDirty(curveData);
                }
                if (GUILayout.Button("删除选中控制点", GUILayout.Width(120)))
                {
                    var points = curveData.curves[currentCurveIndex].points;
                    if (selectedPoints.Count > 0 && points.Count > 0)
                    {
                        Undo.RecordObject(curveData, "Delete Selected Points");
                        // 倒序删除，避免索引错乱
                        var toDelete = new List<int>(selectedPoints);
                        toDelete.Sort((a, b) => b.CompareTo(a));
                        foreach (var idx in toDelete)
                        {
                            if (idx >= 0 && idx < points.Count)
                                points.RemoveAt(idx);
                        }
                        selectedPoints.Clear();
                        EditorUtility.SetDirty(curveData);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                if (GUILayout.Button("全选控制点", GUILayout.Width(100)))
                {
                    selectedPoints.Clear();
                    var points = curveData.curves[currentCurveIndex].points;
                    for (int i = 0; i < points.Count; i++)
                        selectedPoints.Add(i);
                    SceneView.RepaintAll();
                    Repaint();
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!isEnable) return;
            if (curveData == null || curveData.curves.Count == 0) return;
            Event e = Event.current;
            // 保证Unity原生Alt视图操作不被拦截
            //if ((e.modifiers & EventModifiers.Alt) != 0)
            //    return;
            hoveredPoint = -1;
            hoveredSegment = -1;
            var points = curveData.curves[currentCurveIndex].points;

            // 1. 先画所有非当前曲线（灰色，不可交互）
            Handles.color = Color.blue;
            for (int c = 0; c < curveData.curves.Count; c++)
            {
                if (c == currentCurveIndex) continue;
                if (curveData.curves[c].isHidden) continue;
                var curve = curveData.curves[c].points;
                if (curve.Count < 2) continue;
                Handles.color = Color.blue;
                for (int i = 0; i < curve.Count; i++)
                {
                    Vector3 pos = curve[i];
                    float size = HandleUtility.GetHandleSize(pos) * pointPickSize * 1.2f;
                    Handles.SphereHandleCap(0, pos, Quaternion.identity, size, EventType.Repaint);
                }
                Handles.color = Color.gray;
                if (interpolationType == CurveInterpolationType.Linear)
                {
                    for (int i = 0; i < curve.Count - 1; i++)
                        Handles.DrawAAPolyLine(curveLineWidth, curve[i], curve[i + 1]);
                }
                else if (interpolationType == CurveInterpolationType.Bezier)
                {
                    DrawBezierCurve(curve, false);
                }
                else if (interpolationType == CurveInterpolationType.CatmullRom)
                {
                    DrawCatmullRomCurve(curve, false);
                }
            }

            // 2. 画当前曲线点和拖拽
            if (!curveData.curves[currentCurveIndex].isHidden)
            {
                Handles.color = Color.green;
                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 pos = points[i];
                    float size = HandleUtility.GetHandleSize(pos) * pointPickSize;
                    bool isSelected = selectedPoints.Contains(i);
                    Handles.color = isSelected ? Color.red : Color.green;
                    if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap))
                    {
                        // 双击检测
                        if (lastClickedPoint == i && EditorApplication.timeSinceStartup - lastClickTime < doubleClickInterval)
                        {
                            SceneView.lastActiveSceneView.LookAt(pos);
                        }
                        lastClickTime = EditorApplication.timeSinceStartup;
                        lastClickedPoint = i;
                        if (e.control || e.shift)
                        {
                            if (isSelected) selectedPoints.Remove(i);
                            else selectedPoints.Add(i);
                        }
                        else
                        {
                            selectedPoints.Clear();
                            selectedPoints.Add(i);
                        }
                        GUI.FocusControl(null);
                        SceneView.RepaintAll();
                    }
                    // 单点拖拽
                    if (isSelected && selectedPoints.Count == 1)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(curveData, "Move Point");
                            points[i] = newPos;
                            EditorUtility.SetDirty(curveData);
                        }
                    }
                    if (HandleUtility.DistanceToCircle(pos, size) < 10f)
                    {
                        hoveredPoint = i;
                    }

                }
            }
            Handles.color = Color.yellow;
            // 整体拖动Handle
            if (selectedPoints.Count > 1)
            {
                Vector3 center = Vector3.zero;
                foreach (var idx in selectedPoints) center += points[idx];
                center /= selectedPoints.Count;
                EditorGUI.BeginChangeCheck();
                Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 delta = newCenter - center;
                    Undo.RecordObject(curveData, "Move Multiple Points");
                    foreach (var idx in selectedPoints)
                    {
                        points[idx] += delta;
                    }
                    EditorUtility.SetDirty(curveData);
                }
            }
            Handles.color = Color.yellow;
            if (points.Count >= 2)
            {
                if (interpolationType == CurveInterpolationType.Linear)
                {
                    for (int i = 0; i < points.Count - 1; i++)
                        Handles.DrawAAPolyLine(curveLineWidth, points[i], points[i + 1]);
                }
                else if (interpolationType == CurveInterpolationType.Bezier)
                {
                    DrawBezierCurve(points, true);
                }
                else if (interpolationType == CurveInterpolationType.CatmullRom)
                {
                    DrawCatmullRomCurve(points, true);
                }
            }
            // 检测段悬停
            if (points.Count >= 2)
            {
                int bestSeg = -1;
                float bestDist = float.MaxValue;
                Vector3 bestCurvePos = Vector3.zero;
                int bestInsertIdx = -1;
                Vector2 mousePos = Event.current.mousePosition;
                // 对每个段采样，找最近点
                for (int i = 0; i < points.Count - 1; i++)
                {
                    int sampleCount = 30;
                    float tStart = i / (float)(points.Count - 1);
                    float tEnd = (i + 1) / (float)(points.Count - 1);
                    for (int s = 0; s <= sampleCount; s++)
                    {
                        float t = Mathf.Lerp(tStart, tEnd, s / (float)sampleCount);
                        Vector3 pt = SampleCurveAt(t, points, interpolationType);
                        float dist = (HandleUtility.WorldToGUIPoint(pt) - mousePos).magnitude;
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestSeg = i;
                            bestCurvePos = pt;
                            bestInsertIdx = i + 1;
                        }
                    }
                }
                if (bestDist < 15f) // 鼠标足够靠近曲线
                {
                    hoveredSegment = bestSeg;
                    // 双击曲线插入点
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (lastClickedSegment == bestSeg && EditorApplication.timeSinceStartup - lastSegmentClickTime < doubleClickInterval)
                        {
                            Undo.RecordObject(curveData, "Insert Point By DoubleClick");
                            points.Insert(bestInsertIdx, bestCurvePos);
                            selectedPoints.Clear();
                            selectedPoints.Add(bestInsertIdx);
                            EditorUtility.SetDirty(curveData);
                            SceneView.RepaintAll();
                            Event.current.Use();
                        }
                        lastSegmentClickTime = EditorApplication.timeSinceStartup;
                        lastClickedSegment = bestSeg;
                    }
                }
            }

            // 4. 右键菜单
            if (e.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();
                if (hoveredPoint != -1)
                {
                    menu.AddItem(new GUIContent("删除点"), false, () =>
                    {
                        Undo.RecordObject(curveData, "Delete Point");
                        points.RemoveAt(hoveredPoint);
                        EditorUtility.SetDirty(curveData);
                    });
                }
                if (hoveredSegment != -1)
                {
                    menu.AddItem(new GUIContent("插入点"), false, () =>
                    {
                        Undo.RecordObject(curveData, "Insert Point");
                        Vector3 p0 = points[hoveredSegment];
                        Vector3 p1 = points[hoveredSegment + 1];
                        Vector3 insertPos = (p0 + p1) * 0.5f;
                        points.Insert(hoveredSegment + 1, insertPos);
                        EditorUtility.SetDirty(curveData);
                    });
                }
                menu.ShowAsContext();
                e.Use();
            }
            // 处理点击空白处取消选择（仅单击且无拖动/框选/点/段命中时）
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (hoveredPoint == -1 && hoveredSegment == -1 && !mouseDownOnEmpty)
                    mouseDownOnEmpty = true;
                else
                    mouseDownOnEmpty = false;
            }
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                mouseDownOnEmpty = false;
            }
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                if (mouseDownOnEmpty && hoveredPoint == -1 && hoveredSegment == -1)
                {
                    if (selectedPoints.Count > 0)
                    {
                        selectedPoints.Clear();
                        GUI.FocusControl(null);
                        SceneView.RepaintAll();
                        Repaint();
                    }
                }
                mouseDownOnEmpty = false;
            }
            // 处理Delete/Backspace快捷键删除选中控制点
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace))
            {
                var delPoints = curveData.curves[currentCurveIndex].points;
                if (selectedPoints.Count > 0 && delPoints.Count > 0)
                {
                    Undo.RecordObject(curveData, "Delete Selected Points");
                    var toDelete = new List<int>(selectedPoints);
                    toDelete.Sort((a, b) => b.CompareTo(a));
                    foreach (var idx in toDelete)
                    {
                        if (idx >= 0 && idx < delPoints.Count)
                            delPoints.RemoveAt(idx);
                    }
                    selectedPoints.Clear();
                    EditorUtility.SetDirty(curveData);
                    SceneView.RepaintAll();
                    Repaint();
                    Event.current.Use();
                }
            }
        }

        // 修改DrawBezierCurve和DrawCatmullRomCurve，增加isCurrent参数，当前曲线可显示切线和交互，非当前曲线只画曲线
        private void DrawBezierCurve(List<Vector3> points, bool isCurrent)
        {
            int count = points.Count;
            if (count < 2) return;
            for (int i = 0; i < count - 1; i++)
            {
                Vector3 p0 = points[i];
                Vector3 p1 = points[i + 1];
                Vector3 c0 = p0 + (i > 0 ? (points[i + 1] - points[i - 1]) : (points[i + 1] - points[i])) * 0.25f;
                Vector3 c1 = p1 - (i < count - 2 ? (points[i + 2] - points[i]) : (points[i + 1] - points[i])) * 0.25f;
                if (isCurrent && showTangents)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawAAPolyLine(curveLineWidth, p0, c0);
                    Handles.DrawAAPolyLine(curveLineWidth, p1, c1);
                    Handles.SphereHandleCap(0, c0, Quaternion.identity, HandleUtility.GetHandleSize(c0) * 0.07f, EventType.Repaint);
                    Handles.SphereHandleCap(0, c1, Quaternion.identity, HandleUtility.GetHandleSize(c1) * 0.07f, EventType.Repaint);
                    Handles.color = Color.yellow;
                }
                Vector3 prev = p0;
                for (int j = 1; j <= 20; j++)
                {
                    float t = j / 20f;
                    Vector3 pt = Mathf.Pow(1 - t, 3) * p0 +
                                 3 * Mathf.Pow(1 - t, 2) * t * c0 +
                                 3 * (1 - t) * t * t * c1 +
                                 Mathf.Pow(t, 3) * p1;
                    Handles.DrawAAPolyLine(curveLineWidth, prev, pt);
                    prev = pt;
                }
            }
        }
        private void DrawCatmullRomCurve(List<Vector3> points, bool isCurrent)
        {
            int count = points.Count;
            if (count < 2) return;
            Vector3 prev = points[0];
            for (int i = 0; i < count - 1; i++)
            {
                Vector3 p0 = i == 0 ? points[i] : points[i - 1];
                Vector3 p1 = points[i];
                Vector3 p2 = points[i + 1];
                Vector3 p3 = (i + 2 < count) ? points[i + 2] : points[i + 1];
                for (int j = 1; j <= 20; j++)
                {
                    float t = j / 20f;
                    Vector3 pt = GetCatmullRomPosition(t, p0, p1, p2, p3);
                    Handles.DrawAAPolyLine(curveLineWidth, prev, pt);
                    if (isCurrent && showTangents)
                    {
                        Vector3 tangent = GetCatmullRomTangent(t, p0, p1, p2, p3).normalized * HandleUtility.GetHandleSize(pt) * 0.5f;
                        Handles.color = Color.cyan;
                        Handles.DrawAAPolyLine(curveLineWidth, pt, pt + tangent);
                        Handles.color = Color.yellow;
                    }
                    prev = pt;
                }
            }
        }

        // Catmull-Rom插值公式
        private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        // Catmull-Rom切线公式
        private Vector3 GetCatmullRomTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            return 0.5f * ((-p0 + p2) + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2);
        }

        // 按距离离散化所有曲线
        private void DiscretizeAllCurvesBySpacingAndPlaceParticles()
        {
            particlesRoot = new GameObject(particlesRootName);
            Undo.RegisterCreatedObjectUndo(particlesRoot, "Create Particles Root");
            placedParticles.Clear();
            for (int c = 0; c < curveData.curves.Count; c++)
            {
                var curvePoints = curveData.curves[c].points;
                if (curvePoints.Count < 2) continue;
                // 1. 先高密度采样曲线
                int highSampleCount = 5000;
                List<Vector3> denseSamples = new List<Vector3>();
                List<float> cumulativeLengths = new List<float>();
                float totalLen = 0f;
                Vector3 prev = SampleCurveAt(0, curvePoints, interpolationType);
                denseSamples.Add(prev);
                cumulativeLengths.Add(0f);
                for (int i = 1; i <= highSampleCount; i++)
                {
                    float t = i / (float)highSampleCount;
                    Vector3 pt = SampleCurveAt(t, curvePoints, interpolationType);
                    totalLen += Vector3.Distance(prev, pt);
                    denseSamples.Add(pt);
                    cumulativeLengths.Add(totalLen);
                    prev = pt;
                }
                // 2. 计算粒子数量和实际间距，保证首尾对齐
                int N = Mathf.Max(2, Mathf.RoundToInt(totalLen / discretizeSpacing) + 1);
                float actualSpacing = totalLen / (N - 1);
                // 3. 依次采样
                List<Vector3> samples = new List<Vector3>();
                for (int i = 0; i < N; i++)
                {
                    float targetLen = i * actualSpacing;
                    if (targetLen > totalLen) targetLen = totalLen;
                    int idx = cumulativeLengths.BinarySearch(targetLen);
                    if (idx < 0) idx = ~idx;
                    if (idx == 0)
                    {
                        samples.Add(denseSamples[0]);
                    }
                    else if (idx >= denseSamples.Count)
                    {
                        samples.Add(denseSamples[denseSamples.Count - 1]);
                    }
                    else
                    {
                        float lenA = cumulativeLengths[idx - 1];
                        float lenB = cumulativeLengths[idx];
                        float t = (targetLen - lenA) / (lenB - lenA + 1e-6f);
                        Vector3 pA = denseSamples[idx - 1];
                        Vector3 pB = denseSamples[idx];
                        samples.Add(Vector3.Lerp(pA, pB, t));
                    }
                }
                // 实例化粒子
                float offset = 0.01f;
                for (int i = 0; i < samples.Count; i++)
                {
                    Vector3 pos = samples[i];
                    if (i > 0)
                    {
                        Vector3 tangent = (samples[i] - samples[i - 1]).normalized;
                        pos += tangent * offset;
                    }
                    GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(particlePrefab);
                    go.transform.position = pos;
                    go.transform.SetParent(particlesRoot.transform);
                    Undo.RegisterCreatedObjectUndo(go, "Place Particle");
                    placedParticles.Add(go);
                }
            }
        }

        private void DeleteAllPlacedParticles()
        {
            foreach (var go in placedParticles)
            {
                if (go != null)
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }
            placedParticles.Clear();
            if (particlesRoot != null)
            {
                Undo.DestroyObjectImmediate(particlesRoot);
                particlesRoot = null;
            }
        }

        // 估算贝塞尔曲线长度
        private float EstimateBezierLength(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, int steps = 10)
        {
            float len = 0f;
            Vector3 prev = p0;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 pt = Mathf.Pow(1 - t, 3) * p0 +
                             3 * Mathf.Pow(1 - t, 2) * t * c0 +
                             3 * (1 - t) * t * t * c1 +
                             Mathf.Pow(t, 3) * p1;
                len += Vector3.Distance(prev, pt);
                prev = pt;
            }
            return len;
        }

        // 估算Catmull-Rom曲线长度
        private float EstimateCatmullRomLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps = 10)
        {
            float len = 0f;
            Vector3 prev = p1;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 pt = GetCatmullRomPosition(t, p0, p1, p2, p3);
                len += Vector3.Distance(prev, pt);
                prev = pt;
            }
            return len;
        }

        // 曲线采样函数
        private Vector3 SampleCurveAt(float t, List<Vector3> points, CurveInterpolationType type)
        {
            int count = points.Count;
            if (type == CurveInterpolationType.Linear)
            {
                float totalLen = 0f;
                List<float> segLens = new List<float>();
                for (int i = 0; i < count - 1; i++)
                {
                    float len = Vector3.Distance(points[i], points[i + 1]);
                    segLens.Add(len);
                    totalLen += len;
                }
                float targetLen = t * totalLen;
                float acc = 0f;
                for (int i = 0; i < segLens.Count; i++)
                {
                    if (acc + segLens[i] >= targetLen)
                    {
                        float segT = (targetLen - acc) / segLens[i];
                        return Vector3.Lerp(points[i], points[i + 1], segT);
                    }
                    acc += segLens[i];
                }
                return points[count - 1];
            }
            else if (type == CurveInterpolationType.Bezier)
            {
                // 采样到总段数
                float totalLen = 0f;
                List<float> segLens = new List<float>();
                for (int i = 0; i < count - 1; i++)
                {
                    Vector3 p0 = points[i];
                    Vector3 p1 = points[i + 1];
                    Vector3 c0 = p0 + (i > 0 ? (points[i + 1] - points[i - 1]) : (points[i + 1] - points[i])) * 0.25f;
                    Vector3 c1 = p1 - (i < count - 2 ? (points[i + 2] - points[i]) : (points[i + 1] - points[i])) * 0.25f;
                    float len = EstimateBezierLength(p0, c0, c1, p1);
                    segLens.Add(len);
                    totalLen += len;
                }
                float targetLen = t * totalLen;
                float acc = 0f;
                for (int i = 0; i < segLens.Count; i++)
                {
                    if (acc + segLens[i] >= targetLen)
                    {
                        float segT = (targetLen - acc) / segLens[i];
                        Vector3 p0 = points[i];
                        Vector3 p1 = points[i + 1];
                        Vector3 c0 = p0 + (i > 0 ? (points[i + 1] - points[i - 1]) : (points[i + 1] - points[i])) * 0.25f;
                        Vector3 c1 = p1 - (i < count - 2 ? (points[i + 2] - points[i]) : (points[i + 1] - points[i])) * 0.25f;
                        return Mathf.Pow(1 - segT, 3) * p0 +
                               3 * Mathf.Pow(1 - segT, 2) * segT * c0 +
                               3 * (1 - segT) * segT * segT * c1 +
                               Mathf.Pow(segT, 3) * p1;
                    }
                    acc += segLens[i];
                }
                return points[count - 1];
            }
            else if (type == CurveInterpolationType.CatmullRom)
            {
                float totalLen = 0f;
                List<float> segLens = new List<float>();
                for (int i = 0; i < count - 1; i++)
                {
                    Vector3 p0 = i == 0 ? points[i] : points[i - 1];
                    Vector3 p1 = points[i];
                    Vector3 p2 = points[i + 1];
                    Vector3 p3 = (i + 2 < count) ? points[i + 2] : points[i + 1];
                    float len = EstimateCatmullRomLength(p0, p1, p2, p3);
                    segLens.Add(len);
                    totalLen += len;
                }
                float targetLen = t * totalLen;
                float acc = 0f;
                for (int i = 0; i < segLens.Count; i++)
                {
                    if (acc + segLens[i] >= targetLen)
                    {
                        float segT = (targetLen - acc) / segLens[i];
                        Vector3 p0 = i == 0 ? points[i] : points[i - 1];
                        Vector3 p1 = points[i];
                        Vector3 p2 = points[i + 1];
                        Vector3 p3 = (i + 2 < count) ? points[i + 2] : points[i + 1];
                        return GetCatmullRomPosition(segT, p0, p1, p2, p3);
                    }
                    acc += segLens[i];
                }
                return points[count - 1];
            }
            return points[0];
        }

        // 工具函数：生成矩形
        private static Rect MakeRect(Vector2 a, Vector2 b)
        {
            return new Rect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
    }

    [CustomEditor(typeof(CurveData))]
    public class CurveDataEditor : Editor
    {
        private const float curveLineWidth = 4f;
        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            CurveData curve = (CurveData)target;
            if (curve.curves == null || curve.curves.Count == 0 || curve.curves[0].points.Count < 2)
            {
                EditorGUI.LabelField(r, "无曲线数据");
                return;
            }

            // 画白底
            EditorGUI.DrawRect(r, Color.white);

            // 归一化到窗口
            Vector3 min = curve.curves[0].points[0], max = curve.curves[0].points[0];
            foreach (var pt in curve.curves[0].points)
            {
                min = Vector3.Min(min, pt);
                max = Vector3.Max(max, pt);
            }
            Vector3 size = max - min;
            float margin = 10f;
            Vector2 scale = new Vector2(
                (r.width - 2 * margin) / (size.x == 0 ? 1 : size.x),
                (r.height - 2 * margin) / (size.y == 0 ? 1 : size.y)
            );

            // 画线
            Handles.BeginGUI();
            Handles.color = Color.blue;
            for (int i = 0; i < curve.curves[0].points.Count - 1; i++)
            {
                Vector2 p0 = new Vector2(
                    (curve.curves[0].points[i].x - min.x) * scale.x + r.x + margin,
                    r.yMax - ((curve.curves[0].points[i].y - min.y) * scale.y + margin)
                );
                Vector2 p1 = new Vector2(
                    (curve.curves[0].points[i + 1].x - min.x) * scale.x + r.x + margin,
                    r.yMax - ((curve.curves[0].points[i + 1].y - min.y) * scale.y + margin)
                );
                Handles.DrawAAPolyLine(curveLineWidth, p0, p1);
            }
            Handles.EndGUI();
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Curve Preview");
        }
    }
}