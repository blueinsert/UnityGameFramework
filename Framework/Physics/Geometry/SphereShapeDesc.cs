using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [ExecuteInEditMode]
    public class SphereShapeDesc : MonoBehaviour
    {
        public SphereShape Shape { get { return m_shape; } }

        private SphereShape m_shape = new SphereShape();

        public Vector3 m_position;
        public float m_radius;
        public bool m_isDrawGizmons = false;
        public Color m_gizmonsColor = Color.blue;

        public void UpdateShapeIfNeeded()
        {
            m_shape.m_position = m_position;
            m_shape.m_radius = m_radius;
            m_shape.m_local2WorldTransform.FromTransform(this.transform);
            m_shape.UpdateAabb();
        }

        void Start()
        {
            UpdateShapeIfNeeded();
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            UpdateShapeIfNeeded();
#endif
        }

        private void OnEnable()
        {
            UpdateShapeIfNeeded();
        }

        void OnDrawGizmos()
        {
            if (!m_isDrawGizmons) return;
            Gizmos.color = m_gizmonsColor ;

            var shape = m_shape;
            GizmonsUtil.DrawAabb(shape.WorldAabb, Color.green);
            var prev = Gizmos.matrix;
            var local2WorldMatrix = Matrix4x4.TRS(shape.m_local2WorldTransform.translation, shape.m_local2WorldTransform.rotation, shape.m_local2WorldTransform.scale);
            //local2WorldMatrix *= Matrix4x4.Translate(shape.m_position);
            Gizmos.matrix = local2WorldMatrix;
            GizmonsUtil.DrawAabb(shape.Aabb,Color.blue);
            Gizmos.color = m_gizmonsColor;
            Gizmos.DrawSphere(m_position, shape.m_radius);
            Gizmos.matrix = prev;

            
        }

        void DrawAabb(Aabb aabb)
        {
            var size = aabb.size.XYZ();
            var p0 = aabb.min.XYZ();
            var p1 = p0 + new Vector3(0, 0, size.z);
            var p2 = p0 + new Vector3(size.x, 0, size.z);
            var p3 = p0 + new Vector3(size.x, 0, 0);
            var p4 = p0 + new Vector3(0, size.y, 0);
            var p5 = p1 + new Vector3(0, size.y, 0);
            var p6 = p2 + new Vector3(0, size.y, 0);
            var p7 = p3 + new Vector3(0, size.y, 0);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);

            Gizmos.DrawLine(p4, p5);
            Gizmos.DrawLine(p5, p6);
            Gizmos.DrawLine(p6, p7);
            Gizmos.DrawLine(p7, p4);

            Gizmos.DrawLine(p0, p4);
            Gizmos.DrawLine(p3, p7);
            Gizmos.DrawLine(p2, p6);
            Gizmos.DrawLine(p1, p5);
        }
    }
}