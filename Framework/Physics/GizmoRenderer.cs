using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using bluebean.UGFramework.DataStruct;
using System.Collections.Generic;

namespace bluebean.UGFramework
{

    [ExecuteAlways]
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
        private MaterialPropertyBlock _propertyBlock;
        private List<MaterialPropertyBlock> _propertyBlocks = new List<MaterialPropertyBlock>();
        private Queue<MaterialPropertyBlock> _propertyBlockPool = new Queue<MaterialPropertyBlock>();


        MaterialPropertyBlock GetPropertyBlock()
        {
            if (_propertyBlockPool.Count > 0)
                return _propertyBlockPool.Dequeue();

            return new MaterialPropertyBlock();
        }

        void ReleasePropertyBlock(MaterialPropertyBlock block)
        {
            block.Clear();
            _propertyBlockPool.Enqueue(block);
        }

        private void Awake()
        {
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

        private void OnDestroy()
        {
            Cleanup();
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

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
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
            _propertyBlocks.Clear();
            _propertyBlockPool.Clear();
        }

        private void CreateGizmoMaterial()
        {
            Shader shader = Shader.Find("Hidden/GizmoDepthAware");

            if (shader == null)
            {
                Debug.LogWarning("GizmoDepthAware shader not found. Using fallback shader.");
                shader = Shader.Find("Unlit/Color");
            }

            if (shader == null)
            {
                Debug.LogError("Failed to find any suitable shader for Gizmo rendering");
                return;
            }

            _gizmoMaterial = new Material(shader);
            _gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
            _gizmoMaterial.enableInstancing = true; // 启用实例化支持
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
            if (camera == null) return;

            if (_commandBuffer == null)
            {
                _commandBuffer = new CommandBuffer { name = "GizmoRenderer" };
            }

            _commandBuffer.Clear();
            _currentCamera = camera;

            // 设置视图和投影矩阵
            _commandBuffer.SetViewProjectionMatrices(
                camera.worldToCameraMatrix,
                camera.projectionMatrix
            );

            // 为每个网格创建独立的属性块
            CreatePropertyBlocks();

            // 按深度排序
            SortGizmosByDepth();

            // 添加绘制命令
            for (int i = 0; i < _gizmosToDraw.Count; i++)
            {
                _commandBuffer.DrawMesh(
                    _gizmosToDraw[i].mesh,
                    _gizmosToDraw[i].matrix,
                    _gizmoMaterial,
                    0, // submesh index
                    0, // shader pass
                    _propertyBlocks[i] // 使用独立的属性块
                );
            }

            // 执行命令缓冲区
            Graphics.ExecuteCommandBuffer(_commandBuffer);

            foreach (var item in _propertyBlocks)
            {
                ReleasePropertyBlock(item);
            }

            // 清除已绘制的Gizmo
            _gizmosToDraw.Clear();
            _propertyBlocks.Clear();
            
        }

        private void CreatePropertyBlocks()
        {
            
            _propertyBlocks.Clear();

            foreach (var gizmo in _gizmosToDraw)
            {
                var block = GetPropertyBlock();

                // 设置颜色
                block.SetColor("_Color", gizmo.color);

                // 配置深度设置
                if (gizmo.color.a < 0.99f)
                {
                    // 透明物体
                    block.SetInt("_ZWrite", 0);
                    block.SetInt("_ZTest", (int)CompareFunction.LessEqual);
                }
                else
                {
                    // 不透明物体
                    block.SetInt("_ZWrite", 1);
                    block.SetInt("_ZTest", (int)CompareFunction.Less);
                }

                _propertyBlocks.Add(block);
            }
        }

        private void SortGizmosByDepth()
        {
            if (_currentCamera == null) return;

            // 使用临时列表进行排序，保持属性块和Gizmo数据对应
            var sortedList = new List<GizmoData>(_gizmosToDraw);
            var sortedBlocks = new List<MaterialPropertyBlock>(_propertyBlocks);

            sortedList.Sort((a, b) =>
            {
                Vector3 posA = a.matrix.MultiplyPoint3x4(Vector3.zero);
                Vector3 posB = b.matrix.MultiplyPoint3x4(Vector3.zero);

                float depthA = _currentCamera.WorldToViewportPoint(posA).z;
                float depthB = _currentCamera.WorldToViewportPoint(posB).z;

                return depthB.CompareTo(depthA);
            });

            // 重新排序属性块以匹配排序后的Gizmo
            for (int i = 0; i < sortedList.Count; i++)
            {
                int origIndex = _gizmosToDraw.IndexOf(sortedList[i]);
                sortedBlocks[i] = _propertyBlocks[origIndex];
            }

            _gizmosToDraw.Clear();
            _gizmosToDraw.AddRange(sortedList);
            _propertyBlocks.Clear();
            _propertyBlocks.AddRange(sortedBlocks);
        }

        public static void DrawGizmo(Mesh mesh, Matrix4x4 matrix, Color color)
        {
#if UNITY_EDITOR
            if (!Application.isEditor) return;
            if (mesh == null) return;

            EnsureInstanceExists();

            _gizmosToDraw.Add(new GizmoData
            {
                mesh = mesh,
                matrix = matrix,
                color = color
            });
#endif
        }

        public static void DrawMesh(Mesh mesh, Color color)
        {
            if (!Application.isEditor) return;
            // 添加到绘制队列
            DrawGizmo(mesh, Matrix4x4.identity, color);
        }

        private static void EnsureInstanceExists()
        {
#if UNITY_EDITOR
            if (_instance != null) return;

            _instance = FindObjectOfType<GizmoRenderer>();

            if (_instance == null)
            {
                var go = new GameObject("GizmoRenderer");
                go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                _instance = go.AddComponent<GizmoRenderer>();
            }
#endif
        }
#endif
    }
}

