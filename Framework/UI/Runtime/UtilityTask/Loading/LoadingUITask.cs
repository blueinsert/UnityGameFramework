using bluebean.ProjectE.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{

    public class LoadingUITask : UITaskBase
    {
        public LoadingUITask(string name) : base(typeof(LoadingUITask).Name)
        {
        }

        //public static void StartUITask()
        //{
        //    UIIntent intent = new UIIntent(typeof(FadeInOutUITask).Name, false, false, false);
        //    UIManager.Instance.StartUITask(intent);
        //}

        private static void PostTaskAction(Action<FadeInOutUITask> action)
        {
            var target = UIManager.Instance.FindUITask(typeof(FadeInOutUITask).Name) as FadeInOutUITask;
            if (target != null)
            {
                action(target);
            }
            else
            {
                UIIntent intent = new UIIntent(typeof(FadeInOutUITask).Name, false, false, false);
                UIManager.Instance.StartUITask(intent, onViewUpdateComplete: () => {
                    target = UIManager.Instance.FindUITask(typeof(FadeInOutUITask).Name) as FadeInOutUITask;
                    action(target);
                });
            }
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
            m_uiCtrl = m_viewControllerArray[0] as FadeInOutUIController;
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
        }

        protected override void OnTick()
        {

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
                LayerName = "FadeInOutUILayer",
                AssetPath = "Assets/Framework/UI/Resources/FadeInOutUIPrefab.prefab",
            },
        };

        protected override ViewControllerDesc[] ViewControllerDescArray
        {
            get { return m_viewControllerDescs; }
        }

        private ViewControllerDesc[] m_viewControllerDescs = new ViewControllerDesc[]{
            new ViewControllerDesc()
            {
                AtachLayerName = "FadeInOutUILayer",
                AtachPath = "./",
                TypeFullName = "bluebean.UGFramework.UI.FadeInOutUIController",
            },
        };
        #endregion

        private FadeInOutUIController m_uiCtrl = null;

        public FadeInOutUIController FadeInOutUIController { get { return m_uiCtrl; } }
        //private string m_msg;

        //public const string ParamKey_Msg = "ParamKey_Msg";
    }
}
