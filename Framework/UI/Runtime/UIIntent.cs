using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class UIIntent : SceneIntent
    {
        public bool IsFullScreen { get { return m_isFullScreen; } }
        public bool IsRecoverOnReturn { get { return m_isRecoverOnReturn; } }
        public bool PushIntoStack { get { return m_pushIntoStack; } }


        private bool m_isRecoverOnReturn;//当全屏页面返回时是否恢复UITask
        private bool m_isFullScreen;//是否全屏
        private bool m_pushIntoStack = true;


        public UIIntent(string name,bool pushIntoStack = true,bool isFullScreen = true, bool isRecoverOnReturn=true):base(name)
        {
            m_pushIntoStack = pushIntoStack;
            m_isRecoverOnReturn = isRecoverOnReturn;
            m_isFullScreen = isFullScreen;
        }

    }
}
