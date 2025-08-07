using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public struct SphereShape
    {
        public float m_localRadius;
        public Vector3 m_localPosition;
        public AffineTransform m_local2WorldTransform;

        public Aabb m_localAabb;

        public Aabb LocalAabb
        {
            get
            {
                return m_localAabb;
            }
        }

        public Vector3 WorldPosition { 
            get
            {
                var matrix = m_local2WorldTransform.ToMatrix();
                var center = matrix.MultiplyPoint3x4(m_localPosition);
                return center;
            } 
        }

        public float Radius
        {
            get
            {
                var matrix = m_local2WorldTransform.ToMatrix();
                var radius = matrix.lossyScale[0] * m_localRadius;
                return radius;
            }
        }

        public Aabb WorldAabb
        {
            get
            {
                var matrix = m_local2WorldTransform.ToMatrix();
                var center = matrix.MultiplyPoint3x4(m_localPosition);
                var radius = matrix.lossyScale[0] * m_localRadius;
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
            var min = m_localPosition - new Vector3(m_localRadius, m_localRadius, m_localRadius);
            var max = m_localPosition + new Vector3(m_localRadius, m_localRadius, m_localRadius);
            m_localAabb.min = min;
            m_localAabb.max = max;
        }
    }
}
