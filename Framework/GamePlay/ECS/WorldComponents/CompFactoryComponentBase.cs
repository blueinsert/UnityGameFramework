using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public abstract class CompFactoryComponentBase : WorldComponentBase,IcomponentFactory
    {
        public CompFactoryComponentBase(TheWorldBase world) : base(world)
        {
        }

        public T CreateComponent<T>() where T : ComponentBase
        {
            throw new System.NotImplementedException();
        }
    }
}
