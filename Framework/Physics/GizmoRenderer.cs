using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using bluebean.UGFramework.DataStruct;
using System.Collections.Generic;

namespace bluebean.UGFramework
{
    [ExecuteAlways] // 在编辑模式下也执行
    public class GizmoRenderer : MonoBehaviour
    {
#if UNITY_EDITOR
        public struct GizmoData
        {
            public Mesh mesh;
            public Matrix4x4 matrix;
            public Color color;
        }

        private static GizmoRenderer _instance;
        private CommandBuffer _commandBuffer;
        private Material _gizmoMaterial;
        private Camera _currentCamera;
        private static readonly List<GizmoData> _gizmosToDraw = new List<GizmoData>();

        private void Awake()
        {
            // 单例模式确保只有一个实例
            if (_instance != null && _instance != this)
            {
                DestroyImmediate(this);
                return;
            }

            _instance = this;
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            Cleanup();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void Initialize()
        {
            if (_commandBuffer == null)
            {
                _commandBuffer = new CommandBuffer { name = "GizmoRenderer" };
            }

            if (_gizmoMaterial == null)
            {
                CreateGizmoMaterial();
            }
        }

        private void Cleanup()
        {
            if (_commandBuffer != null)
            {
                _commandBuffer.Release();
                _commandBuffer = null;
            }

            if (_gizmoMaterial != null)
            {
                DestroyImmediate(_gizmoMaterial);
                _gizmoMaterial = null;
            }

            _gizmosToDraw.Clear();
        }

        private void CreateGizmoMaterial()
        {
            // 尝试加载自定义着色器
            Shader shader = Shader.Find("Hidden/GizmoDepthAware");

            // 如果找不到，回退到内置着色器
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            _gizmoMaterial = new Material(shader);
            _gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
            _gizmoMaterial.enableInstancing = true;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.type == EventType.Repaint)
            {
                RenderGizmos(sceneView.camera);
            }
        }

        private void RenderGizmos(Camera camera)
        {
            if (_gizmosToDraw.Count == 0) return;
            if (_gizmoMaterial == null) return;

            // 确保命令缓冲区准备就绪
            if (_commandBuffer == null)
            {
                _commandBuffer = new CommandBuffer { name = "GizmoRenderer" };
            }

            _commandBuffer.Clear();

            // 设置相机
            _currentCamera = camera;
            _commandBuffer.SetViewMatrix(camera.worldToCameraMatrix);
            _commandBuffer.SetProjectionMatrix(camera.projectionMatrix);

            // 按深度排序（从后往前）
            SortGizmosByDepth();

            // 添加绘制命令
            foreach (var gizmo in _gizmosToDraw)
            {
                ConfigureMaterialForGizmo(gizmo);
                _commandBuffer.DrawMesh(gizmo.mesh, gizmo.matrix, _gizmoMaterial);
            }

            // 执行命令缓冲区
            Graphics.ExecuteCommandBuffer(_commandBuffer);

            // 清除已绘制的Gizmo
            _gizmosToDraw.Clear();
        }

        private void SortGizmosByDepth()
        {
            if (_currentCamera == null) return;

            // 使用相机视图空间Z值排序（从远到近）
            _gizmosToDraw.Sort((a, b) =>
            {
                Vector3 posA = a.matrix.MultiplyPoint3x4(Vector3.zero);
                Vector3 posB = b.matrix.MultiplyPoint3x4(Vector3.zero);

                float depthA = _currentCamera.worldToCameraMatrix.MultiplyPoint3x4(posA).z;
                float depthB = _currentCamera.worldToCameraMatrix.MultiplyPoint3x4(posB).z;

                return depthB.CompareTo(depthA); // 降序排序（远到近）
            });
        }

        private void ConfigureMaterialForGizmo(GizmoData gizmo)
        {
            if (_gizmoMaterial == null) return;

            // 设置颜色
            _gizmoMaterial.color = gizmo.color;

            // 配置深度设置
            if (gizmo.color.a < 0.99f)
            {
                // 透明物体
                _gizmoMaterial.SetInt("_ZWrite", 0);
                _gizmoMaterial.SetInt("_ZTest", (int)CompareFunction.LessEqual);
            }
            else
            {
                // 不透明物体
                _gizmoMaterial.SetInt("_ZWrite", 1);
                _gizmoMaterial.SetInt("_ZTest", (int)CompareFunction.Less);
            }
        }

        // 公共API：添加要绘制的Gizmo
        public static void DrawGizmo(Mesh mesh, Matrix4x4 matrix, Color color)
        {
            if (!Application.isEditor) return;
            if (mesh == null) return;

            EnsureInstanceExists();

            _gizmosToDraw.Add(new GizmoData
            {
                mesh = mesh,
                matrix = matrix,
                color = color
            });
        }

        public static void DrawMesh(Mesh mesh, Color color)
        {
            if (!Application.isEditor) return;
            // 添加到绘制队列
            DrawGizmo(mesh, Matrix4x4.identity, color);
        }

        // 公共API：绘制四边形Gizmo
        public static void DrawQuad(Aabb aabb, int axis, float value, Color color)
        {
            if (!Application.isEditor) return;

            // 计算四边形顶点
            Vector3 min = aabb.min;
            Vector3 max = aabb.max;
            min[axis] = value;
            max[axis] = value;

            Vector3 size = aabb.size;
            Vector3 dir = Vector3.zero;
            switch (axis)
            {
                case 0: dir = new Vector3(0, 0, size.z); break; // X轴
                case 1: dir = new Vector3(0, 0, size.z); break; // Y轴
                case 2: dir = new Vector3(0, size.y, 0); break; // Z轴
            }

            Vector3 p1 = min;
            Vector3 p2 = min + dir;
            Vector3 p3 = max;
            Vector3 p4 = max - dir;

            // 创建双面四边形
            Mesh quad = CreateDoubleSidedQuad(p1, p2, p3, p4);

            // 添加到绘制队列
            DrawGizmo(quad, Matrix4x4.identity, color);
        }

        private static Mesh CreateDoubleSidedQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3[] vertices = { p1, p2, p3, p4 };

            // 双面三角形索引
            int[] indices = {
            // 正面
            0, 1, 3,
            1, 2, 3,
            
            // 反面
            0, 3, 1,
            1, 3, 2
        };

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void EnsureInstanceExists()
        {
            if (_instance != null) return;

            // 查找现有实例
            _instance = FindObjectOfType<GizmoRenderer>();

            // 创建新实例
            if (_instance == null)
            {
                var go = new GameObject("GizmoRenderer", typeof(GizmoRenderer));
                go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                _instance = go.GetComponent<GizmoRenderer>();
            }
        }
#endif
    }
}