using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public interface IComponentManager
    {
        T GetComponent4Entity<T>(int entityId) where T : ComponentBase;

        int GetAllComponents4Entity(int entityId, ComponentBase[] array);

        IcomponentFactory GetComponentCreater();
    }
}
