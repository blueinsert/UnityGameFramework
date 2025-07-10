using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class UIViewController : MonoViewController
    {
        UITaskBase m_owner = null; 

        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
        }

        public void RegisterOwner(UITaskBase owner)
        {
            m_owner = owner;
        }
    }
}
