using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace bluebean.UGFramework.Physics
{
    [BurstCompile]
    public struct VolumeConstrainApplyJob : IJobParallelFor
    {
        /// <summary>
        /// 约束在四面体顶点数组中的开始索引
        /// </summary>
        [ReadOnly] public NativeArray<int> m_tetStartIndex;
        /// <summary>
        /// 四面体顶点数组
        /// </summary>
        [ReadOnly] public NativeArray<int> m_tets;

        [ReadOnly] public float sorFactor;

        /// <summary>
        /// 粒子位置数组
        /// </summary>
        [ReadOnly] public NativeArray<float4> m_positions;

        /// <summary>
        /// 本次约束求解产生的位置梯度
        /// </summary>
        [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<float4> m_gradients;
        /// <summary>
        /// 本次约束求解产生的位置变化
        /// </summary>
        [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<float4> m_deltas;
        /// <summary>
        /// 每个顶点被累计次数
        /// </summary>
        [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] public NativeArray<int> m_counts;


        public void Execute(int index)
        {
            //index 是约束索引或四面体索引

            var sIndex = m_tetStartIndex[index];
            var p1Index = m_tets[sIndex];
            var p2Index = m_tets[sIndex + 1];
            var p3Index = m_tets[sIndex + 2];
            var p4Index = m_tets[sIndex + 3];
            Unity.Collections.NativeList<int> particleIndices = new Unity.Collections.NativeList<int>(4, Allocator.Temp);
            particleIndices.Add(p1Index);
            particleIndices.Add(p2Index);
            particleIndices.Add(p3Index);
            particleIndices.Add(p4Index);
            for (int j = 0; j < 4; j++)
            {
                var p = particleIndices[j];
                if (m_counts[p] > 0)
                {
                    m_positions[p] += m_deltas[p] / m_counts[p];
                    m_deltas[p] = float4.zero;
                    m_counts[p] = 0;
                }
            }
        }

    }
}
