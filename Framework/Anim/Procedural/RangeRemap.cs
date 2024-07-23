using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Anim
{
    public class RangeRemap : MonoBehaviour
    {
        public Vector2 m_sourceRange;
        public Vector2 m_destRange;

        public float Remap(float value)
        {
            float res = value;
            float normalized = (value - m_sourceRange.x) / (m_sourceRange.y - m_sourceRange.x);
            res = m_destRange.x + normalized * (m_destRange.y - m_destRange.x);
            res = Mathf.Clamp(res, m_destRange.x, m_destRange.y);
            return res;
        }
    }
}
