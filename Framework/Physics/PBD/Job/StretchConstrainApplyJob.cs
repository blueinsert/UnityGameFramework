using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

namespace bluebean.UGFramework.Physics
{
    //[BurstCompile]
    public struct StretchConstrainApplyJob : IJobParallelFor
    {
        /// <summary>
        /// �߶�����������
        /// </summary>
        [ReadOnly] public NativeArray<int2> m_edges;

        [ReadOnly] public NativeArray<float4> m_particleProperties;
        [ReadOnly] public float sorFactor;

        /// <summary>
        /// ����λ������
        /// </summary>
        //[NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] 
        public NativeArray<float4> m_positions;
        /// <summary>
        /// ����Լ����������λ�ñ仯
        /// </summary>
        //[NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] 
        public NativeArray<float4> m_deltas;
        /// <summary>
        /// ÿ�����㱻�ۼƴ���
        /// </summary>
        //[NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] 
        public NativeArray<int> m_counts;

        
        public void Execute(int index)
        {
            var e = m_edges[index];
            var i = e[0];
            var j = e[1];
            if (m_counts[i] > 0)
            {
                //float4 property = m_particleProperties[i];
                //if(!PBDUtil.IsParticleFixed(property)) 
                {
                    m_positions[i] += m_deltas[i] / m_counts[i];
                }
                if (float.IsNaN(m_deltas[i].x))
                {
                    int k = -1;
                    Debug.LogError($"nan StretchConstrainApplyJob {index}");
                }
                m_deltas[i] = float4.zero;
                m_counts[i] = 0;
            }
            if (m_counts[j] > 0)
            {
                //float4 property = m_particleProperties[j];
                //if (!PBDUtil.IsParticleFixed(property))
                {
                    m_positions[j] += m_deltas[j] / m_counts[j];
                }
                m_deltas[j] = float4.zero;
                m_counts[j] = 0;
            }
        }
    }
}
