using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework
{
    /// <summary>
    /// 设置颜色信息
    /// </summary>
    [Serializable]
    public class StateColorDesc
    {
        public GameObject m_gameObject;
        public Color m_color;
    }

    /// <summary>
    /// UIState信息
    /// </summary>
    [Serializable]
    public class StateDesc
    {
        /// <summary>
        /// StateName
        /// </summary>
        public string m_stateName;
        /// <summary>
        /// 将指定物体设置active为True
        /// </summary>
        public List<GameObject> m_activeObjects = new List<GameObject>();
        /// <summary>
        /// 设置指定物体的颜色
        /// </summary>
        public List<StateColorDesc> m_colorDescs = new List<StateColorDesc>();
    }

    /// <summary>
    /// UI组件状态切换控制器，一个状态包括：要显示的物体，设置物体的颜色，播发UITweener
    /// </summary>
    public class CommonStateController : MonoBehaviour
    {
        public string CurState
        {
            get { return m_curState; }
        }

        public List<StateDesc> m_stateDescs = new List<StateDesc>();

        private string m_curState;

        #region 收集信息

        private bool m_hasCollect = false;

        /// <summary>
        /// 所有state中设置为active的物体
        /// </summary>
        private readonly List<GameObject> m_allGameObjectActiveOrNot = new List<GameObject>();

        #endregion

        public void CollectResources()
        {
            //收集所有设置为Active的物体
            m_allGameObjectActiveOrNot.Clear();
            foreach (var uiStateDesc in m_stateDescs)
            {
                foreach (var gameObject in uiStateDesc.m_activeObjects)
                {
                    m_allGameObjectActiveOrNot.Add(gameObject);
                }
            }
            m_hasCollect = true;
        }

        private StateDesc GetStateDesc(string stateName)
        {
            foreach (var stateDesc in m_stateDescs)
            {
                if (stateDesc.m_stateName == stateName)
                {
                    return stateDesc;
                }
            }
            return null;
        }

        //todo alpha设置与硬切在起始或结束时刻上的不平滑
        public void SetState(string stateName, System.Action onEnd = null, bool refreshTheSameState = true)
        {
            var stateDesc = GetStateDesc(stateName);
            if (stateDesc == null)
            {
                Debug.LogError(string.Format("the UIStateController in {0} don't has state {1}", name, stateName));
                return;
            }
            if (!m_hasCollect)
            {
                CollectResources();
            }
            //隐藏所有物体
            foreach (var go in m_allGameObjectActiveOrNot)
            {
                go.SetActive(false);
            }
            //显示当前state的物体
            foreach (var go in stateDesc.m_activeObjects)
            {
                go.SetActive(true);
            }
            //设置物体的颜色
            foreach (var colorDesc in stateDesc.m_colorDescs)
            {
                if (colorDesc.m_gameObject != null)
                {
                    var setColorImpl = colorDesc.m_gameObject.GetComponent<ISetColor>();
                    if(setColorImpl != null)
                    {
                        setColorImpl.SetColor(colorDesc.m_color);
                    }
                    else
                    {
                        Debug.LogError(string.Format("the gameobject {0} don't have a setColorImpl comp", colorDesc.m_gameObject.name));
                    }
                }
            }
            //var lastState = m_curState;
            m_curState = stateName;
        }

        public void SwichToNextState()
        {
            if (m_stateDescs.Count == 0)
                return;
            var uiStateDesc = GetStateDesc(m_curState);
            var index = m_stateDescs.FindIndex((item) => item == uiStateDesc);
            index++;
            if (index >= m_stateDescs.Count)
            {
                index = 0;
            }
            uiStateDesc = m_stateDescs[index];
            SetState(uiStateDesc.m_stateName);
        }

    }

}
