using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.UGFramework.Timeline
{
    /// <summary>
    /// Callback based process
    /// </summary>
    public class CbbProcess
    {
        /// <summary>
        /// 构造一个Process
        /// </summary>
        /// <param name="execMode">执行方式（串行或并行）</param>
        protected CbbProcess(ProcessExecMode execMode, ProcessExecutor processExecutor = null)
        {
            m_processExecMode = execMode;
            m_processExecutor = processExecutor;
        }

        #region 公共方法

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(CbbProcess child)
        {
            if (child == null)
            {
                return;
            }

            if (m_state != ProcessState.Init)
            {
                return;
            }

            if (m_childList == null)
            {
                m_childList = new List<CbbProcess>();
            }

            // 添加child到列表
            m_childList.Add(child);
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start(OnEnd onEnd = null)
        {
            if (m_state != ProcessState.Init)
            {
                return;
            }

            m_state = ProcessState.Started;

            // 合并当前传入的onEnd和构造函数中传入的onEnd
            m_onEnd = onEnd;

            OnStart();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop(StopOption opt = StopOption.Complete, bool isRecursive = false)
        {
            if (m_state != ProcessState.Started && m_state != ProcessState.WaitChildCompleted)
            {
                return;
            }

            if (isRecursive)
            {
                // 停止所有子过程
                if (m_childList != null)
                {
                    foreach (var child in m_childList)
                    {
                        child.Stop(opt);
                    }
                }
            }

            OnStop(opt);
        }

        /// <summary>
        /// 设置process名字
        /// </summary>
        /// <param name="name"></param>
        public void SetProcessName(string name)
        {
            m_processName = name;
        }
        #endregion

        #region Overrides of Object

        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(m_processName))
            {
                return m_processName;
            }
#endif
            return GetHashCode().ToString();
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 当开始留给子类实现
        /// </summary>
        protected virtual void OnStart()
        {

#if UNITY_EDITOR
            Debug.Log("Process::OnStart " + ToString());
#endif
            if (m_childList != null && m_processExecMode == ProcessExecMode.Parallel)
            {
                foreach (var child in m_childList)
                {
                    // 如果是并行执行，需要启动所有子过程
                    child.Start(OnChildProcessEnd);
                }
            }

            // 开始执行executor
            if (m_processExecutor != null)
            {
                m_processExecutor((res) => { OnStop(!res ? StopOption.Cancel : StopOption.Complete); });
            }
            else
            {
                // 没有传入执行体，默认立即直接完成Process
                OnStop(StopOption.Complete);
            }
        }

        /// <summary>
        /// 当停止，留给子类实现
        /// </summary>
        /// <param name="opt"></param>
        protected virtual void OnStop(StopOption opt)
        {
#if UNITY_EDITOR
            Debug.Log($"Process::OnStop {ToString()} StopOption:{opt} ");
#endif
            if (opt == StopOption.Complete)
            {
                OnComplete();
            }
            else
            {
                OnCancel();
            }
        }

        /// <summary>
        /// 当自身的process完成
        /// </summary>
        protected virtual void OnComplete()
        {
            switch (m_processExecMode)
            {
                case ProcessExecMode.Serial:
                    OnCompleteForSerialModeImpl();
                    break;
                case ProcessExecMode.Parallel:
                    OnCompleteForParallelModeImpl();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 当自身的process完成：串行模式
        /// </summary>
        protected void OnCompleteForSerialModeImpl()
        {
            // 串行模式下，自己是队列中第一个执行的process，自己执行完毕，需要启动childProcess
            if (m_childList != null && m_childList.Count != 0)
            {
                // 等待所有childProcess完成
                m_state = ProcessState.WaitChildCompleted;

                // 启动第一个childProcess
                var firstChild = m_childList[0];
                firstChild.Start(OnChildProcessEnd);
            }
            else
            {
                // 没有childProcess，直接设置完成状态
                m_state = ProcessState.Completed;

                OnProcessEnd(true);
            }
        }

        /// <summary>
        /// 当自身的process完成：并行模式
        /// </summary>
        protected void OnCompleteForParallelModeImpl()
        {
            // 检查是否子过程都完成了
            bool hasChildNotComplete = false;

            // 并行执行的process，需要对每一个process进行检查，所有都完成才行
            if (m_childList != null && m_childList.Count != 0)
            {
                foreach (var child in m_childList)
                {
                    if (child.State != ProcessState.Completed)
                    {
                        hasChildNotComplete = true;
                        break;
                    }
                }
            }

            if (hasChildNotComplete)
            {
                // 如果有子过程没有完成，等待子过程
                m_state = ProcessState.WaitChildCompleted;
            }
            else
            {
                // 子过程都执行完成，自己也执行完成，说明所有的process都完成了
                m_state = ProcessState.Completed;

                OnProcessEnd(true);
            }
        }

        /// <summary>
        /// 当取消
        /// </summary>
        protected virtual void OnCancel()
        {
            // 当已经被取消过了，不能再重入
            if (m_state == ProcessState.Canceled)
            {
                return;
            }

            // 直接设置取消状态
            m_state = ProcessState.Canceled;

            OnProcessEnd(false);
        }

        /// <summary>
        /// 当子过程执行完毕
        /// </summary>
        /// <param name="child"></param>
        /// <param name="isCompleted"></param>
        protected void OnChildProcessEnd(CbbProcess child, bool isCompleted)
        {
            if (isCompleted)
            {
                OnChildCompleted(child);
            }
            else
            {
                OnChildCancel(child);
            }
        }

        /// <summary>
        /// 当子过程完成
        /// </summary>
        /// <param name="child"></param>
        protected virtual void OnChildCompleted(CbbProcess child)
        {
            switch (m_processExecMode)
            {
                case ProcessExecMode.Serial:
                    OnChildCompletedForSerialModeImpl(child);
                    break;
                case ProcessExecMode.Parallel:
                    OnChildCompletedForParallelModeImpl(child);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 当子过程完成：串行模式
        /// </summary>
        protected void OnChildCompletedForSerialModeImpl(CbbProcess child)
        {
            // 错误保护
            if (m_childList == null || !m_childList.Contains(child))
            {
                return;
            }

            // 检查主process的状态
            if (m_state != ProcessState.WaitChildCompleted)
            {
                return;
            }

            // 串行执行的process，只需检查最后一个process是否完成即可
            int indexOfChild = m_childList.IndexOf(child);
            bool hasChildNotComplete = (indexOfChild != m_childList.Count - 1);

            if (!hasChildNotComplete)
            {
                // 子过程都执行完成，自己也执行完成，说明所有的process都完成了
                m_state = ProcessState.Completed;

                OnProcessEnd(true);
            }
            else
            {
                // 还有子过程没有执行完成，继续执行下一个
                m_childList[indexOfChild + 1].Start(OnChildProcessEnd);
            }
        }

        /// <summary>
        /// 当子过程完成：并行模式
        /// </summary>
        protected void OnChildCompletedForParallelModeImpl(CbbProcess child)
        {
            // 错误保护
            if (m_childList == null || !m_childList.Contains(child))
            {
                return;
            }

            // 检查主process的状态
            if (m_state != ProcessState.WaitChildCompleted)
            {
                return;
            }

            // 并行执行的process，需要对每一个process进行检查，所有都完成才行
            bool hasChildNotComplete = false;
            foreach (var lchild in m_childList)
            {
                if (lchild.State != ProcessState.Completed)
                {
                    hasChildNotComplete = true;
                    break;
                }
            }

            if (!hasChildNotComplete)
            {
                // 子过程都执行完成，自己也执行完成，说明所有的process都完成了
                m_state = ProcessState.Completed;

                OnProcessEnd(true);
            }
        }

        /// <summary>
        /// 当子过程取消
        /// </summary>
        protected virtual void OnChildCancel(CbbProcess child)
        {
            // 子过程取消，直接设置取消状态
            m_state = ProcessState.Canceled;

            OnProcessEnd(false);
        }

        /// <summary>
        /// 结束procss
        /// </summary>
        /// <param name="isCompleted"></param>
        protected void OnProcessEnd(bool isCompleted)
        {
            // 调用调试日志函数，
            if (DebugOnEnd != null)
            {
                DebugOnEnd();
                DebugOnEnd = null;
            }

            // 通知结束
            if (m_onEnd != null)
            {
                m_onEnd(this, isCompleted);
                m_onEnd = null;
            }

        }
        #endregion

        #region 成员定义

        /// <summary>
        /// 结束事件回调
        /// </summary>
        /// <param name="isCompleted"></param>
        public delegate void OnEnd(CbbProcess process, bool isCompleted);


        public delegate void DebugOnEndFunc();
        /// <summary>
        /// 测试结束事件的回调，只能在此回调中输出日志！！不能进行逻辑
        /// </summary>
        public DebugOnEndFunc DebugOnEnd { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public ProcessState State
        {
            get { return m_state; }
            set { m_state = value; }
        }
        protected ProcessState m_state = ProcessState.Init;

        /// <summary>
        /// 结束事件回调
        /// </summary>
        protected OnEnd m_onEnd;

        /// <summary>
        /// 子process列表
        /// </summary>
        protected List<CbbProcess> m_childList;

        /// <summary>
        /// process的执行方式
        /// </summary>
        protected ProcessExecMode m_processExecMode;

        /// <summary>
        /// process名字
        /// </summary>
        public string ProcessName
        {
            get
            {
                return m_processName;
            }
        }
        protected string m_processName;
        #endregion

        #region 类型定义

        /// <summary>
        /// 停止选项
        /// </summary>
        public enum StopOption
        {
            Complete,    // 立刻完成
            Cancel      // 在当前位置停止，不触发完成
        }

        /// <summary>
        /// 状态
        /// </summary>
        public enum ProcessState
        {
            Init,
            Started,
            Canceled,
            WaitChildCompleted,     // 自己完成了，但是子过程还没完成
            Completed,
        }

        /// <summary>
        /// process的执行方式
        /// </summary>
        public enum ProcessExecMode
        {
            Serial,     // 串行化执行
            Parallel    // 并行化执行
        }

        #endregion

        /// <summary>
        /// UIProcess的执行体delegate
        /// </summary>
        public delegate void ProcessExecutor(Action<bool> onExecEnd);

        /// <summary>
        /// 执行体
        /// </summary>
        private readonly ProcessExecutor m_processExecutor;
    }
}
