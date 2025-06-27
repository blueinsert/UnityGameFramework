using bluebean.ProjectE.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public enum DialogBoxResult
    {
        OK,
        Cancel,
    }

    public class DialogBoxUITask : UITaskBase
    {
        public DialogBoxUITask(string name) : base(typeof(DialogBoxUITask).Name)
        {
        }

        public static void StartUITask(string msg, Action<DialogBoxResult> cb)
        {
            UIIntent intent = new UIIntent(typeof(DialogBoxUITask).Name, false, false, false);
            intent.SetCustomParam(ParamKey_Msg, msg);
            intent.SetCustomParam(ParamKey_CB, cb);
            UIManager.Instance.StartUITask(intent);
        }

        #region UITask生命周期

        private void InitDataFromIntent(UIIntent curIntent)
        {
            m_msg = curIntent.GetCustomClassParam<string>(ParamKey_Msg);
            this.callback = curIntent.GetCustomClassParam<Action<DialogBoxResult>>(ParamKey_CB);
        }

        protected override void OnIntentChange(UIIntent prevIntent, UIIntent curIntent)
        {
            InitDataFromIntent(curIntent);
        }


        protected override bool OnStart(UIIntent intent)
        {
            bool res = base.OnStart(intent);
            return res;
        }

        protected override bool OnResume(UIIntent intent)
        {
            bool res = base.OnResume(intent);
            return res;
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override bool IsNeedUpdateCache()
        {
            return false;
        }

        protected override bool IsNeedLoadAssets()
        {
            return true;
        }

        protected override bool CollectAssetPathsToLoad()
        {
            return false;
        }

        protected override void OnAllViewControllerCreateCompleted()
        {
            base.OnAllViewControllerCreateCompleted();
            m_uiCtrl = m_viewControllerArray[0] as DialogBoxUIController;
            if (m_uiCtrl != null)
            {
                m_uiCtrl.EventOnDialogBoxClose += OnDialogBoxClose;
            }
        }

        protected override void OnAllAssetClear()
        {
            base.OnAllAssetClear();
            if (m_uiCtrl != null)
            {
                m_uiCtrl.EventOnDialogBoxClose -= OnDialogBoxClose;

            }
            m_uiCtrl = null;
        }

        private void PushAllLayer()
        {
            foreach (var layer in m_layerDic.Values)
            {
                if (layer.State != SceneLayerState.Using)
                {
                    SceneTreeManager.Instance.PushLayer(layer);
                }
            }
        }

        protected override void UpdateView()
        {
            PushAllLayer();

            m_uiCtrl.ShowDialogBox(m_msg);
        }

        protected override void OnTick()
        {

        }



        #endregion

        private void OnDialogBoxClose(DialogBoxResult result)
        {
            this.callback.Invoke(result);
            this.callback = null;
            //CloseAndReturn();
            Pause();
        }


        #region 资源描述

        protected override LayerDesc[] LayerDescArray
        {
            get
            {
                return m_layerDescs;
            }
        }

        private LayerDesc[] m_layerDescs = new LayerDesc[] {
            new LayerDesc(){
                LayerName = "DialogBoxUILayer",
                AssetPath = "Assets/Framework/UI/Resources/DialogBoxUIPrefab.prefab",
            },
        };

        protected override ViewControllerDesc[] ViewControllerDescArray
        {
            get { return m_viewControllerDescs; }
        }

        private ViewControllerDesc[] m_viewControllerDescs = new ViewControllerDesc[]{
            new ViewControllerDesc()
            {
                AtachLayerName = "DialogBoxUILayer",
                AtachPath = "./",
                TypeFullName = "bluebean.UGFramework.UI.DialogBoxUIController",
            },
        };
        #endregion

        private Action<DialogBoxResult> callback = null;
        private DialogBoxUIController m_uiCtrl = null;

        private string m_msg;


        public const string ParamKey_Msg = "ParamKey_Msg";
        public const string ParamKey_CB = "ParamKey_Msg";
    }
}
