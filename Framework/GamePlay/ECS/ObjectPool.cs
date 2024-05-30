using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.UGFramework.GamePlay
{
    public interface IReuseable
    {
       bool IsActive();

        void SetActive(bool isActive);
    }

    public class ObjectPool<T> :IRollbackAble where T : class,IReuseable,IRollbackAble
    {
        public List<T> m_array;

        public ObjectPool(int count)
        {
            m_array = new List<T>(count);
            for(int i = 0; i < count; i++)
            {
               var instance = System.Activator.CreateInstance(typeof(T));
                m_array.Add(instance as T);
            }
        }

        public T Allocate()
        {
            for(int i = 0; i < m_array.Count; i++)
            {
                if (!m_array[i].IsActive())
                {
                    m_array[i].SetActive(true);
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

        public void TakeSnapShot(SnapShotWriter writer)
        {
            for (int i = 0; i < m_array.Count; i++)
            {
                m_array[i].TakeSnapShot(writer);
            }
        }
    }
}
