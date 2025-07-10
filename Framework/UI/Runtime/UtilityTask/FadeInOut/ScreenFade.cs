using System;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework.UI
{
    public class ScreenFade
    {
        public void Setup(Image image)
        {
            m_image = image;
        }

        /// <summary>
        /// 画面从黑屏淡入
        /// </summary>
        /// <param name="time"></param>
        /// <param name="color"></param>
        /// <param name="onEnd"></param>         
        public void FadeIn(float time, Color color, Action onEnd = null)
        {
            if (m_onEnd != null)
                m_onEnd();
            m_curTime = 0;
            m_duration = time;
            m_color = color;
            m_isFadeIn = true;
            m_delayFrame = 1;       // 延迟一帧，用来处理界面切换瞬间可能出现错误画面问题
            m_isEnd = false;
            m_onEnd = onEnd;

            if (time > 0)
            {
                m_image.gameObject.SetActive(true);
                m_image.color = new Color(m_color.r, m_color.g, m_color.b, 1);
            }
            else
            {
                m_image.gameObject.SetActive(false);
                if (onEnd != null)
                {
                    m_onEnd = null;
                    onEnd();
                }
            }
        }

        /// <summary>
        /// 画面淡出到黑屏
        /// </summary>
        /// <param name="time"></param>
        /// <param name="color"></param>         
        /// <param name="onEnd"></param>
        public void FadeOut(float time, Color color, Action onEnd = null)
        {
            if (m_onEnd != null)
                m_onEnd();
            m_curTime = 0;
            m_duration = time;
            m_delayFrame = 0;
            m_color = color;
            m_isFadeIn = false;
            m_isEnd = false;
            m_onEnd = onEnd;

            m_image.gameObject.SetActive(true);

            if (time > 0)
            {
                m_image.color = new Color(m_color.r, m_color.g, m_color.b, 0);
            }
            else
            {
                m_image.color = new Color(m_color.r, m_color.g, m_color.b, 1);
                if (onEnd != null)
                {
                    m_onEnd = null;
                    onEnd();
                }
            }
        }

        /// <summary>
        /// 是否正在淡入或淡出
        /// </summary>
        /// <returns></returns>
        bool IsFading()
        {
            return m_image.gameObject.activeSelf;
        }

        /// <summary>
        /// 更新淡出淡入
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            if (m_isEnd && m_onEnd != null)
            {
                Action onFadeEnd = m_onEnd;
                m_onEnd = null;
                m_isEnd = false;
                onFadeEnd();
            }

            if (m_curTime < m_duration)
            {
                float a = 0;
                if (m_delayFrame > 0)
                {
                    --m_delayFrame;
                }
                else
                {
                    m_curTime += dt;
                    a = Mathf.Clamp01(m_curTime / m_duration);
                }

                if (m_isFadeIn)
                    a = 1 - a;

                m_image.color = new Color(m_color.r, m_color.g, m_color.b, a);
                if (m_curTime >= m_duration)
                {
                    m_curTime = m_duration = 0;
                    if (m_isFadeIn)
                        m_image.gameObject.SetActive(false);

                    if (m_onEnd != null)    // 延后一帧回调，以避免画面没有完全被压暗
                        m_isEnd = true;
                }
            }
        }

        float m_curTime;
        float m_duration;
        int m_delayFrame;
        bool m_isFadeIn;
        bool m_isEnd;
        Color m_color = Color.black;
        Action m_onEnd;

        Image m_image;
    }
}
