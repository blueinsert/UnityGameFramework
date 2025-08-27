using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework.UI
{
    public class DialogBoxUIController : UIViewController
    {
        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
            m_dialogBoxOkButton.onClick.AddListener(OnDialogBoxOkButtonClick);
            m_dialogBoxCancelButton.onClick.AddListener(OnDialogBoxCancelButtonClick);
            this.gameObject.SetActive(false);
        }

        public void ShowDialogBox(string tip)
        {
            m_dialogBoxText.text = tip;
            if (!m_dialogBoxStateCtrl.gameObject.activeSelf)
            {
                m_dialogBoxStateCtrl.gameObject.SetActive(true);
                m_dialogBoxStateCtrl.SetUIState("Show");
            }
        }

        public void HideDialogBox(System.Action onComplete = null)
        {
            m_dialogBoxStateCtrl.SetUIState("Close", () => {
                m_dialogBoxStateCtrl.gameObject.SetActive(false);
                if (onComplete != null)
                {
                    onComplete();
                }
            });
        }

        private void OnDialogBoxOkButtonClick()
        {
            HideDialogBox(() => {
                if (EventOnDialogBoxClose != null)
                {
                    EventOnDialogBoxClose(DialogBoxResult.OK);
                }
            });
        }

        private void OnDialogBoxCancelButtonClick()
        {
            HideDialogBox(() => {
                if (EventOnDialogBoxClose != null)
                {
                    EventOnDialogBoxClose(DialogBoxResult.Cancel);
                }
            });
        }

        public event Action<DialogBoxResult> EventOnDialogBoxClose;

        [AutoBind("./")]
        public UIStateController m_dialogBoxStateCtrl;
        [AutoBind("./Detail/DescText")]
        public Text m_dialogBoxText;
        [AutoBind("./Detail/ButtonOK")]
        public UnityEngine.UI.Button m_dialogBoxOkButton;
        [AutoBind("./Detail/ButtonCancel")]
        public UnityEngine.UI.Button m_dialogBoxCancelButton;
    }
}
