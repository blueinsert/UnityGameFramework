using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [ExecuteInEditMode]
    public class SphereShapeDesc : ShapeDescBase
    {
        public SphereShape Shape { get { return m_shape; } }

        private SphereShape m_shape = new SphereShape();

        public Vector3 m_position;
        public float m_radius;
        public bool m_isDrawGizmons = false;
        public Color m_gizmonsColor = Color.blue;

        void Start()
        {
            SetDirty();
        }

        private void OnValidate()
        {
            SetDirty();
        }

        public override void UpdateShapeImpl()
        {
            m_shape.m_position = m_position;
            m_shape.m_radius = m_radius;
            m_shape.m_local2WorldTransform.FromTransform(this.transform);
            m_shape.UpdateAabb();
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

       
    }
}