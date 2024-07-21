using bluebean.UGFramework.DataStruct;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace bluebean.UGFramework.Physics
{
    public class StretchConstrainGroup : ConstrainGroup
    {
        /// <summary>
        /// 边顶点索引数组
        /// </summary>
        public NativeInt2List m_edgeList = new NativeInt2List();
        /// <summary>
        /// 每个约束(边)的初始长度
        /// </summary>
        public NativeFloatList m_restLenList = new NativeFloatList();
        /// <summary>
        /// 约束柔度
        /// </summary>
        public NativeFloatList m_complianceList = new NativeFloatList();

        private NativeArray<int2> m_edges;
        private NativeArray<float> m_restLens;
        private NativeArray<float> m_compliances;

        public StretchConstrainGroup(ISolver solver) : base(ConstrainType.Stretch, solver)
        {
            
        }

        private void OnConstrainCountChanged()
        {
            m_edges = m_edgeList.AsNativeArray<int2>();
            m_restLens = m_restLenList.AsNativeArray<float>();
            m_compliances = m_complianceList.AsNativeArray<float>();
        }

        public void AddConstrain(VectorInt2 edge, float restLen,float compliance)
        {
            m_edgeList.Add(edge);
            m_restLenList.Add(restLen);
            m_complianceList.Add(compliance);
            m_constrainCount++;

            OnConstrainCountChanged();
        }

        public override JobHandle Apply(JobHandle inputDeps, float substepTime)
        {
            var job = new StretchConstrainApplyJob() {
                m_edges = m_edges,
                m_positions = this.m_solver.ParticlePositions,
                m_deltas = this.m_solver.PositionDeltas,
                m_counts = this.m_solver.PositionConstraintCounts,
                m_particleProperties = this.m_solver.ParticleProperties,
            };
            return job.Schedule(m_constrainCount, 4, inputDeps);
        }

        public override JobHandle Solve(JobHandle inputDeps, float substepTime)
        {
            var job = new StretchConstrainSolveJob() {
                m_edges = this.m_edges,
                m_restLen = this.m_restLens,
                m_compliances = this.m_compliances,
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
