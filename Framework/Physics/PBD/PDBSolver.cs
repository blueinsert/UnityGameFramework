using bluebean.UGFramework.DataStruct;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using static UnityEditor.ShaderData;
using static UnityEngine.InputManagerEntry;

namespace bluebean.UGFramework.Physics
{
    public class PDBSolver : MonoBehaviour, ISolverEnv, ISolver
    {
        public int m_targetFrameRate = 60;
        public float m_dtSubStep = 0.0333f;
        public float m_dtStep = 0.0333f;
        public float m_damping = 0.99f;
        public float m_damping_subStep = 0.99f;
        [Range(0f, 0.1f)]
        public float m_edgeCompliance = 0.0f;
        [Range(0f, 1f)]
        public float m_volumeCompliance = 0.0f;
        [Range(1, 55)]
        public int m_subStep = 22;
        [Range(0f, 1f)]
        public float m_collideCompliance = 0.0f;
        [Header("�������ٶ�")]
        public Vector3 m_g = new Vector3(0, -9.8f, 0);

        public List<VolumeConstrain> m_volumeConstrains = new List<VolumeConstrain>();
        public List<StretchConstrain> m_stretchConstrains = new List<StretchConstrain>();
        public List<CollideConstrain> m_collideConstrains = new List<CollideConstrain>();

        public List<PDBActor> m_actors = new List<PDBActor>();
        public Dictionary<int, PDBActor> m_actorDic = new Dictionary<int, PDBActor>();

        // all particle positions [NonSerialized] private
        public NativeVector4List m_positions = new NativeVector4List();
        private NativeIntList m_freeList = new NativeIntList();

        public NativeVector4List ParticlePositions
        {
            get
            {
                return m_positions;
            }
        }

        public void Awake()
        {
            Application.targetFrameRate = m_targetFrameRate;
            m_dtStep = 1.0f / m_targetFrameRate;
            m_damping_subStep = Mathf.Pow(m_damping, 1.0f / m_subStep);
            m_dtSubStep = m_dtStep / m_subStep;
        }

        private void EnsureParticleArraysCapacity(int count)
        {
            // only resize if the count is larger than the current amount of particles:
            if (count >= m_positions.count)
            {
                m_positions.ResizeInitialized(count);
            }

            //if (count >= m_ParticleToActor.Length)
            //{
            //    Array.Resize(ref m_ParticleToActor, count * 2);
            //}
        }

        private void AllocateParticles(int[] particleIndices)
        {

            // If attempting to allocate more particles than we have:
            if (particleIndices.Length > m_freeList.count)
            {
                int grow = particleIndices.Length - m_freeList.count;

                // append new free indices:
                for (int i = 0; i < grow; ++i)
                    m_freeList.Add(m_positions.count + i);

                // grow particle arrays:
                EnsureParticleArraysCapacity(m_positions.count + particleIndices.Length);
            }

            // determine first particle in the free list to use:
            int first = m_freeList.count - particleIndices.Length;

            // copy free indices to the input array:
            m_freeList.CopyTo(particleIndices, first, particleIndices.Length);

            // shorten the free list:
            m_freeList.ResizeUninitialized(first);

        }

        public void AddActor(PDBActor actor)
        {
            if (!m_actorDic.ContainsKey(actor.ActorId))
            {
                m_actorDic.Add(actor.ActorId, actor);
                m_actors.Add(actor);

                actor.m_particleIndicesInSolver = new int[actor.ParcileCount];

                AllocateParticles(actor.m_particleIndicesInSolver);

                //load init position
                for(int i = 0; i < actor.ParcileCount; i++)
                {
                    var index = actor.m_particleIndicesInSolver[i];
                    var pos = actor.GetParticleInitPosition(i);
                    this.m_positions[index] = pos;
                }
            }   
        }

        public void RemoveActor(PDBActor actor)
        {
            m_actors.Remove(actor);
            m_actorDic.Remove(actor.ActorId);
        }

        public void AddConstrain(ConstrainBase constrain)
        {
            constrain.SetSolveEnv(this);
            switch (constrain.m_type)
            {
                case ConstrainType.Stretch:
                    m_stretchConstrains.Add(constrain as StretchConstrain);
                    break;
                case ConstrainType.Volume:
                    m_volumeConstrains.Add(constrain as VolumeConstrain);
                    break;
                case ConstrainType.Collide:
                    m_collideConstrains.Add(constrain as CollideConstrain);
                    break;
            }
        }

        private void ClearStretchConstrains(int actorId)
        {
            List<StretchConstrain> toRemoves = new List<StretchConstrain>();
            foreach (var constrain in m_stretchConstrains)
            {
                if (constrain.m_actorId == actorId)
                {
                    toRemoves.Add(constrain);
                }
            }
            foreach (var constrain in toRemoves)
            {
                m_stretchConstrains.Remove(constrain);
            }

        }

