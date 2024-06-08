using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public interface IComponentManager
    {
        T GetComponent4Entity<T>(int entityId) where T : ComponentBase;

        IcomponentFactory GetComponentCreater();
    }
}
