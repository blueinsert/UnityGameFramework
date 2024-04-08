using System;
using System.Collections;
using System.Collections.Generic;

namespace bluebean.UGFramework.UI
{
    public class UITaskRegisterItem
    {
        public string Name { get; set; }
        public string TypeFullName { get; set; }
    }

    public class UIManager
    {

        #region 单例模式
        private UIManager() { }
        public static UIManager m_instance;
        public static UIManager Instance { get { return m_instance; } }
        public static UIManager CreateInstance()
        {
            m_instance = new UIManager();
            return m_instance;
        }
        #endregion

        #region 内部变量
        /// <summary>
        /// 栈结构
        /// </summary>
        private List<UIIntent> m_uiIntentStack = new List<UIIntent>();

        /// <summary>
        /// 字典结构
        /// </summary>
        private Dictionary<string, UITaskBase> m_uiTaskDic = new Dictionary<string, UITaskBase>();

        /// <summary>
        /// 注册字典
        /// </summary>
        private Dictionary<string, UITaskRegisterItem> m_uiTaskRegistyerItemDic = new Dictionary<string, UITaskRegisterItem>();
        #endregion

        #region 公共方法

        /// <summary>
        /// 注册UITask条目
        /// </summary>
        /// <param name="uiTaskRegisterItem"></param>
        public void RegisterUITask(UITaskRegisterItem uiTaskRegisterItem)
        {
            if (string.IsNullOrEmpty(uiTaskRegisterItem.Name))
            {
                Debug.LogError("uiTaskRegisterItem's name is null or empty");
                return;
            }
            if (m_uiTaskRegistyerItemDic.ContainsKey(uiTaskRegisterItem.Name))
            {
                Debug.LogError("RegisterUITask, Already contain the same name UITask,name:" + uiTaskRegisterItem.Name);
                return;
            }
            m_uiTaskRegistyerItemDic.Add(uiTaskRegisterItem.Name, uiTaskRegisterItem);
        }

        /// <summary>
        /// 开启一个UITask
        /// </summary>
        /// <param name="intent"></param>
        /// <param name="onPrepareEnd"></param>
        /// <param name="onViewUpdateComplete"></param>
        /// <param name="redirectOnLoadAllAssetsComplete"></param>
        /// <returns></returns>
        public bool StartUITask(UIIntent intent, Action<bool> onPrepareEnd = null, Action onViewUpdateComplete = null, Action redirectOnLoadAllAssetsComplete = null)
        {
            if (!m_uiTaskRegistyerItemDic.ContainsKey(intent.Name))
            {
                Debug.LogError("StartUITask can't find UITaskRegisterItem name:" + intent.Name);
                return false;
            }
            var uiTaskRegisterItem = m_uiTaskRegistyerItemDic[intent.Name];
            var uiTask = CreateOrGetUITaskInstance(uiTaskRegisterItem);
            if (uiTask == null)
            {
                Debug.LogError("StartUITask Can't create UITask instance, typeFullName:" + uiTaskRegisterItem.TypeFullName);
            }
            if (redirectOnLoadAllAssetsComplete != null)
            {
                uiTask.UpdateCtx.SetRedirectOnLoadAssetCompleteCb(redirectOnLoadAllAssetsComplete);
            }
            if (onViewUpdateComplete != null)
            {
                uiTask.UpdateCtx.SetViewUpdateCompleteCb(onViewUpdateComplete);
            }
            StartUITaskInternal(uiTask, intent, onPrepareEnd);
            return true;
        }

        public T FindUITask<T>() where T : UITaskBase
        {
            if (m_uiTaskDic.ContainsKey(typeof(T).Name))
            {
                var uiTask = m_uiTaskDic[typeof(T).Name];
                return uiTask as T;
            }
            return null;
        }

        public UITaskBase FindUITask(string name)
        {
            if (m_uiTaskDic.ContainsKey(name))
            {
                var uiTask = m_uiTaskDic[name];
                return uiTask;
            }
            return null;
        }

        public void StopAllUITask()
        {
            Debug.Log("UIManager:StopAllUITask");
            List<UITaskBase> allUITask = new List<UITaskBase>();
            foreach (var pair in m_uiTaskDic)
            {
                allUITask.Add(pair.Value);
            }
            foreach (var task in allUITask)
            {
                task.Stop();
            }
        }

        public void TryCloseUITask(string name, bool destroy = false)
        {
            var uiTask = FindUITask(name);
            if (uiTask != null && uiTask.State == TaskState.Runing)
            {
                if (destroy)
                {
                    uiTask.Stop();
                }
                else
                {
                    //stop or pause,根据负载情况todo
                    //uiTask.Stop();
                    uiTask.Pause();
                }
            }
        }

        public void CloseAndReturn2(string targetTaskName, UIIntent intent, Action<bool> onFinish = null, bool destroy = false)
        {
            var top = m_uiIntentStack[m_uiIntentStack.Count - 1];
            if (intent != top)
            {
                Debug.LogError(string.Format("can't close not top uitask:{0} curTop:{1}", intent.Name, top.Name));
                if (onFinish != null) onFinish(false);
                return;
            }
            TryCloseUITask(intent.Name, destroy);
            m_uiIntentStack.RemoveAt(m_uiIntentStack.Count - 1);
            var targetIndex = m_uiIntentStack.FindIndex((elem) => { return elem.Name == targetTaskName; });
            if (targetIndex == -1)
            {
                Debug.LogError(string.Format("can't find target task name:", targetTaskName));
                if (onFinish != null) onFinish(false);
                return;
            }
            var targetIntent = m_uiIntentStack[targetIndex];
            m_uiIntentStack.RemoveRange(targetIndex, m_uiIntentStack.Count - targetIndex);
            StartUITask(targetIntent,onViewUpdateComplete:()=> {
                if (onFinish != null) onFinish(true);
            });
        }

