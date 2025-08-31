using bluebean.UGFramework.Asset;
using bluebean.UGFramework.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace bluebean.ProjectW.UI
{
    public abstract class UIDocumentViewController : UIViewController
    {
        protected abstract string GetUmlPath();
        protected abstract string GetStylePath();

        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
            var uiDocument = AssetUtility.Instance.GetAsset<VisualTreeAsset>(GetUmlPath());
            UIDocumentManager.Instance.SetContent(uiDocument);
            UIUtility.AttachUIController2UIDocument(this, UIDocumentManager.Instance.MainUIDocument.rootVisualElement);
            //var ss = AssetUtility.Instance.GetAsset<StyleSheet>(GetStylePath());
            //UIDocumentManager.Instance.MainUIDocument.rootVisualElement.styleSheets.Add(ss);
            SetupUI();
        }

        protected virtual void SetupUI()
        {

        }
    }
}
