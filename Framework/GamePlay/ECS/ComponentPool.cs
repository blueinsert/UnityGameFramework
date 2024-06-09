using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.UGFramework.GamePlay
{

    public class ComponentPool<T> where T : ComponentBase, IReuseable
    {
        public List<T> m_array;

        public ComponentPool(int count)
        {
            m_array = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var instance = System.Activator.CreateInstance(typeof(T));
                m_array.Add(instance as T);
            }
        }

        public T Allocate()
        {
            for (int i = 0; i < m_array.Count; i++)
            {
                if (!m_array[i].IsInusing())
                {
                    m_array[i].SetInusing(true);
                    return m_array[i];
                }
            }
            Debug.LogError(string.Format("Allocate {0} failed", typeof(T).Name));
            return default(T);
        }

        public void Foreach(Action<ComponentBase> action)
        {
            for (int i = 0; i < m_array.Count; i++)
            {
                action(m_array[i]);
            }
        }

        public ComponentBase GetUsedComponent(int ownerId)
        {
            for (int i = 0; i < m_array.Count; i++)
            {
                var elem = m_array[i];
                if(elem.IsInusing() && elem.OwnEntityId == ownerId)
                {
                    return elem;
                }
            }
            return null;
        }
    }
}
