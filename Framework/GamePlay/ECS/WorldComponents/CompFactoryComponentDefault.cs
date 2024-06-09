using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    /// <summary>
    /// 用法展示
    /// </summary>
    public class CompFactoryComponentDefault : CompFactoryComponentBase
    {

        private CompManagerComponentDefault m_compManagerComponentDefault;

        public CompFactoryComponentDefault(TheWorldBase world) : base(world)
        {
        }


        public override void PostInitialize()
        {
            base.PostInitialize();
            m_compManagerComponentDefault = m_world.GetWorldComponent<CompManagerComponentDefault>();
        }

        public override T CreateComponent<T>()
        {
            return m_compManagerComponentDefault.AllocateComponent<T>();
        }
    }
}
