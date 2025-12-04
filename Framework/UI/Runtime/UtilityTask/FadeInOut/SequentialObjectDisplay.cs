using System;
using System.Collections;
using UnityEngine;

namespace bluebean.UGFramework.UI
{
    public class SequentialObjectDisplay : MonoBehaviour
    {
        [Header("需要显示的物体")]
        public GameObject[] objectsToDisplay = new GameObject[3];

        [Header("显示时间间隔")]
        public float displayInterval = 1f;
        [Header("开始显示索引")]
        public int startIndex = 0;

        private int currentIndex = 0;
        private Coroutine displayCoroutine;

        void Start()
        {
            // 初始时隐藏所有物体
            HideAllObjects();
            startIndex = Mathf.Clamp(startIndex, 0, objectsToDisplay.Length - 1);
            currentIndex = startIndex;
             // 开始显示循环
             displayCoroutine = StartCoroutine(DisplaySequence());
        }

        IEnumerator DisplaySequence()
        {
            //Debug.Log("DisplaySequence start");
            while (true)
            {
                // 隐藏所有物体
                HideAllObjects();

                // 显示当前物体
                if (objectsToDisplay[currentIndex] != null)
                {
                    objectsToDisplay[currentIndex].SetActive(true);
                }

                // 等待指定时间
                yield return new UnityEngine.WaitForSeconds(displayInterval);

                // 移动到下一个物体
                currentIndex = (currentIndex + 1) % objectsToDisplay.Length;
                //Debug.Log($"{DateTime.Now.ToString("HHmmss:ff")} DisplaySequence currentIndex:{currentIndex}");

            }
        }

        void HideAllObjects()
        {
            foreach (GameObject obj in objectsToDisplay)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // 提供公共方法控制显示循环
        public void StartDisplay()
        {
            if (displayCoroutine == null)
            {
                displayCoroutine = StartCoroutine(DisplaySequence());
            }
        }

        public void StopDisplay()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }
            HideAllObjects();
        }

        void OnDestroy()
        {
            // 确保在对象销毁时停止协程
            StopDisplay();
        }
    }
}