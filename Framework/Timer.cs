using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework
{
    public static class Timer
    {
        /// <summary>
        /// 得到上一次tick和这次tick的时间差
        /// </summary>
        /// <returns></returns>
        public static uint GetLastFrameDeltaTimeMs()
        {
            return (uint)((m_currTime - m_lastTickTime).TotalMilliseconds);
        }

        /// <summary>
        /// 得到当前的帧计数
        /// </summary>
        /// <returns></returns>
        public static int GetCurrFrameCount()
        {
            return UnityEngine.Time.frameCount;
        }

        public static void Tick()
        {
            m_lastTickTime = m_currTime;
            m_currTime = System.DateTime.Now;
            m_currTick++;
        }

        public static System.DateTime m_currTime;
        public static ulong m_currTick;
        public static System.DateTime m_lastTickTime = System.DateTime.MaxValue;
    }
}
