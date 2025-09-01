using bluebean.UGFramework.Asset;
using bluebean.UGFramework.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace bluebean.ProjectW.UI
{
    public abstract class UIDocumentViewController : UIViewController
    {
        private bool m_isDirty = false;

        protected VisualTreeAsset m_uiTree;
        protected abstract string GetUmlPath();
        protected abstract string GetStylePath();

        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
            m_uiTree = AssetUtility.Instance.GetAsset<VisualTreeAsset>(GetUmlPath());
            SetupUI();
        }

        public override void OnShow()
        {
            base.OnShow();
            if (m_isDirty)
            {
                SetupUI();
                m_isDirty = false;
            }
        }

        public override void OnHide()
        {
            base.OnHide();
            m_isDirty = true;
        }

        protected virtual void SetupUI()
        {
            UIDocumentManager.Instance.SetContent(m_uiTree);
            UIUtility.AttachUIController2UIDocument(this, UIDocumentManager.Instance.MainUIDocument.rootVisualElement);
            //var ss = AssetUtility.Instance.GetAsset<StyleSheet>(GetStylePath());
            //UIDocumentManager.Instance.MainUIDocument.rootVisualElement.styleSheets.Add(ss);
        }
    }
}
