using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bluebean.UGFramework.UI
{

    public class LoadingUIController : UIViewController
    {
        CoroutineScheduler m_scheduler = new CoroutineScheduler();

        protected override void OnBindFieldsComplete()
        {
            base.OnBindFieldsComplete();
            this.gameObject.SetActive(false);
        }

        public void Show()
        {
            m_scheduler.StartCorcoutine(Co_LoadingProcess());
        }

        private IEnumerator Co_LoadingProcess()
        {
            List<GameObject> nodes = new List<GameObject>();
            for(int i = 0; i < this.gameObject.transform.childCount; i++)
            {
                if (this.gameObject.transform.GetChild(i).gameObject.activeSelf)
                {
                    nodes.Add(this.gameObject.transform.GetChild(i).gameObject);
                }
            }
            foreach (var node in nodes)
            {
                node.SetActive(false);
            }
            FadeInOutUITask.StartShowFadeOutLoadingFadeIn(null);
            yield return new WaitForSeconds(0.5f);
            this.gameObject.SetActive(true);
            foreach (var node in nodes)
            {
                node.gameObject.SetActive(true);
                yield return null;
                FadeInOutUITask.HideFadeOutLoadingFadeIn();
                yield return new WaitForSeconds(2);
                FadeInOutUITask.StartShowFadeOutLoadingFadeIn(() => {
                    node.gameObject.SetActive(false);
                });
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
            FadeInOutUITask.HideFadeOutLoadingFadeIn();
            this.gameObject.SetActive(false);
            this.m_ownerTask.Stop();
        }

        public void Tick()
        {
            float dt = Time.deltaTime;
            m_scheduler.Tick();
        }
    }
}
