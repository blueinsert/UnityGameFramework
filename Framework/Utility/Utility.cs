using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace bluebean.UGFramework
{
    public class Utility
    {
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
