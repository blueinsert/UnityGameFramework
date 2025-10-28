using bluebean.UGFramework.ConfigData;
using bluebean.UGFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System;

namespace bluebean.UGFramework
{
    public class SystemUtility : MonoBehaviour
    {
        // 加密过的TimeScale，防止外挂修改内存加速
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
        /// 预加载判断是否低系统内存
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
        /// 预加载判断是否高系统内存
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
        /// 获取电池充电状态
        /// </summary>
        /// <returns></returns>
        public static BatteryStatus GetBatteryStatus()
        {
            return SystemInfo.batteryStatus;
        }

        /// <summary>
        /// 获取电池电量
        /// </summary>
        /// <returns></returns>
        public static float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }

        /// <summary>
        /// 记录电池状态和电量日志
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

        public static string GetCurrentTargetPlatform()
        {
            string platform = "";
            if (string.IsNullOrEmpty(platform))
            {
                if (!Application.isEditor)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            platform = "Android";
                            break;
                        case RuntimePlatform.IPhonePlayer:
                            platform = "iOS";
                            break;
                        case RuntimePlatform.OSXPlayer:
                            platform = "StandaloneOSXIntel";
                            break;
                        case RuntimePlatform.WindowsPlayer:
                            if (IntPtr.Size == 8)
                                platform = "StandaloneWindows64";
                            else
                                platform = "StandaloneWindows";
                            break;
                        case RuntimePlatform.WebGLPlayer:
                            platform = "WebGL";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
#if UNITY_EDITOR
                    platform = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#endif
                }
            }
            return platform;
        }
    }
}