using bluebean.UGFramework.ConfigData;
using bluebean.UGFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace bluebean.UGFramework
{
    public class SystemUtility : MonoBehaviour
    {
        // ���ܹ���TimeScale����ֹ����޸��ڴ����
        private static readonly System.Random _random = new System.Random();
        private static int _timeScale = (int)(Time.timeScale * 10000);
        private static readonly int _timeScaleMask = _random.Next(int.MinValue, int.MaxValue);
        public static float TimeScale
        {
            get
            {
                return (_timeScale ^ _timeScaleMask) / 10000.0f;
            }
            set
            {
                _timeScale = (int)(value * 10000) ^ _timeScaleMask;
            }
        }

        public static void SetTimeScale(float scale)
        {
            TimeScale = scale;
            Time.timeScale = scale;
        }

        /// <summary>
        /// Ԥ�����ж��Ƿ��ϵͳ�ڴ�
        /// </summary>
        /// <returns></returns>
        public static bool IsLowSystemMemory()
        {
#if UNITY_EDITOR
            return true;
#else
            return SystemInfo.systemMemorySize <= 1100;
#endif
        }

        /// <summary>
        /// Ԥ�����ж��Ƿ��ϵͳ�ڴ�
        /// </summary>
        /// <returns></returns>
        public static bool IsLargeSystemMemory()
        {
#if UNITY_EDITOR
            return false;
#else
             return SystemInfo.systemMemorySize > 4000;
#endif
        }

        /// <summary>
        /// ��ȡ��س��״̬
        /// </summary>
        /// <returns></returns>
        public static BatteryStatus GetBatteryStatus()
        {
            return SystemInfo.batteryStatus;
        }

        /// <summary>
        /// ��ȡ��ص���
        /// </summary>
        /// <returns></returns>
        public static float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }

        /// <summary>
        /// ��¼���״̬�͵�����־
        /// </summary>
        public static void LogBatteryStatus()
        {
            Debug.Log(string.Format("Battery Level {0:f2}, Status {1}", GetBatteryLevel(), GetBatteryStatus()));
        }

        public static long GetRuntimeMemorySize()
        {
#if UNITY_IOS
            return Report_memory();
#endif
            return Profiler.GetTotalAllocatedMemoryLong();
        }
    }
}