using bluebean.UGFramework;
using bluebean.UGFramework.Asset;
using bluebean.UGFramework.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace bluebean.ProjectW.UI
{
    public enum UIDocumentRenderType
    {
        Base,
        Overlay,
    }

    [RequireComponent(typeof(UIDocument))]
    public abstract class UIDocumentViewController : UIViewController
    {
        [AutoBind("./")]
        public UIDocument m_uiDocument = null;

        protected UIDocumentRenderType m_renderType = UIDocumentRenderType.Base;

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

        public UIDocumentRenderType GetRenderType()
        {
            return m_renderType;
        }

        protected void SetRenderType(UIDocumentRenderType renderType)
        {
            m_renderType = renderType;
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

        private void SetContent(VisualTreeAsset visualTreeAsset)
        {
            m_uiDocument.visualTreeAsset = visualTreeAsset;
        }

        //private void AddContent(VisualTreeAsset visualTreeAsset)
        //{
        //    m_uiDocument.rootVisualElement.Add(visualTreeAsset.Instantiate());
        //}

        public void ClearContent()
        {
            m_uiDocument.visualTreeAsset = null;
        }

        protected virtual void SetupUI()
        {
            var renderType = GetRenderType();
            if (renderType == UIDocumentRenderType.Base)
            {
                SetContent(m_uiTree);
            }
            //else
            //{
            //   AddContent(m_uiTree);
            //}
            UIUtility.AttachUIController2UIDocument(this, m_uiDocument.rootVisualElement);
            //var ss = AssetUtility.Instance.GetAsset<StyleSheet>(GetStylePath());
            //UIDocumentManager.Instance.MainUIDocument.rootVisualElement.styleSheets.Add(ss);
        }
    }
}
