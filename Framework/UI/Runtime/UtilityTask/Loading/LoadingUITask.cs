using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{

    public class LoadingUITask : UITaskBase
    {
        public LoadingUITask(string name) : base(name)
        {
        }

        public static void StartUITask(Action onEnd = null)
        {
            UIIntent intent = new UIIntent(typeof(LoadingUITask).Name, false, false, false);
            UIManager.Instance.StartUITask(intent, onViewUpdateComplete: () => {
                var target = UIManager.Instance.FindUITask(typeof(LoadingUITask).Name) as LoadingUITask;
                target.EventOnStop += () => {
                    onEnd?.Invoke();
                };
            });
        }

        #region UITask生命周期

        private void InitDataFromIntent(UIIntent curIntent)
        {
            //m_msg = curIntent.GetCustomClassParam<string>(ParamKey_Msg);
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
            m_uiCtrl = m_viewControllerArray[0] as LoadingUIController;
            if (m_uiCtrl != null)
            {
               
            }
        }

        protected override void OnAllAssetClear()
        {
            base.OnAllAssetClear();
            if (m_uiCtrl != null)
            {
                
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
            m_uiCtrl.Show();
        }

        protected override void OnTick()
        {
            if (m_uiCtrl != null)
            {
                m_uiCtrl.Tick();
            }
        }



        #endregion

        


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
                LayerName = "LoadingUILayer",
                AssetPath = "Assets/Framework/UI/Resources/LoadingUIPrefab.prefab",
            },
        };

        protected override ViewControllerDesc[] ViewControllerDescArray
        {
            get { return m_viewControllerDescs; }
        }

        private ViewControllerDesc[] m_viewControllerDescs = new ViewControllerDesc[]{
            new ViewControllerDesc()
            {
                AtachLayerName = "LoadingUILayer",
                AtachPath = "./",
                TypeFullName = "bluebean.UGFramework.UI.LoadingUIController",
            },
        };
        #endregion

        private LoadingUIController m_uiCtrl = null;

        public LoadingUIController FadeInOutUIController { get { return m_uiCtrl; } }

    }
}
