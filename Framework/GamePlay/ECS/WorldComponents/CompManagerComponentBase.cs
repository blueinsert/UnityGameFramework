using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public class CompManagerComponentBase : WorldComponentBase, IComponentManager
    {
        public CompManagerComponentBase(TheWorldBase world) : base(world)
        {
        }

        public IcomponentFactory GetComponentCreater()
        {
            throw new System.NotImplementedException();
        }

        public T GetComponent4Entity<T>(int entityId) where T : ComponentBase
        {
            throw new System.NotImplementedException();
        }
    }
}
