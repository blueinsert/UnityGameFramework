using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework.UI
{

    public class LoadingUIController : UIViewController
    {
        
        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
        }

        public void Show()
        {
            m_uiStateCtrl.SetUIState("show", () => { 

            });
        }

        void Update()
        {

        }

        [AutoBind("./")]
        UIStateController m_uiStateCtrl = null;
    }
}
