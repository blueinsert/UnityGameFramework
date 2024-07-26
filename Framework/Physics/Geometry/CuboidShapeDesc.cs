using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public class CuboidShapeDesc : MonoBehaviour
    {
        static Vector4[] s_points = new Vector4[8] {
            new Vector4(-0.5f, -0.5f, -0.5f, 1),
            new Vector4(-0.5f, -0.5f, 0.5f, 1),
            new Vector4(0.5f, -0.5f, 0.5f, 1),
            new Vector4(0.5f, -0.5f, -0.5f, 1),
            new Vector4(-0.5f, 0.5f, -0.5f, 1),
            new Vector4(-0.5f, 0.5f, 0.5f, 1),
            new Vector4(0.5f, 0.5f, 0.5f, 1),
            new Vector4(0.5f, 0.5f, -0.5f, 1)
        };

        public Vector3 m_position = Vector3.zero;
        public Vector3 m_rotation = Vector3.zero;
        public Vector3 m_scale = Vector3.one;

        public Transform m_parent;
        public bool m_isDrawGizmons = false;

        public void Awake()
        {
            //m_parent = this.transform;
        }

        public Matrix4x4 TransformMatrix
        {
            get
            {
                Matrix4x4 m1 = m_parent == null ? Matrix4x4.identity : m_parent.localToWorldMatrix;
                Quaternion qua = Quaternion.Euler(m_rotation.x, m_rotation.y, m_rotation.z);
                Matrix4x4 m2 = new Matrix4x4();
                m2.SetTRS(m_position, qua, m_scale);
                return m1 * m2;
            }
        }

        public CuboidShape GetShape()
        {
            CuboidShape shape = new CuboidShape();
            Matrix4x4 m = TransformMatrix;
            for (int i = 0; i < s_points.Length; i++)
            {
                Vector3 p = m * s_points[i];
                shape[i] = p;
            }
            return shape;
        }

        void OnDrawGizmos()
        {
            if (!m_isDrawGizmons) return;
            Gizmos.color = Color.blue;
            var shape = GetShape();
            DrawCuboid(shape);
        }

        void DrawCuboid(CuboidShape points)
        {
            //下表面
            Mesh quad = CreateQuad(points[3], points[2], points[1], points[0]);
            Gizmos.DrawMesh(quad);
            //上表面
            quad = CreateQuad(points[4], points[5], points[6], points[7]);
            Gizmos.DrawMesh(quad);
            //左表面
            quad = CreateQuad(points[5], points[4], points[0], points[1]);
            Gizmos.DrawMesh(quad);
            //右表面
            quad = CreateQuad(points[7], points[6], points[2], points[3]);
            Gizmos.DrawMesh(quad);
            //前表面
            quad = CreateQuad(points[4], points[7], points[3], points[0]);
            Gizmos.DrawMesh(quad);
            //后表面
            quad = CreateQuad(points[6], points[5], points[1], points[2]);
            Gizmos.DrawMesh(quad);
        }

        Mesh CreateQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Vector3[] vertices = {
            p1,p2,p3,p4
        };
            int[] indices = new int[6] { 0, 1, 3, 1, 2, 3 };
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
