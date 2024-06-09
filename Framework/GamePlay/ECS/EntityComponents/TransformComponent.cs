using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public class TransformComponent : ComponentBase
    {
        public Vector3 Position { get { return m_position; } }
        public Vector3 Rotate { get { return m_rotate; } }

        private Vector3 m_rotate;
        private Vector3 m_position;

        public void PosSet(float x, float y, float z)
        {
            m_position.x = x;
            m_position.y = y;
            m_position.z = z;
        }

        public void PosAdd(float deltaX, float deltaY,float deltaZ)
        {
            m_position.x += deltaX;
            m_position.y += deltaY;
            m_position.z += deltaZ;
        }

        public void SetRotate(Vector3 rotate)
        {
            m_rotate = rotate;
        }
    }
}
