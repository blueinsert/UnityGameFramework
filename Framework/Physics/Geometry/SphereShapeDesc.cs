using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    [ExecuteInEditMode]
    public class SphereShapeDesc : MonoBehaviour
    {
        private SphereShape m_shape = new SphereShape();

        public Vector3 m_position;
        public float m_radius;
        public bool m_isDrawGizmons = false;

        public void UpdateShapeIfNeeded()
        {
            m_shape.m_position = m_position;
            m_shape.m_radius = m_radius;
            m_shape.m_local2World.FromTransform(this.transform);
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
            Gizmos.color = Color.blue;
            var pos = m_shape.m_local2World.translation;
            var center = new Vector3(pos.x,pos.y,pos.z) + m_shape.m_position;
            var radius = m_shape.m_local2World.scale[0] * m_shape.m_radius;
            Gizmos.DrawSphere(center, radius);
        }
    }
}