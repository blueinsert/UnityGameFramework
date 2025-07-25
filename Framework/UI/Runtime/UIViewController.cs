﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class UIViewController : MonoViewController
    {
        protected UITaskBase m_ownerTask = null; 

        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
        }

        public void RegisterOwner(UITaskBase owner)
        {
            m_ownerTask = owner;
        }
    }
}
