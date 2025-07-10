using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework.UI
{
    public enum FadeStyle
    {
        None,
        Black,
        White
    }

    public class FadeInOutUIController : UIViewController
    {
        ScreenFade m_screenFade;
        bool m_hideFadeOutLoadingFadeIn = true;
        float m_defaultFadeOutTime = 0.3f;
        float m_defaultFadeInTime = 0.3f;
        CoroutineScheduler m_scheduler = new CoroutineScheduler();

        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
            //this.gameObject.SetActive(false);

            m_fadeImage.gameObject.SetActive(false);
            m_screenFade = new ScreenFade();
            m_screenFade.Setup(m_fadeImage);
        }

        public void StartFadeOut(Action fadeoutEnd, FadeStyle style = FadeStyle.Black, float fadeOutTime = -1)
        {
            if (fadeOutTime < 0)
                fadeOutTime = m_defaultFadeOutTime;

            Color fadeColor = Color.black;
            if (style == FadeStyle.White)
                fadeColor = Color.white;

            FadeOut(fadeOutTime, fadeColor, fadeoutEnd);
        }

        /// <summary>
        /// ��ʼ�������������
        /// </summary>
        /// <param name="fadeOutEnd">���������ص�</param>
        /// <param name="fadeOutTime"></param>
        /// <param name="fadeInTime"></param>
        /// <param name="style">1����ɫ�� 2����ɫ</param>
        public void StartShowFadeOutLoadingFadeIn(Action fadeOutEnd, FadeStyle style = FadeStyle.Black, float fadeOutTime = -1, float fadeInTime = -1)
        {
            if (IsFading())
            {
                // �Ѿ�����ȫ���ˣ�ֻ��ҪFadeIn
                fadeOutTime = 0;
            }
            m_scheduler.StartCorcoutine(ShowFadeOutLoadingFadeIn(fadeOutEnd, style, fadeOutTime, fadeInTime));
        }

        IEnumerator ShowFadeOutLoadingFadeIn(Action fadeOutEnd, FadeStyle style, float fadeOutTime, float fadeInTime)
        {
            m_hideFadeOutLoadingFadeIn = false;

            if (fadeOutTime < 0)
                fadeOutTime = m_defaultFadeOutTime;
            if (fadeInTime < 0)
                fadeInTime = m_defaultFadeInTime;

            Color fadeColor = Color.black;
            if (style == FadeStyle.White)
                fadeColor = Color.white;

            bool isEnd = false;
            FadeOut(fadeOutTime, fadeColor, () => { isEnd = true; });
            yield return new WaitUntil(() => isEnd);
            if (fadeOutEnd != null)
            {
                fadeOutEnd();
                fadeOutEnd = null;
            }

            if (!m_hideFadeOutLoadingFadeIn)
            {
                ShowLoading(style);
                yield return new WaitUntil(() => m_hideFadeOutLoadingFadeIn);
            }

            HideLoading();

            isEnd = false;
            FadeIn(fadeInTime, fadeColor, () => { isEnd = true; });
            yield return new WaitUntil(() => isEnd);
        }

        public void HideFadeOutLoadingFadeIn()
        {
            m_hideFadeOutLoadingFadeIn = true;
        }

        /// <summary>
        /// ����Ӻ�������
        /// </summary>
        /// <param name="time"></param>
        /// <param name="color"></param>
        /// <param name="onEnd"></param>         
        public void FadeIn(float time, Color color, Action onEnd = null)
        {
            m_screenFade.FadeIn(time, color, onEnd);
        }

        /// <summary>
        /// ���浭��������
        /// </summary>
        /// <param name="time"></param>
        /// <param name="color"></param>         
        /// <param name="onEnd"></param>
        public void FadeOut(float time, Color color, Action onEnd = null)
        {
            m_screenFade.FadeOut(time, color, onEnd);
        }

        /// <summary>
        /// �Ƿ����ڵ���򵭳�
        /// </summary>
        /// <returns></returns>
        public bool IsFading()
        {
            return m_fadeImage.gameObject.activeSelf;
        }

        /// <summary>
        /// ��ʾ��ȡ�ж���
        /// </summary>
        /// <param name="show"></param>
        /// <param name="style">1����ɫ�� 2����ɫ</param>        
        public void ShowLoading(FadeStyle style)
        {
            m_loadingGameObject.SetActive(true);
            for (int i = 0; i < m_loadingGameObject.transform.childCount; ++i)
            {
                m_loadingGameObject.transform.GetChild(i).gameObject.SetActive((i + 1) == (int)style);
            }
        }

        /// <summary>
        /// ���ض�ȡ�ж���
        /// </summary>
        public void HideLoading()
        {
            m_loadingGameObject.SetActive(false);
        }


        void Update()
        {
            float dt = Time.deltaTime;
            m_scheduler.Tick();
            if (m_screenFade != null)
                m_screenFade.Update(dt);
        }

        [AutoBind("./Loading")]
        GameObject m_loadingGameObject = null;
        [AutoBind("./Fade")]
        Image m_fadeImage = null;
    }
}
