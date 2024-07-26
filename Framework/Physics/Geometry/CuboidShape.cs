using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public struct CuboidShape : IEnumerable<Vector3>
    {
        public Vector3 m_p0;
        public Vector3 m_p1;
        public Vector3 m_p2;
        public Vector3 m_p3;
        public Vector3 m_p4;
        public Vector3 m_p5;
        public Vector3 m_p6;
        public Vector3 m_p7;

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
                    case 0: return m_p0;
                    case 1: return m_p1;
                    case 2: return m_p2;
                    case 3: return m_p3;
                    case 4: return m_p4;
                    case 5: return m_p5;
                    case 6: return m_p6;
                    case 7: return m_p7;
                }
                return Vector3.zero;
            }
            set
            {
                // 检查索引是否越界  
                if (index < 0 || index > 7)
                {
                    throw new IndexOutOfRangeException("Index was out of range.");
                }
                switch (index)
                {
                    case 0: m_p0 = value; break;
                    case 1: m_p1 = value; break;
                    case 2: m_p2 = value; break;
                    case 3: m_p3 = value; break;
                    case 4: m_p4 = value; break;
                    case 5: m_p5 = value; break;
                    case 6: m_p6 = value; break;
                    case 7: m_p7 = value; break;
                }
            }
        }


        public IEnumerator<Vector3> GetEnumerator()
        {
            yield return m_p0;
            yield return m_p1;
            yield return m_p2;
            yield return m_p3;
            yield return m_p4;
            yield return m_p5;
            yield return m_p6;
            yield return m_p7;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
