using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class DelayTimeExecItem
    {
        public DateTime m_execTargetTime;  // ����һ��ʱ��㱻ִ��
        public Action m_action;         // ִ����
    }

    /// <summary>
    /// ����֧��delayexec
    /// </summary>
    public class DelayExecItem
    {
        public ulong m_execTargetTick;  // ����һ֡��ִ��
        public Action m_action;         // ִ����
    }

    public interface IDelayExecMgr
    {
        /// <summary>
        /// Ͷ���ӳ��¼����������delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        public void PostDelayTimeExecuteAction(Action action, float delaySeconds);

        /// <summary>
        /// Ͷ���ӳ��¼�����tick����delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        public void PostDelayTicksExecuteAction(Action action, ulong delayTickCount);

    }

    public class DelayExecMgrComp : ITickable, IDelayExecMgr
    {
        /// <summary>
        /// �ӳ�һ��ʱ��ִ�е���Ϊ�б�
        /// </summary>
        private LinkedList<DelayTimeExecItem> m_delayTimeExecList = new LinkedList<DelayTimeExecItem>();

        /// <summary>
        /// ����֧����ʱִ�е����ݽṹ
        /// </summary>
        private LinkedList<DelayExecItem> m_delayExecList = new LinkedList<DelayExecItem>();

        #region ���ⷽ��
        /// <summary>
        /// Ͷ���ӳ��¼����������delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        public void PostDelayTimeExecuteAction(Action action, float delaySeconds)
        {
            // �����ݴ�
            if (delaySeconds < 0)
            {
                delaySeconds = 0;
            }

            // ����һ��item
            var item = new DelayTimeExecItem
            {
                m_execTargetTime = Timer.m_currTime.AddSeconds(delaySeconds),
                m_action = action
            };

            // ����ִ��ʱ����Ⱥ�˳��item�����б�
            if (m_delayTimeExecList.Count == 0 || m_delayTimeExecList.Last.Value.m_execTargetTime <= item.m_execTargetTime)
            {
                // �����ж�һ��item�ǲ������һ��ִ�еģ����Ƿ�Ӧ�ü����б�ĩβ�����������ʵ��ʹ�����ģ�
                m_delayTimeExecList.AddLast(item);
            }
            else
            {
                // ����Ļ���˵��itemӦ�ò��뵽�б��м��ĳ��λ�ã�������Ҫ�����б��ҵ�λ�ò���
                var currExecItemNode = m_delayTimeExecList.First;
                while (currExecItemNode.Value.m_execTargetTime <= item.m_execTargetTime)
                {
                    // �ҵ��б��У�������ִ��ʱ�䲻����item�����һ��Node������һ��Node
                    currExecItemNode = currExecItemNode.Next;
                }

                // ��item���뵽������ִ��ʱ�䲻����item�����һ��Node������һ��Node��ǰ��
                m_delayTimeExecList.AddBefore(currExecItemNode, item);
            }
        }


        /// <summary>
        /// Ͷ���ӳ��¼�����tick����delay
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

        #region �ڲ�����

        /// <summary>
        /// ��ʱִ��
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTickCount"></param>
        protected void ExecAfterTicks(Action action, ulong delayTickCount = 1)
        {
            // ��ʱ����m_execDelayTicksList�����̰߳�ȫ
            if (m_delayExecList == null)
            {
                m_delayExecList = new LinkedList<DelayExecItem>();
            }

            // ����һ��item
            DelayExecItem item = new DelayExecItem
            {
                m_execTargetTick = Timer.m_currTick + delayTickCount,
                m_action = action
            };

            // ����ִ��ʱ����Ⱥ�˳��item�����б�

            if (m_delayExecList.Count == 0 || m_delayExecList.Last.Value.m_execTargetTick <= item.m_execTargetTick)
            {
                // �����ж�һ��item�ǲ������һ��ִ�еģ����Ƿ�Ӧ�ü����б�ĩβ�����������ʵ��ʹ�����ģ�
                m_delayExecList.AddLast(item);
            }
            else
            {
                // ����Ļ���˵��itemӦ�ò��뵽�б��м��ĳ��λ�ã�������Ҫ�����б��ҵ�λ�ò���
                var currExecItemNode = m_delayExecList.First;
                while (currExecItemNode != null && currExecItemNode.Value.m_execTargetTick <= item.m_execTargetTick)
                {
                    // �ҵ��б��У�������ִ��ʱ�䲻����item�����һ��Node������һ��Node
                    currExecItemNode = currExecItemNode.Next;
                }

                // ��item���뵽������ִ��ʱ�䲻����item�����һ��Node������һ��Node��ǰ��
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
                        // ִ��ʱ�䵽����Ҫִ��
                        execItem.m_action();

                        // ִ����ϣ���execItem���б����Ƴ�
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
            // ִ����ʱִ���߼�
            if (m_delayExecList != null)
            {
                DelayExecItem execItem;
                while (m_delayExecList.First != null)
                {
                    execItem = m_delayExecList.First.Value;

                    if (execItem.m_execTargetTick <= Timer.m_currTick)
                    {
                        // ִ��ʱ�䵽����Ҫִ��
                        execItem.m_action();

                        // ִ����ϣ���execItem���б����Ƴ�
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
