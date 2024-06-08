using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.UGFramework.GamePlay
{
    public interface IReuseable
    {
       bool IsInusing();

        void SetInusing(bool isInusing);
    }

    public class ObjectPool<T> where T : class,IReuseable
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
                if (!m_array[i].IsInusing())
                {
                    m_array[i].SetInusing(true);
                    return m_array[i];
                }
            }
            Debug.LogError(string.Format("Allocate {0} failed", typeof(T).Name));
            return default(T);
        }

    }
}
