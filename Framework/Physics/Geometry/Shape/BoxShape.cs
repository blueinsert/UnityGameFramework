using bluebean.UGFramework.DataStruct;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public struct BoxShape
    {

        public static readonly Vector4[] s_localCorners = new Vector4[8] {
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

        private Aabb m_worldAabb;

        public Aabb WorldAabb
        {
            get
            {
                int count = 0;
                var matrix = m_local2WorldTransform.ToMatrix();
                foreach (var lp in s_localCorners)
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

        public Vector3 GetLocalCornerByIndex(int index)
        {
            // 检查索引是否越界  
            if (index < 0 || index > 7)
            {
                throw new IndexOutOfRangeException("Index was out of range.");
            }
            switch (index)
            {
                case 0: return s_localCorners[0];
                case 1: return s_localCorners[1];
                case 2: return s_localCorners[2];
                case 3: return s_localCorners[3];
                case 4: return s_localCorners[4];
                case 5: return s_localCorners[5];
                case 6: return s_localCorners[6];
                case 7: return s_localCorners[7];
            }
            return Vector3.zero;
        }


        public void GetWorldCorners(List<Vector3> worldCorners)
        {
            worldCorners.Clear();
            var l2w = m_local2WorldTransform.ToMatrix();
            foreach (var c in s_localCorners)
            {
                var wc = l2w.MultiplyPoint3x4(c);
                worldCorners.Add(wc);
            }
        }
    }
}