        private void ClearVolumeConstrains(int actorId)
        {
            List<VolumeConstrain> toRemoves = new List<VolumeConstrain>();
            foreach (var constrain in m_volumeConstrains)
            {
                if (constrain.m_actorId == actorId)
                {
                    toRemoves.Add(constrain);
                }
            }
            foreach (var constrain in toRemoves)
            {
                m_volumeConstrains.Remove(constrain);
            }

        }

        private void ClearCollideConstrains(int actorId)
        {
            List<CollideConstrain> toRemoves = new List<CollideConstrain>();
            foreach (var constrain in m_collideConstrains)
            {
                if (constrain.m_actorId == actorId)
                {
                    toRemoves.Add(constrain);
                }
            }
            foreach (var constrain in toRemoves)
            {
                m_collideConstrains.Remove(constrain);
            }

        }

        public void ClearConstrain(int actorId, ConstrainType type)
        {
            switch (type)
            {
                case ConstrainType.Stretch:
                    ClearStretchConstrains(actorId);
                    break;
                case ConstrainType.Volume:
                    ClearVolumeConstrains(actorId);
                    break;
                case ConstrainType.Collide:
                    ClearCollideConstrains(actorId);
                    break;
            }
        }

        void PreSubStep()
        {
            for (int i = 0; i < m_actors.Count; i++)
            {
                m_actors[i].PreSubStep(m_dtSubStep, m_g);
            }
        }

        void PostSubStep()
        {
            for (int i = 0; i < m_actors.Count; i++)
            {
                m_actors[i].PostSubStep(m_dtSubStep, m_damping_subStep);
            }
        }

        void PreStep()
        {
            for (int i = 0; i < m_actors.Count; i++)
            {
                m_actors[i].PreStep();
            }
        }

        void PostStep()
        {
            for (int i = 0; i < m_actors.Count; i++)
            {
                m_actors[i].PostStep();
            }
        }

        void SolveStrethConstrains()
        {
            foreach (var constrain in m_stretchConstrains)
            {
                constrain.SetParams(m_edgeCompliance);
                constrain.Solve(m_dtSubStep);
            }
        }

        void SolveVolumeConstrains()
        {
            foreach (var constrain in m_volumeConstrains)
            {
                constrain.SetParams(m_volumeCompliance);
                constrain.Solve(m_dtSubStep);
            }
        }

        void SolveCollideConstrains()
        {
            foreach (var constrain in m_collideConstrains)
            {
                constrain.SetParams(m_collideCompliance);
                constrain.Solve(m_dtSubStep);
            }
        }

        void Solve()
        {
            Profiler.BeginSample("SolveStrethConstrains");
            SolveStrethConstrains();
            Profiler.EndSample();

            Profiler.BeginSample("SolveVolumeConstrains");
            SolveVolumeConstrains();
            Profiler.EndSample();

            Profiler.BeginSample("SolveCollideConstrains");
            SolveCollideConstrains();
            Profiler.EndSample();
        }

        void SubStep()
        {
            Profiler.BeginSample("PreSubStep");
            PreSubStep();
            Profiler.EndSample();

            Profiler.BeginSample("Solve");
            Solve();
            Profiler.EndSample();

            Profiler.BeginSample("PostSubStep");
            PostSubStep();
            Profiler.EndSample();
        }

        void Step()
        {
            Profiler.BeginSample("PreStep");
            PreStep();
            Profiler.EndSample();
            Profiler.BeginSample("SubStep");
            for (int i = 0; i < m_subStep; i++)
            {
                SubStep();
            }
            Profiler.EndSample();
            Profiler.BeginSample("PreStep");
            PostStep();
            Profiler.EndSample();
        }

        // Update is called once per frame
        void Update()
        {
            Step();
        }

        #region ISolverEnv
        public PDBActor GetActor(int id)
        {
            return m_actorDic[id];
        }

        public Vector3 GetParticlePosition(int actorId, int particleId)
        {
            var actor = GetActor(actorId);
            return actor.GetParticlePosition(particleId);
        }

        public float GetParticleInvMass(int actorId, int particleId)
        {
            var actor = GetActor(actorId);
            return actor.GetParticleInvMass(particleId);
        }

        public void ModifyParticelPosition(int actorId, int particleId, Vector3 deltaPos)
        {
            var actor = GetActor(actorId);
            actor.ModifyParticelPosition(particleId, deltaPos);
        }

        public Vector4Int GetTetVertexIndex(int actorId, int tetIndex)
        {
            var actor = GetActor(actorId);
            return actor.GetTetVertexIndex(tetIndex);
        }

        public float GetTetRestVolume(int actorId, int tetIndex)
        {
            var actor = GetActor(actorId);
            return actor.GetTetRestVolume(tetIndex);
        }

        public float GetEdgeRestLen(int actorId, int edgeIndex)
        {
            var actor = GetActor(actorId);
            return actor.GetEdgeRestLen(edgeIndex);
        }

        public Vector2Int GetEdgeParticles(int actorId, int edgeIndex)
        {
            var actor = GetActor(actorId);
            return actor.GetEdgeParticles(edgeIndex);
        }
        #endregion
    }
}