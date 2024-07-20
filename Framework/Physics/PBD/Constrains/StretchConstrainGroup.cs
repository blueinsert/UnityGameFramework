using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace bluebean.UGFramework.Physics
{
    public class StretchConstrainGroup : ConstrainGroup
    {
        /// <summary>
        /// �߶�����������
        /// </summary>
        public NativeInt2List m_edges = new NativeInt2List();
        /// <summary>
        /// ÿ��Լ��(��)�ĳ�ʼ����
        /// </summary>
        public NativeFloatList m_restLen = new NativeFloatList();
        /// <summary>
        /// Լ�����
        /// </summary>
        public NativeFloatList m_compliances = new NativeFloatList();

        public StretchConstrainGroup(ISolver solver) : base(ConstrainType.Stretch, solver)
        {

        }

        public void AddConstrain(VectorInt2 edge, float restLen,float compliance)
        {
            m_edges.Add(edge);
            m_restLen.Add(restLen);
            m_compliances.Add(compliance);
            m_constrainCount++;
        }

        public override JobHandle Apply(JobHandle inputDeps, float substepTime)
        {
            var job = new StretchConstrainApplyJob() {
                m_edges = this.m_edges.AsNativeArray<int2>(),
                m_positions = this.m_solver.ParticlePositions,
                m_deltas = this.m_solver.PositionDeltas,
                m_counts = this.m_solver.PositionConstraintCounts,
            };
            return job.Schedule(m_constrainCount, 4, inputDeps);
        }

        public override JobHandle Solve(JobHandle inputDeps, float substepTime)
        {
            var job = new StretchConstrainSolveJob() {
                m_edges = this.m_edges.AsNativeArray<int2>(),
                m_restLen = this.m_restLen.AsNativeArray<float>(),
                m_compliances = this.m_compliances.AsNativeArray<float>(),
                m_invMasses = this.m_solver.InvMasses,
                m_positions = this.m_solver.ParticlePositions,
                m_deltas = this.m_solver.PositionDeltas,
                m_counts = this.m_solver.PositionConstraintCounts,
                m_deltaTimeSqr = substepTime*substepTime,
            };
            return job.Schedule(m_constrainCount, 4, inputDeps);
        }
    }
}
