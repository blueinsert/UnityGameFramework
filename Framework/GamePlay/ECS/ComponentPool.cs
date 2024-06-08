using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.UGFramework.GamePlay
{

    public class ComponentPool<T> : IRollbackAble where T : ComponentBase, IReuseable, IRollbackAble
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

        public void RollBackTo(SnapShotReader reader)
        {
            for (int i = 0; i < m_array.Count; i++)
            {
                m_array[i].RollBackTo(reader);
            }
        }

        public void RecoverRefInEntity()
        {
            foreach (var comp in m_array)
            {
                if (comp.IsInusing())
                {
                    //var e = m_battleWorld.GetEntityById(comp.OwnEntityId);
                    //if (e == null)
                    //{
                    //    Debug.LogError(string.Format("m_battleWorld.GetEntityById failed,entityId:{0}", comp.OwnEntityId));
                    //}
                    //if (e != null)
                    //    e.AddOwnCompRef(comp);
                }
            }
        }

        public void TakeSnapShot(SnapShotWriter writer)
        {
            for (int i = 0; i < m_array.Count; i++)
            {
                m_array[i].TakeSnapShot(writer);
            }
        }
    }
}
