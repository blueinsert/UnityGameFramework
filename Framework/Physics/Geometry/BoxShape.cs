using bluebean.UGFramework.DataStruct;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public struct BoxShape : IEnumerable<Vector3>
    {

        public static Vector4[] s_localPoints = new Vector4[8] {
            new Vector4(-0.5f, -0.5f, -0.5f, 1),
            new Vector4(-0.5f, -0.5f, 0.5f, 1),
            new Vector4(0.5f, -0.5f, 0.5f, 1),
            new Vector4(0.5f, -0.5f, -0.5f, 1),
            new Vector4(-0.5f, 0.5f, -0.5f, 1),
            new Vector4(-0.5f, 0.5f, 0.5f, 1),
            new Vector4(0.5f, 0.5f, 0.5f, 1),
            new Vector4(0.5f, 0.5f, -0.5f, 1)
        };

        public AffineTransform m_local2WorldTransform;

        public Aabb m_worldAabb;

        public Aabb WorldAabb { 
            get
            {
                int count = 0;
                var matrix = m_local2WorldTransform.ToMatrix();
                foreach(var lp in this)
                {
                    var wp = matrix.MultiplyPoint3x4(lp);
                    if (count == 0)
                    {
                        m_worldAabb.min = wp;
                        m_worldAabb.max = wp;
                    }
                    else
                        m_worldAabb.Encapsulate(wp);
                    count++;
                }
                return m_worldAabb;
            } 
        }

        //public CuboidShape() { }

        public Vector3 this[int index]
        {
            get
            {
                // 检查索引是否越界  
                if (index < 0 || index > 7)
                {
                    throw new IndexOutOfRangeException("Index was out of range.");
                }
                switch (index)
                {
                    case 0: return s_localPoints[0];
                    case 1: return s_localPoints[1];
                    case 2: return s_localPoints[2];
                    case 3: return s_localPoints[3];
                    case 4: return s_localPoints[4];
                    case 5: return s_localPoints[5];
                    case 6: return s_localPoints[6];
                    case 7: return s_localPoints[7];
                }
                return Vector3.zero;
            }
        }


        public IEnumerator<Vector3> GetEnumerator()
        {
            for(int i=0;i< s_localPoints.Length; i++)
            {
                yield return s_localPoints[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
