using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Physics
{
    [RequireComponent(typeof(TetMesh))]
    public class PDBActor : MonoBehaviour
    {
        public int m_actorId;
        public int ActorId { get { return m_actorId; } }

        //runtime data
        protected Vector3[] m_X_last = null;
        protected Vector3[] m_X = null;
        protected Vector3[] m_V = null;

        public int ParcileCount { get { return m_X.Length; } }

        /// <summary>
        /// 每个粒子在solver positions数组中的索引
        /// </summary>
        //[HideInInspector]
        public int[] m_particleIndicesInSolver;

        protected TetMesh m_tetMesh = null;

        public MeshFilter m_meshFilter = null;
        public Mesh m_mesh = null;

        public Vector3 GetParticlePosition(int index)
        {
            return m_X[index];
        }

        public Vector3 GetParticleInitPosition(int particleIndex)
        {
            return m_tetMesh.GetParticlePos(particleIndex);
        }

        public void ModifyParticelPosition(int particleId, Vector3 deltaPos)
        {
            if (!m_tetMesh.IsParticleFixed(particleId))
            {
                m_X[particleId] += deltaPos;
            }
        }

        public virtual float GetEdgeRestLen(int edgeIndex)
        {
            return m_tetMesh.GetEdgeRestLen(edgeIndex);
        }

        public float GetParticleInvMass(int particleId)
        {
            return m_tetMesh.GetParticleInvMass(particleId);
        }

        public Vector4Int GetTetVertexIndex(int tetIndex)
        {
            return m_tetMesh.GetTetVertexIndex(tetIndex);
        }

        public virtual float GetTetRestVolume(int tetIndex)
        {
            return m_tetMesh.GetTetRestVolume(tetIndex);
        }

        public Vector2Int GetEdgeParticles(int edgeIndex)
        {
            return m_tetMesh.GetEdgeParticles(edgeIndex);
        }

        public virtual void Initialize()
        {
            m_tetMesh = GetComponent<TetMesh>();

            m_X = m_tetMesh.m_pos;
            m_X_last = new Vector3[m_X.Length];
            Util.CopyArray(m_X, m_X_last);
            m_V = new Vector3[m_X.Length];

            m_meshFilter = GetComponent<MeshFilter>();
            m_mesh = m_meshFilter.mesh;
        }

        public virtual void PreSubStep(float dt, Vector3 g)
        {
            Util.CopyArray(m_X, m_X_last);
            for (int i = 0; i < m_X.Length; i++)
            {
                if (!m_tetMesh.IsParticleFixed(i))
                {
                    m_V[i] += g * dt;
                    m_X[i] += m_V[i] * dt;
                }
            }

        }

        public virtual void PostSubStep(float dt, float velDamp)
        {
            for (int i = 0; i < m_X.Length; i++)
            {
                m_V[i] = (m_X[i] - m_X_last[i]) / dt;
                m_V[i] *= velDamp;
            }
        }

        public virtual void PreStep() { }

        public virtual void PostStep()
        {

            SyncMesh();
        }

        public void SyncMesh()
        {
            m_mesh.vertices = m_X;
            //Debug.Log($"y:{m_X[0].y} vy:{m_V[0].y}");
            m_mesh.RecalculateNormals();
        }
    }
}
