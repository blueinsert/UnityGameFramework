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
    [BurstCompile]
    public struct StretchConstrainApplyJob : IJobParallelFor
    {
        /// <summary>
        /// �߶�����������
        /// </summary>
        [ReadOnly] public NativeArray<int2> m_edges;
        /// <summary>
        /// ����λ������
        /// </summary>
        [ReadOnly] public NativeArray<float4> m_positions;
        /// <summary>
        /// ����Լ����������λ�ñ仯
        /// </summary>
        [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<float4> m_deltas;
        /// <summary>
        /// ÿ�����㱻�ۼƴ���
        /// </summary>
        [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<int> m_counts;

        [ReadOnly] public float sorFactor;

        public void Execute(int index)
        {
            var e = m_edges[index];
            var i = e[0];
            var j = e[1];
            if (m_counts[i] > 0)
            {
                m_positions[i] += m_deltas[i] / m_counts[i];
                m_deltas[i] = float4.zero;
                m_counts[i] = 0;
            }
            if (m_counts[j] > 0)
            {
                m_positions[j] += m_deltas[j] / m_counts[j];
                m_deltas[j] = float4.zero;
                m_counts[j] = 0;
            }
        }
    }
}
