using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public abstract class CompManagerComponentBase : WorldComponentBase, IComponentManager
    {
        public CompManagerComponentBase(TheWorldBase world) : base(world)
        {
        }

        public abstract IcomponentFactory GetComponentCreater();

        public abstract T GetComponent4Entity<T>(int entityId) where T : ComponentBase;

        public abstract int GetAllComponents4Entity(int entityId, ComponentBase[] array);
    }
}
