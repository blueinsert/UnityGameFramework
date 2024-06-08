using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public class SystemBase
    {
        /// <summary>
        /// 该系统所关注的实体列表
        /// </summary>
        private readonly List<Entity> m_attentionEnties = new List<Entity>();
        protected IEntityManager m_entityOwner = null;

        public SystemBase()
        {
        }

        public void SetEntityOwner(IEntityManager entityOwner)
        {
            m_entityOwner = entityOwner;
        }

        public void PreProcess()
        {
            var world = m_entityOwner;
            if (world.IsDirty())
            {
                m_attentionEnties.Clear();
                foreach (var entity in world.GetAllEntities())
                {
                    if (Filter(entity))
                    {
                        m_attentionEnties.Add(entity);
                    }
                }
            }
        }

        public void Process()
        {
            for(int i = 0; i < m_attentionEnties.Count; i++)
            {
                ProcessEntity(m_attentionEnties[i]);
            }
        }

        protected virtual void ProcessEntity(Entity entity)
        {

        }

        /// <summary>
        /// 所关注的实体的过滤器
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual bool Filter(Entity e)
        {
            return true;
        }
 
    }
}
