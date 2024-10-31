using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class UITaskBase : SceneTaskBase
    {
        public UITaskBase(string name) : base(name)
        { }

        #region SceneTaskBase重载方法
        protected sealed override bool OnStart(SceneIntent intent)
        {
            var ok = OnStart(intent as UIIntent);
            return ok;
        }

        protected sealed override bool OnResume(SceneIntent intent)
        {
            var ok = OnResume(intent as UIIntent);
            return ok;
        }

        protected sealed override void StartUpdatePipeline(SceneIntent intent)
        {
            base.StartUpdatePipeline(intent);
        }

        #endregion

        #region 更新流程
        protected virtual bool OnStart(UIIntent uiIntent)
        {
            StartUIUpdatePipeline(uiIntent);
            return true;
        }

        protected virtual bool OnResume(UIIntent uiIntent)
        {
            StartUIUpdatePipeline(uiIntent);
            return true;
        }

        public void OnNewIntent(UIIntent intent)
        {
            StartUIUpdatePipeline(intent);
        }

        protected virtual void OnIntentChange(UIIntent prevIntent, UIIntent curIntent)
        {

        }

        protected virtual void StartUIUpdatePipeline(UIIntent uiIntent)
        {
            if (uiIntent != null)
            {
                OnIntentChange(m_curUIIntent, uiIntent);
                m_curUIIntent = uiIntent;
            }

            StartUpdatePipeline(uiIntent);
        }

        #endregion

        public void CloseAndReturn(Action<bool> onFinish = null, bool stay4Reuse = false)
        {
            UIManager.Instance.CloseAndReturn(CurUIIntent, onFinish, stay4Reuse);
        }

        /// <summary>
        /// 存放内容数据
        /// </summary>
        protected UIIntent m_curUIIntent;

        public UIIntent CurUIIntent { get { return m_curUIIntent; } }

    }
}
