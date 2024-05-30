using System;
using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public class Entity: IReuseable,IRollbackAble
    {
        public int ID { get { return m_id; } }

        public List<ComponentBase> Components
        {
            get
            {
                return new List<ComponentBase>(m_componentDic.Values);
            }
        }
        private int m_id = -1;
        private readonly Dictionary<string, ComponentBase> m_componentDic = new Dictionary<string, ComponentBase>();

        private bool m_isActive = false;

        public Entity()
        {
            
        }

        public void ClearCompRefDic()
        {
            m_componentDic.Clear();
        }

        public void AddOwnCompRef(ComponentBase component)
        {
            m_componentDic.Add(component.GetType().Name, component);
        }

        public void SetId(int id)
        {
            m_id = id;
        }

        public T GetComponent<T>() where T : ComponentBase
        {
            ComponentBase res;
            if(m_componentDic.TryGetValue(typeof(T).Name, out res))
            {
                return res as T;
            }
            return null;
        }

        public T AddComponent<T>() where T : ComponentBase,new()
        {
            //var component = m_world.AllocateComponent<T>();
            //component.SetOwnEntityId(this.ID);
            //AddOwnCompRef(component);
            //return component;
            return null;
        }

        public bool IsActive()
        {
            return m_isActive;
        }

        public void SetActive(bool isActive)
        {
            m_isActive = isActive;
        }

        public void TakeSnapShot(SnapShotWriter writer)
        {
            writer.WriteBool(m_isActive);
            writer.WriteInt32(m_id);
        }

        public void RollBackTo(SnapShotReader reader)
        {
            m_isActive = reader.ReadBool();
            m_id = reader.ReadInt32();
        }
    }
}
