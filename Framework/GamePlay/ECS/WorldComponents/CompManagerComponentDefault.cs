using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    /// <summary>
    /// 用法展示
    /// </summary>
    public class CompManagerComponentDefault : CompManagerComponentBase
    {
        private const int EntityCapcity = 100;

        private ComponentPool<TransformComponent> m_transformPool = null;


        public CompManagerComponentDefault(TheWorldBase world) : base(world)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            m_transformPool = new ComponentPool<TransformComponent>(EntityCapcity);
        }

        public override IcomponentFactory GetComponentCreater()
        {
            var comp = m_world.CreateWorldComponent<CompFactoryComponentDefault>();
            return comp;
        }

        public override int GetAllComponents4Entity(int entityId, ComponentBase[] array)
        {
            if(array == null)
            {
                array = new ComponentBase[EntityCapcity];
            }
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = null;
            }
            int count = 0;
            m_transformPool.Foreach((elem) => {
                if(elem.IsInusing() && elem.OwnEntityId == entityId)
                {
                    array[count] = elem;
                    count++;
                }
            });
            //
            return count;
        }

        public override T GetComponent4Entity<T>(int entityId)
        {
            if (typeof(T) == typeof(TransformComponent))
            {
                var res = m_transformPool.GetUsedComponent(entityId);
                return res as T;
            }
            //
            return null;
        }

        public T AllocateComponent<T>() where T:ComponentBase
        {
            if (typeof(T) == typeof(TransformComponent))
            {
                var res = m_transformPool.Allocate();
                return res as T;
            }
            //
            return null;
        }

    }
}
