using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public abstract class WorldComponentBase
    {
        protected TheWorldBase m_world = null;

        public WorldComponentBase(TheWorldBase world)
        {
            m_world = world;
        }

        public virtual void PreInitialize() { }
        public virtual void Initialize() { }

        public virtual void PoseInitialize() { }
    }
}
