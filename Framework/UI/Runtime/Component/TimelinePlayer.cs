using UnityEngine;
using UnityEngine.Playables;

namespace bluebean.UGFramework.UI
{
    /// <summary>
    /// Timeline 播放器封装，统一管理 PlayableDirector 的播放和 updateMode 设置
    /// </summary>
    public class TimelinePlayer
    {
        private PlayableDirector m_director;
        private DirectorUpdateMode m_updateMode;

        public TimelinePlayer(PlayableDirector director)
        {
            m_director = director;
            m_updateMode = director != null ? director.timeUpdateMode : DirectorUpdateMode.GameTime;
        }

        /// <summary>
        /// 设置 updateMode
        /// </summary>
        public void SetUpdateMode(DirectorUpdateMode mode)
        {
            m_updateMode = mode;
            if (m_director != null)
                m_director.timeUpdateMode = mode;
        }

        /// <summary>
        /// 播放 Timeline，自动处理不同 updateMode 的初始化
        /// </summary>
        public void Play()
        {
            if (m_director == null)
                return;
            // 针对不同 updateMode 做初始化
            switch (m_updateMode)
            {
                case DirectorUpdateMode.GameTime:
                case DirectorUpdateMode.UnscaledGameTime:
                    m_director.time = 0;
                    m_director.Play();
                    break;
                case DirectorUpdateMode.Manual:
                    m_director.time = 0;
                    m_director.Evaluate(); // 先评估第一帧
                    m_director.Play();
                    break;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (m_director != null)
                m_director.Stop();
        }

        /// <summary>
        /// 获取当前 PlayableDirector
        /// </summary>
        public PlayableDirector GetDirector()
        {
            return m_director;
        }
    }
} 