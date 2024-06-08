﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.GamePlay
{
    public class Entity : IReuseable
    {
        public int ID { get { return m_id; } }

        private int m_id = -1;

        private bool m_isActive = false;

        public Entity()
        {
            
        }

        public void SetId(int id)
        {
            m_id = id;
        }


        public bool IsInusing()
        {
            return m_isActive;
        }

        public void SetInusing(bool isActive)
        {
            m_isActive = isActive;
        }
    }
}
