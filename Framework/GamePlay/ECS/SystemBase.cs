﻿using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public class SystemBase
    {
        /// <summary>
        /// 该系统所关注的实体列表
        /// </summary>
        private readonly List<Entity> m_attentionEnties = new List<Entity>();

        public SystemBase()
        {
        }

        public void Process()
        {
            //var world = m_world;
            //if (world.IsDirty())
            //{
            //    m_attentionEnties.Clear();
            //    foreach(var entity in world.GetAllEntities())
            //    {
            //        if (Filter(entity))
            //        {
            //            m_attentionEnties.Add(entity);
            //        }
            //    }
            //}
            ProcessEntity(m_attentionEnties);
        }

        protected virtual void ProcessEntity(List<Entity> entities)
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
