using System.Collections;
using System.Collections.Generic;
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
            var prev = Gizmos.matrix;
            var shape = m_shape;
            var local2WorldMatrix = Matrix4x4.TRS(shape.m_local2WorldTransform.translation, shape.m_local2WorldTransform.rotation, shape.m_local2WorldTransform.scale);
            local2WorldMatrix *= Matrix4x4.Translate(shape.m_position);
            Gizmos.matrix = local2WorldMatrix;
            Gizmos.DrawSphere(new Vector3(0, 0, 0), shape.m_radius);
            Gizmos.matrix = prev;
        }
    }
}