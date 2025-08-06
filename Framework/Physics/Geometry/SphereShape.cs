using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public struct SphereShape
    {
        public float m_radius;
        public Vector3 m_position;
        public AffineTransform m_local2WorldTransform;

        public Aabb m_aabb;

        public Aabb Aabb
        {
            get
            {
                return m_aabb;
            }
        }

        public Vector3 WorldPosition { 
            get
            {
                var matrix = m_local2WorldTransform.ToMatrix();
                var center = matrix.MultiplyPoint3x4(m_position);
                return center;
            } 
        }

        public Aabb WorldAabb
        {
            get
            {
                var matrix = m_local2WorldTransform.ToMatrix();
                var center = matrix.MultiplyPoint3x4(m_position);
                var radius = matrix.lossyScale[0] * m_radius;
                Aabb b = new Aabb();
                var min = center - new Vector3(radius, radius, radius);
                var max = center + new Vector3(radius, radius, radius);
                b.min = min;
                b.max = max;
                return b;
            }
        }

        public void UpdateAabb()
        {
            var min = m_position - new Vector3(m_radius, m_radius, m_radius);
            var max = m_position + new Vector3(m_radius, m_radius, m_radius);
            m_aabb.min = min;
            m_aabb.max = max;
        }
    }
}
