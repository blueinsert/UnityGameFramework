using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Timeline;

namespace bluebean.UGFramework.UI
{
    /// <summary>
    /// 设置颜色信息
    /// </summary>
    [Serializable]
    public class UIColorDesc
    {
        public GameObject m_gameObject;
        public Color m_color;
    }

    /// <summary>
    /// Timeline和状态名的组合
    /// </summary>
    [Serializable]
    public class TimelineStateDesc
    {
        /// <summary>
        /// Timeline组件
        /// </summary>
        public UnityEngine.Playables.PlayableDirector m_timelineDirector;
        /// <summary>
        /// 要播放的状态名（轨道组名）
        /// </summary>
        public string m_stateName;
    }

    /// <summary>
    /// UIState信息
    /// </summary>
    [Serializable]
    public class UIStateDesc
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
        public List<UIColorDesc> m_uiColorDescs = new List<UIColorDesc>();
        /// <summary>
        /// 播放指定的Tweener
        /// </summary>
        public List<TweenMain> m_tweeners = new List<TweenMain>();
        /// <summary>
        /// 播放指定的Timeline和状态组合
        /// </summary>
        public List<TimelineStateDesc> m_timelineStates = new List<TimelineStateDesc>();
    }

    /// <summary>
    /// UI组件状态切换控制器，一个状态包括：要显示的物体，设置物体的颜色，播发UITweener
    /// </summary>
    public class UIStateController : MonoBehaviour
    {
        public string CurState
        {
            get { return m_curState; }
        }

        public List<UIStateDesc> m_uiStateDescs = new List<UIStateDesc>();

        private string m_curState;

        #region 收集信息

        private bool m_hasCollect = false;

        /// <summary>
        /// 所有state中设置为active的物体
        /// </summary>
        private readonly List<GameObject> m_allGameObjectActiveOrNot = new List<GameObject>();
        /// <summary>
        /// 所有tweener
        /// </summary>
        private readonly List<TweenMain> m_allTweeners = new List<TweenMain>();
        /// <summary>
        /// 所有timeline director
        /// </summary>
        private readonly List<UnityEngine.Playables.PlayableDirector> m_allTimelineDirectors = new List<UnityEngine.Playables.PlayableDirector>();

        #endregion

        // 只启用指定GroupTrack及其子轨道，其它全部禁用
        private void EnableOnlyGroupTrack(UnityEngine.Playables.PlayableDirector director, string groupName)
        {
            if (director == null || director.playableAsset == null)
                return;
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
                return;
            // 禁用所有轨道
            foreach (var rootTrack in timelineAsset.GetRootTracks())
                SetTrackMuteRecursive(rootTrack, true);
            // 查找目标GroupTrack并启用
            var targetGroup = FindGroupTrackByName(timelineAsset, groupName);
            if (targetGroup != null)
            {
                SetTrackMuteRecursive(targetGroup, false);
                // 重建Timeline图，确保生效
                double t = director.time;
                director.RebuildGraph();
                director.time = t;
                director.Play();
            }
            else
            {
                Debug.LogWarning($"未找到名为 {groupName} 的GroupTrack");
            }
        }
        // 递归设置轨道及其子轨道的mute
        private void SetTrackMuteRecursive(TrackAsset track, bool mute)
        {
            track.muted = mute;
            foreach (var child in track.GetChildTracks())
                SetTrackMuteRecursive(child, mute);
        }
        // 在TimelineAsset中查找指定名称的GroupTrack
        private GroupTrack FindGroupTrackByName(TimelineAsset asset, string groupName)
        {
            foreach (var root in asset.GetRootTracks())
            {
                var found = FindGroupTrackRecursive(root, groupName);
                if (found != null) return found;
            }
            return null;
        }
        private GroupTrack FindGroupTrackRecursive(TrackAsset track, string groupName)
        {
            if (track is GroupTrack group && group.name == groupName)
                return group;
            foreach (var child in track.GetChildTracks())
            {
                var found = FindGroupTrackRecursive(child, groupName);
                if (found != null) return found;
            }
            return null;
        }

        public void CollectResources()
        {
            //收集所有设置为Active的物体
            m_allGameObjectActiveOrNot.Clear();
            foreach (var uiStateDesc in m_uiStateDescs)
            {
                foreach (var gameObject in uiStateDesc.m_activeObjects)
                {
                    m_allGameObjectActiveOrNot.Add(gameObject);
                }
            }
            //收集所有tweener
            m_allTweeners.Clear();
            foreach (var uiStateDesc in m_uiStateDescs)
            {
                foreach (var tweener in uiStateDesc.m_tweeners)
                {
                    if(tweener != null)
                        m_allTweeners.Add(tweener);
                }
            }
            //收集所有timeline director
            m_allTimelineDirectors.Clear();
            foreach (var uiStateDesc in m_uiStateDescs)
            {
                foreach (var timelineState in uiStateDesc.m_timelineStates)
                {
                    if(timelineState != null && timelineState.m_timelineDirector != null)
                    {
                        m_allTimelineDirectors.Add(timelineState.m_timelineDirector);
                    }
                }
            }
            m_hasCollect = true;
        }

        private UIStateDesc GetUIStateDesc(string stateName)
        {
            foreach (var uiStateDesc in m_uiStateDescs)
            {
                if (uiStateDesc.m_stateName == stateName)
                {
                    return uiStateDesc;
                }
            }
            return null;
        }

        //todo alpha设置与硬切在起始或结束时刻上的不平滑
        public void SetUIState(string stateName, System.Action onEnd = null, bool refreshTheSameState = true)
        {
            var uiStateDesc = GetUIStateDesc(stateName);
            if (uiStateDesc == null)
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
            //enable所有tweener
            foreach (var tweener in m_allTweeners)
            {
                if (tweener != null)
                {
                    tweener.enabled = false;
                } 
            }
            //停止所有timeline
            foreach (var director in m_allTimelineDirectors)
            {
                if (director != null)
                {
                    director.Stop();
                }
            }
            //显示当前state的物体
            foreach (var go in uiStateDesc.m_activeObjects)
            {
                go.SetActive(true);
            }
            //设置物体的颜色
            foreach (var uiColorDesc in uiStateDesc.m_uiColorDescs)
            {
                if (uiColorDesc.m_gameObject != null)
                {
                    Image image = uiColorDesc.m_gameObject.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = uiColorDesc.m_color;
                    }
                    Text text = uiColorDesc.m_gameObject.GetComponent<Text>();
                    if (text != null)
                    {
                        text.color = uiColorDesc.m_color;
                    }
                }
            }
            bool hasTweeners = uiStateDesc.m_tweeners != null && uiStateDesc.m_tweeners.Count != 0;
            var lastState = m_curState;
            m_curState = stateName;
           
            //播放当前state的timeline和tweener，联合完成回调
            int timelineCount = (uiStateDesc.m_timelineStates != null) ? uiStateDesc.m_timelineStates.Count : 0;
            int timelineFinished = 0;
            int tweenerCount = (uiStateDesc.m_tweeners != null) ? uiStateDesc.m_tweeners.Count : 0;
            int tweenerFinished = 0;
            System.Action tryInvokeEnd = () => {
                if ((timelineCount == 0 || timelineFinished >= timelineCount) && (tweenerCount == 0 || tweenerFinished >= tweenerCount))
                {
                    if (onEnd != null)
                        onEnd();
                }
            };
            // timeline
            if(timelineCount > 0)
            {
                foreach(var timelineState in uiStateDesc.m_timelineStates)
                {
                    if(timelineState != null && timelineState.m_timelineDirector != null)
                    {
                        var director = timelineState.m_timelineDirector;
                        // 只启用与状态名同名的GroupTrack
                        EnableOnlyGroupTrack(director, timelineState.m_stateName);
                        director.time = 0;
                        director.stopped -= OnTimelineStopped;
                        director.stopped += OnTimelineStopped;
                        director.Play();
                    }
                }
            }
            // tweener
            if(tweenerCount > 0)
            {
                foreach (var tweener in uiStateDesc.m_tweeners)
                {
                    if (tweener != null)
                    {
                        if(tweener.delay <= 0)
                        {
                            tweener.ResetToBeginning();
                        }
                        tweener.OnFinished.RemoveAllListeners();
                        tweener.OnFinished.AddListener(() => {
                            tweenerFinished++;
                            tryInvokeEnd();
                        });
                        tweener.enabled = true;
                        tweener.PlayForward();
                    }
                }
            }
            // 如果都没有，直接回调
            if (timelineCount == 0 && tweenerCount == 0 && onEnd != null)
            {
                onEnd();
            }
            void OnTimelineStopped(UnityEngine.Playables.PlayableDirector d)
            {
                timelineFinished++;
                tryInvokeEnd();
            }
        }

        public void SwichToNextState()
        {
            if (m_uiStateDescs.Count == 0)
                return;
            var uiStateDesc = GetUIStateDesc(m_curState);
            var index = m_uiStateDescs.FindIndex((item) => item == uiStateDesc);
            index++;
            if (index >= m_uiStateDescs.Count)
            {
                index = 0;
            }
            uiStateDesc = m_uiStateDescs[index];
            SetUIState(uiStateDesc.m_stateName);
        }

    }

}
