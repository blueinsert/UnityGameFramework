using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class DelayTimeExecItem
    {
        public DateTime m_execTargetTime;  // 在哪一个时间点被执行
        public Action m_action;         // 执行体
    }

    /// <summary>
    /// 用来支持delayexec
    /// </summary>
    public class DelayExecItem
    {
        public ulong m_execTargetTick;  // 在哪一帧被执行
        public Action m_action;         // 执行体
    }

    public interface IDelayExecMgr
    {
        /// <summary>
        /// 投递延迟事件，按秒进行delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        public void PostDelayTimeExecuteAction(Action action, float delaySeconds);

        /// <summary>
        /// 投递延迟事件，按tick进行delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        public void PostDelayTicksExecuteAction(Action action, ulong delayTickCount);

    }

    public class DelayExecMgrComp : ITickable, IDelayExecMgr
    {
        /// <summary>
        /// 延迟一定时间执行的行为列表
        /// </summary>
        private LinkedList<DelayTimeExecItem> m_delayTimeExecList = new LinkedList<DelayTimeExecItem>();

        /// <summary>
        /// 用来支持延时执行的数据结构
        /// </summary>
        private LinkedList<DelayExecItem> m_delayExecList = new LinkedList<DelayExecItem>();

        #region 对外方法
        /// <summary>
        /// 投递延迟事件，按秒进行delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        public void PostDelayTimeExecuteAction(Action action, float delaySeconds)
        {
            // 参数容错
            if (delaySeconds < 0)
            {
                delaySeconds = 0;
            }

            // 构造一个item
            var item = new DelayTimeExecItem
            {
                m_execTargetTime = Timer.m_currTime.AddSeconds(delaySeconds),
                m_action = action
            };

            // 按照执行时间的先后顺序将item加入列表
            if (m_delayTimeExecList.Count == 0 || m_delayTimeExecList.Last.Value.m_execTargetTime <= item.m_execTargetTime)
            {
                // 首先判断一下item是不是最后一个执行的（即是否应该加入列表末尾，这种情况是实际使用最多的）
                m_delayTimeExecList.AddLast(item);
            }
            else
            {
                // 否则的话，说明item应该插入到列表中间的某个位置，所以需要遍历列表找到位置插入
                var currExecItemNode = m_delayTimeExecList.First;
                while (currExecItemNode.Value.m_execTargetTime <= item.m_execTargetTime)
                {
                    // 找到列表中，“所有执行时间不大于item的最后一个Node”的下一个Node
                    currExecItemNode = currExecItemNode.Next;
                }

                // 将item插入到“所有执行时间不大于item的最后一个Node”的下一个Node的前面
                m_delayTimeExecList.AddBefore(currExecItemNode, item);
            }
        }


        /// <summary>
        /// 投递延迟事件，按tick进行delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        public void PostDelayTicksExecuteAction(Action action, ulong delayTickCount)
        {
            ExecAfterTicks(action, delayTickCount);
        }

        #endregion

        public void Tick()
        {
            TickForDelayTimeExecuteActionList();
            TickForDelayTickExecuteActionList();
        }

        #region 内部方法

        /// <summary>
        /// 延时执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        protected void ExecAfterTicks(Action action, ulong delayTickCount = 1)
        {
            // 延时构造m_execDelayTicksList，非线程安全
            if (m_delayExecList == null)
            {
                m_delayExecList = new LinkedList<DelayExecItem>();
            }

            // 构造一个item
            DelayExecItem item = new DelayExecItem
            {
                m_execTargetTick = Timer.m_currTick + delayTickCount,
                m_action = action
            };

            // 按照执行时间的先后顺序将item加入列表

            if (m_delayExecList.Count == 0 || m_delayExecList.Last.Value.m_execTargetTick <= item.m_execTargetTick)
            {
                // 首先判断一下item是不是最后一个执行的（即是否应该加入列表末尾，这种情况是实际使用最多的）
                m_delayExecList.AddLast(item);
            }
            else
            {
                // 否则的话，说明item应该插入到列表中间的某个位置，所以需要遍历列表找到位置插入
                var currExecItemNode = m_delayExecList.First;
                while (currExecItemNode != null && currExecItemNode.Value.m_execTargetTick <= item.m_execTargetTick)
                {
                    // 找到列表中，“所有执行时间不大于item的最后一个Node”的下一个Node
                    currExecItemNode = currExecItemNode.Next;
                }

                // 将item插入到“所有执行时间不大于item的最后一个Node”的下一个Node的前面
                if (currExecItemNode != null) m_delayExecList.AddBefore(currExecItemNode, item);
            }
        }

        private void TickForDelayTimeExecuteActionList()
        {
            if (m_delayTimeExecList != null && m_delayTimeExecList.Count != 0)
            {
                DelayTimeExecItem execItem;
                while (m_delayTimeExecList.First != null)
                {
                    execItem = m_delayTimeExecList.First.Value;

                    if (execItem.m_execTargetTime <= Timer.m_currTime)
                    {
                        // 执行时间到，需要执行
                        execItem.m_action();

                        // 执行完毕，将execItem从列表中移除
                        m_delayTimeExecList.RemoveFirst();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void TickForDelayTickExecuteActionList()
        {
            // 执行延时执行逻辑
            if (m_delayExecList != null)
            {
                DelayExecItem execItem;
                while (m_delayExecList.First != null)
                {
                    execItem = m_delayExecList.First.Value;

                    if (execItem.m_execTargetTick <= Timer.m_currTick)
                    {
                        // 执行时间到，需要执行
                        execItem.m_action();

                        // 执行完毕，将execItem从列表中移除
                        m_delayExecList.RemoveFirst();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