        public void CloseAndReturn(UIIntent intent, Action<bool> onFinish = null, bool destroy = false)
        {
            var top = m_uiIntentStack[m_uiIntentStack.Count - 1];
            if (intent != top)
            {
                Debug.LogError(string.Format("can't close not top uitask:{0} curTop:{1}", intent.Name, top.Name));
                if (onFinish != null) onFinish(false);
                return;
            }
            TryCloseUITask(intent.Name, destroy);
            m_uiIntentStack.RemoveAt(m_uiIntentStack.Count - 1);
            if (!intent.IsFullScreen)
            {

                if (onFinish != null) onFinish(true);
            }
            else
            {
                List<UIIntent> toRecoverIntentList = new List<UIIntent>();
                for (int i = m_uiIntentStack.Count - 1; i >= 0; i--)
                {
                    var uiIntent = m_uiIntentStack[i];

                    if (uiIntent.IsFullScreen && uiIntent.IsRecoverOnReturn)
                    {
                        toRecoverIntentList.Add(uiIntent);
                        break;
                    }
                    else
                    {
                        toRecoverIntentList.Add(uiIntent);
                    }
                }
                m_uiIntentStack.RemoveRange(m_uiIntentStack.Count - toRecoverIntentList.Count, toRecoverIntentList.Count);
                toRecoverIntentList.Reverse();
                RecoverUITasks(toRecoverIntentList, onFinish);
            }
        }

        private void RecoverUITasks(List<UIIntent> intents, Action<bool> onFinish)
        {
            for (int i = 0; i < intents.Count; i++)
            {
                var uiIntent = intents[i];
                if (uiIntent.IsRecoverOnReturn)
                {
                    StartUITask(uiIntent);
                }
            }
            if (onFinish != null) onFinish(true);
        }
        #endregion

        #region 内部方法

        /// <summary>
        /// 当UITask销毁
        /// </summary>
        /// <param name="uiTask"></param>
        private void OnUITaskStop(UITaskBase uiTask)
        {
            var intent = uiTask.CurUIIntent;
            //从字典中移除
            m_uiTaskDic.Remove(uiTask.Name);
        }

        private void OnUITaskStart(UITaskBase uiTask)
        {
            m_uiTaskDic[uiTask.Name] = uiTask;
        }

        /// <summary>
        /// 创建一个新的或者复用存在的UITask
        /// </summary>
        /// <param name="uiTaskRegisterItem"></param>
        /// <returns></returns>
        private UITaskBase CreateOrGetUITaskInstance(UITaskRegisterItem uiTaskRegisterItem)
        {
            UITaskBase instance = null;
            //复用之前的UITask
            UITaskBase uiTask = null;
            m_uiTaskDic.TryGetValue(uiTaskRegisterItem.Name, out uiTask);
            if (uiTask != null)
            {
                instance = uiTask;
            }
            else
            {
                //如果不存在，创建一个新的
                instance = ClassLoader.CreateInstance(uiTaskRegisterItem.TypeFullName, uiTaskRegisterItem.Name) as UITaskBase;
            }
            return instance;
        }

        /// <summary>
        /// 开启UITask
        /// </summary>
        /// <param name="uiTask"></param>
        /// <param name="intent"></param>
        /// <param name="onPrepareEnd"></param>
        private void StartUITaskInternal(UITaskBase uiTask, UIIntent intent, Action<bool> onPrepareEnd = null)
        {
            uiTask.PrapareDataForStart((res) =>
            {
                if (onPrepareEnd != null)
                {
                    onPrepareEnd(res);
                }
                if (res)
                {
                    if (intent.IsFullScreen)
                    {
                        for (int i = m_uiIntentStack.Count - 1; i >= 0; i--)
                        {
                            var uiIntent = m_uiIntentStack[i];
                            TryCloseUITask(uiIntent.Name);
                        }
                    }
                    //加入栈
                    if (intent.PushIntoStack)
                        m_uiIntentStack.Add(intent);
                    StartOrResumeUITask(uiTask, intent);
                }
                else
                {
                    Debug.LogError(string.Format("PrapareDataForStart Failed UITask Name:", uiTask.Name));
                }
            });
        }

        private void StartOrResumeUITask(UITaskBase uiTask, UIIntent intent)
        {
            switch (uiTask.State)
            {
                case TaskState.Init:
                    uiTask.UpdateCtx.m_isInit = true;
                    uiTask.EventOnStop += () => { OnUITaskStop(uiTask); };
                    uiTask.EventOnStart += () => { OnUITaskStart(uiTask); };
                    uiTask.Start(intent);
                    break;
                case TaskState.Paused:
                    uiTask.UpdateCtx.m_isResume = true;
                    uiTask.Resume(intent);
                    Debug.Log(string.Format("UIManager:OnResume:{0}", uiTask.Name));
                    break;
                case TaskState.Runing:
                    uiTask.OnNewIntent(intent);
                    Debug.Log(string.Format("UIManager:OnNewIntent:{0}", uiTask.Name));
                    break;
            }
        }

        #endregion

    }
}
