using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public interface IcomponentFactory
    {
        T CreateComponent<T>() where T: ComponentBase;
    }
}
