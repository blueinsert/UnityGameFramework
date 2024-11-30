using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Anim
{
    [ExecuteInEditMode]
    public class ConvexMeshAnim : MeshProceduralAnim
    {
        public float m_a = 2.0f;
        public float m_b = 2.0f;
        public float m_c = 2.0f;
        public float m_d = 2.0f;
        public float m_e = 2.0f;
        public override float ProceduralSample(Vector2 coordinate, float normalizedTime)
        {
            var temp = m_a * normalizedTime - m_b * Mathf.Abs(Mathf.Pow(coordinate.x, m_d)) - m_c * Mathf.Abs(Mathf.Pow(coordinate.y, m_e));
            if (temp < 0)
                return 0;
            return Mathf.Sqrt(temp);
        }
    }
}
