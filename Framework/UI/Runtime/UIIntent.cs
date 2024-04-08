using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class UIIntent
    {
        public string Name { get { return m_name; } }
        public bool IsFullScreen { get { return m_isFullScreen; } }
        public bool IsRecoverOnReturn { get { return m_isRecoverOnReturn; } }
        public bool PushIntoStack { get { return m_pushIntoStack; } }

        private string m_name;

        private bool m_isRecoverOnReturn;//当全屏页面返回时是否恢复UITask
        private bool m_isFullScreen;//是否全屏
        private bool m_pushIntoStack = true;

        private readonly Dictionary<string, object> m_customParamDic = new Dictionary<string, object>();

        public UIIntent(string name,bool pushIntoStack = true,bool isFullScreen = true, bool isRecoverOnReturn=true)
        {
            m_name = name;
            m_pushIntoStack = pushIntoStack;
            m_isRecoverOnReturn = isRecoverOnReturn;
            m_isFullScreen = isFullScreen;
        }

        public void SetCustomParam(string key, object value)
        {
            if (m_customParamDic.ContainsKey(key))
            {
                m_customParamDic[key] = value;
            }
            else
            {
                m_customParamDic.Add(key, value);
            }
        }

        public T GetCustomClassParam<T>(string key) where T:class
        {
            if (m_customParamDic.ContainsKey(key))
            {
                return m_customParamDic[key] as T;
            }
            return null;
        }

        public T GetCustomStructParam<T>(string key) where T : struct
        {
            if (m_customParamDic.ContainsKey(key))
            {
                return (T)m_customParamDic[key];
            }
            return default(T);
        }

        public void ClearCustomParam(string key)
        {
            if (m_customParamDic.ContainsKey(key))
            {
                m_customParamDic.Remove(key);
            }
        }

        public void ClearAllCustomParam()
        {
            m_customParamDic.Clear();
        }
    }
}
