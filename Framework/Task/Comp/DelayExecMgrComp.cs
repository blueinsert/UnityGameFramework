using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class DelayTimeExecItem
    {
        public DateTime m_execTargetTime;  // 到达该时间点被执行
        public Action m_action;         // 执行动作
    }

    /// <summary>
    /// 用于支持延迟执行
    /// </summary>
    public class DelayExecItem
    {
        public ulong m_execTargetTick;  // 到达该帧时执行
        public Action m_action;         // 执行动作
    }

    public interface IDelayExecMgr
    {
        /// <summary>
        /// 投递延迟事件，按时间delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        public void PostDelayTimeExecuteAction(Action action, float delaySeconds);

        /// <summary>
        /// 投递延迟事件，按tick数delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        public void PostDelayTicksExecuteAction(Action action, ulong delayTickCount);

    }

    public class DelayExecMgrComp : ITickable, IDelayExecMgr
    {
        /// <summary>
        /// 延迟一段时间执行的动作列表
        /// </summary>
        private LinkedList<DelayTimeExecItem> m_delayTimeExecList = new LinkedList<DelayTimeExecItem>();

        /// <summary>
        /// 用于支持按帧执行的数据结构
        /// </summary>
        private LinkedList<DelayExecItem> m_delayExecList = new LinkedList<DelayExecItem>();

        #region 公共方法
        /// <summary>
        /// 投递延迟事件，按时间delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        public void PostDelayTimeExecuteAction(Action action, float delaySeconds)
        {
            // 参数校验
            if (delaySeconds < 0)
            {
                delaySeconds = 0;
            }

            // 创建一个item
            var item = new DelayTimeExecItem
            {
                m_execTargetTime = Timer.m_currTime.AddSeconds(delaySeconds),
                m_action = action
            };

            // 按执行时间从小到大item插入列表
            if (m_delayTimeExecList.Count == 0 || m_delayTimeExecList.Last.Value.m_execTargetTime <= item.m_execTargetTime)
            {
                // 如果最后一个item也是最晚执行的，则应直接插入到列表末尾，提升效率
                m_delayTimeExecList.AddLast(item);
            }
            else
            {
                // 否则说明item应该插入到列表中间的某个位置，需要遍历列表找到位置
                var currExecItemNode = m_delayTimeExecList.First;
                while (currExecItemNode.Value.m_execTargetTime <= item.m_execTargetTime)
                {
                    // 找到列表中第一个执行时间大于item的Node，插入到该Node前面
                    currExecItemNode = currExecItemNode.Next;
                }

                // 将item插入到找到的Node前面
                m_delayTimeExecList.AddBefore(currExecItemNode, item);
            }
        }


        /// <summary>
        /// 投递延迟事件，按tick数delay
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
        /// 按tick执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        protected void ExecAfterTicks(Action action, ulong delayTickCount = 1)
        {
            // 按tick存入m_execDelayTicksList，线程安全
            if (m_delayExecList == null)
            {
                m_delayExecList = new LinkedList<DelayExecItem>();
            }

            // 创建一个item
            DelayExecItem item = new DelayExecItem
            {
                m_execTargetTick = Timer.m_currTick + delayTickCount,
                m_action = action
            };

            // 按执行时间从小到大item插入列表

            if (m_delayExecList.Count == 0 || m_delayExecList.Last.Value.m_execTargetTick <= item.m_execTargetTick)
            {
                // 如果最后一个item也是最晚执行的，则应直接插入到列表末尾，提升效率
                m_delayExecList.AddLast(item);
            }
            else
            {
                // 否则说明item应该插入到列表中间的某个位置，需要遍历列表找到位置
                var currExecItemNode = m_delayExecList.First;
                while (currExecItemNode != null && currExecItemNode.Value.m_execTargetTick <= item.m_execTargetTick)
                {
                    // 找到列表中第一个执行时间大于item的Node，插入到该Node前面
                    currExecItemNode = currExecItemNode.Next;
                }

                // 将item插入到找到的Node前面
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
                        // 执行时间到达，需要执行
                        execItem.m_action();

                        // 执行完毕，将execItem从列表移除
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
            // 执行按tick执行的逻辑
            if (m_delayExecList != null)
            {
                DelayExecItem execItem;
                while (m_delayExecList.First != null)
                {
                    execItem = m_delayExecList.First.Value;

                    if (execItem.m_execTargetTick <= Timer.m_currTick)
                    {
                        // 执行时间到达，需要执行
                        execItem.m_action();

                        // 执行完毕，将execItem从列表移除
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
