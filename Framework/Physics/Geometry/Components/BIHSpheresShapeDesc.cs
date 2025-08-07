using bluebean.UGFramework.DataStruct;
using bluebean.UGFramework.GamePlay;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public class BIHSpheresShapeDesc : MonoBehaviour
    {
        public BIHSpheres m_spheres = null;

        private MeshPool m_meshPool = new MeshPool();

        public bool m_isDrawGizmons = false;
        [Range(0, 10)]
        public int m_drawDepth = 3;

        private static Material _gizmoMaterial;
        private static Mesh _currentMesh;
        private static Color _currentColor;

        public void Build()
        {
            var sphereShapeDescs = this.gameObject.GetComponentsInChildren<SphereShapeDesc>();
            Build(sphereShapeDescs);
        }


        public void Build(SphereShapeDesc[] sphereShapeDescs)
        {
            var count = sphereShapeDescs.Length;
            if (count == 0)
            {
                m_spheres = null;
                return;
            }
            //构建BIHSpheres
            IBounded[] ts = new IBounded[count];
            Sphere[] spheres = new Sphere[count];
            for (int i = 0; i < count; i++)
            {
                var shpereShape = sphereShapeDescs[i].Shape;

                Sphere sphere = new Sphere(shpereShape.WorldPosition, shpereShape.Radius);
                sphere.SetIndex(i);
                ts[i] = sphere;
            }
            var bihNodes = BIH.Build(ref ts);
            for(int i = 0; i < ts.Length; i++)
            {
                spheres[i] = (Sphere)ts[i];
            }
            m_spheres = new BIHSpheres(bihNodes, spheres);

            //Debug.Log(BIH.BuildPartionLog(bihNodes));
            Debug.Log(BuildPartionLog(bihNodes, spheres));
        }


        private static string BuildPartionLog(BIHNode[] nodes, Sphere[] spheres)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                //leaf node
                if (node.firstChild == -1)
                {
                    StringBuilder pathSb = new StringBuilder();
                    pathSb.Append("[");
                    for (int j = node.start; j < node.start + node.count; j++)
                    {
                        pathSb.Append(spheres[j].index).Append(",");
                    }
                    pathSb.Append("]");
                    int parentIndex = node.parent;
                    while (parentIndex != -1)
                    {
                        pathSb.Insert(0, $"{parentIndex}-->");
                        parentIndex = nodes[parentIndex].parent;
                    }
                    sb.AppendLine(pathSb.ToString());
                }
            }
            return sb.ToString();
        }

        private void OnDestroy()
        {
            m_meshPool.Destroy();
        }

        #region Gizmos

        void OnDrawGizmos()
        {
            if (!m_isDrawGizmons) return;
            if (m_spheres == null || m_spheres.nodes.Length == 0)
                return;
            m_meshPool.BackAll();
            GizmonsUtil.DrawAabb(m_spheres.AABB, Color.green);

            if (m_spheres != null)
            {
                int depth = 1;
                if (m_drawDepth > 0)
                    TraverseBIHNodeList(m_spheres.spheres, m_spheres.nodes, 0, depth);
            }
        }

        void TraverseBIHNodeList(Sphere[] spheres, BIHNode[] nodes, int index, int depth)
        {
            var node = nodes[index];
            int start = node.start;
            int end = start + (node.count - 1);

            // calculate bounding box of all elements:
            Aabb b = node.m_aabb;

            if (node.firstChild != -1)
            {
                int axis = node.axis;
                float left = node.leftSplitPlane;
                float right = node.rightSplitPlane;
                DrawQuad(b, axis, left, Color.blue);
                DrawQuad(b, axis, right, Color.red);
                depth++;
                if (depth <= m_drawDepth)
                {
                    TraverseBIHNodeList(spheres, nodes, node.firstChild, depth);
                    TraverseBIHNodeList(spheres, nodes, node.firstChild + 1, depth);
                }
            }
        }

        void DrawQuad(Aabb aabb, int axis, float value, Color color)
        {
            Gizmos.color = color;
            var min = new Vector3(aabb.min.x, aabb.min.y, aabb.min.z);
            var max = new Vector3(aabb.max.x, aabb.max.y, aabb.max.z);
            var size = new Vector3(aabb.size.x, aabb.size.y, aabb.size.z);
            min[axis] = value;
            max[axis] = value;
            var dir1 = Vector3.one;
            var dir2 = Vector3.one;
            switch (axis)
            {
                case 0:
                    dir1 = new Vector3(0, size.y, 0);
                    dir2 = new Vector3(0, 0, size.z);
                    break;
                case 1:
                    dir1 = new Vector3(size.x, 0, 0);
                    dir2 = new Vector3(0, 0, size.z);
                    break;
                case 2:
                    dir1 = new Vector3(size.x, 0, 0);
                    dir2 = new Vector3(0, size.y, 0);
                    break;
            }
            Mesh quad = CreateQuad(min, min + dir2, max, max - dir2);
            GizmoRenderer.DrawMesh(quad, color);
            //Gizmos.DrawMesh(quad);
            //Graphics.DrawMeshNow(quad, Matrix4x4.identity);

            //DrawWithGraphicsAPI(quad, color);
            // 立即销毁临时创建的网格
            //DestroyImmediate(quad);
        }

        void DrawWithGraphicsAPI(Mesh mesh, Color color)
        {
            // 存储当前绘制状态
            _currentMesh = mesh;
            _currentColor = color;

            // 确保在渲染时调用
            SceneView.duringSceneGui -= RenderMeshInScene;
            SceneView.duringSceneGui += RenderMeshInScene;
        }

        void RenderMeshInScene(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (_currentMesh == null) return;

            // 获取材质
            Material material = GetDepthAwareMaterial(_currentColor);
            if (material == null) return;

            // 应用材质并绘制
            material.SetPass(0);
            Graphics.DrawMeshNow(_currentMesh, Matrix4x4.identity);

            // 清理状态
            _currentMesh = null;
        }

        Material GetDepthAwareMaterial(Color color)
        {
            if (_gizmoMaterial == null)
            {
                // 创建支持深度测试的自定义着色器
                var shader = Shader.Find("Hidden/DepthAwareGizmo");
                if (shader == null)
                {
                    // 如果找不到，使用内置着色器
                    shader = Shader.Find("Standard");
                }

                _gizmoMaterial = new Material(shader);
                _gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            // 配置材质属性
            _gizmoMaterial.color = color;
            _gizmoMaterial.SetInt("_ZWrite", 1); // 启用深度写入
            _gizmoMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);

            // 透明材质特殊处理
            if (color.a < 0.99f)
            {
                _gizmoMaterial.SetInt("_ZWrite", 0); // 透明部分禁用深度写入
                _gizmoMaterial.SetOverrideTag("RenderType", "Transparent");
                _gizmoMaterial.EnableKeyword("_ALPHABLEND_ON");
                _gizmoMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                _gizmoMaterial.DisableKeyword("_ALPHABLEND_ON");
                _gizmoMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }

            return _gizmoMaterial;
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            SceneView.duringSceneGui -= RenderMeshInScene;
#endif
        }

        Mesh CreateQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3[] vertices = {
            p1,p2,p3,p4,
             p1,p2,p3,p4
        };
            int[] indices = new int[12] { 
                0, 1, 3,
                1, 2, 3 ,
                4,7,5,
                5,7,6
            };
            Mesh mesh = m_meshPool.Allocate().m_mesh;// new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            return mesh;
        }



        #endregion
    }
}
