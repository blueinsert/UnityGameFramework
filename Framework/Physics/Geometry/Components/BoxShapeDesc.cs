using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [ExecuteInEditMode]
    public class BoxShapeDesc : ShapeDescBase
    {
        public BoxShape Shape { get { return m_shape; } }

        private BoxShape m_shape = new BoxShape();

        public Vector3 m_position = Vector3.zero;
        public Vector3 m_rotation = Vector3.zero;
        public Vector3 m_scale = Vector3.one;

        public Transform m_parent;
        public bool m_isDrawGizmons = false;

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

        private void OnValidate()
        {
            SetDirty();
        }

        void Start()
        {
            SetDirty();
        }

        void Update()
        {
            UpdateShapeIfNeeded();
        }

        public override void UpdateShapeImpl()
        {
            var local2World = TransformMatrix;

            m_shape.m_local2WorldTransform.FromMatrix(local2World);
        }

        public override bool IsNeededUpdate()
        {
            return base.IsNeededUpdate();
        }

        void OnDrawGizmos()
        {
            if (!m_isDrawGizmons) return;

            GizmonsUtil.DrawAabb(m_shape.WorldAabb, Color.green);

            Gizmos.color = Color.blue;
            var prev = Gizmos.matrix;
            var shape = m_shape;
            var local2WorldMatrix = Matrix4x4.TRS(shape.m_local2WorldTransform.translation, shape.m_local2WorldTransform.rotation, shape.m_local2WorldTransform.scale);
            Gizmos.matrix = local2WorldMatrix;
           
            DrawCuboid(m_shape);

            Gizmos.matrix = prev;
        }

        void DrawCuboid(BoxShape box)
        {
            //下表面
            Mesh quad = CreateQuad(box[3], box[2], box[1], box[0]);
            Gizmos.DrawMesh(quad);
            //上表面
            quad = CreateQuad(box[4], box[5], box[6], box[7]);
            Gizmos.DrawMesh(quad);
            //左表面
            quad = CreateQuad(box[5], box[4], box[0], box[1]);
            Gizmos.DrawMesh(quad);
            //右表面
            quad = CreateQuad(box[7], box[6], box[2], box[3]);
            Gizmos.DrawMesh(quad);
            //前表面
            quad = CreateQuad(box[4], box[7], box[3], box[0]);
            Gizmos.DrawMesh(quad);
            //后表面
            quad = CreateQuad(box[6], box[5], box[1], box[2]);
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
