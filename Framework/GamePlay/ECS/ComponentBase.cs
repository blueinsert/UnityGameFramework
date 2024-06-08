using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public abstract class ComponentBase: IReuseable
    {
        public int OwnEntityId
        {
            get
            {
                return m_ownEntityId;
            }
        }

        protected bool m_isActive = false;
        protected int m_ownEntityId = -1;

        public ComponentBase()
        {
        }

        public virtual bool IsInusing()
        {
            return m_isActive;
        }
        
        public void SetInusing(bool isActive)
        {
            m_isActive = isActive;
        }

        public void SetOwnEntityId(int id)
        {
            m_ownEntityId = id;
        }
    }
}