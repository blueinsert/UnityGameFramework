using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public abstract class ComponentBase: IReuseable,IRollbackAble
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

        public virtual bool IsActive()
        {
            return m_isActive;
        }
        
        public void SetActive(bool isActive)
        {
            m_isActive = isActive;
        }

        public void SetOwnEntityId(int id)
        {
            m_ownEntityId = id;
        }

        public virtual void TakeSnapShot(SnapShotWriter writer)
        {
            writer.WriteBool(m_isActive);
            writer.WriteInt32(m_ownEntityId);
        }

        public virtual void RollBackTo(SnapShotReader reader)
        {
            m_isActive = reader.ReadBool();
            m_ownEntityId = reader.ReadInt32();
        }
    }
}