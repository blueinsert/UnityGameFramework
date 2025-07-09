using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace bluebean.UGFramework
{
    public static class Utility
    {
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PureSign(this float val)
        {
            return ((0 <= val) ? 1 : 0) - ((val < 0) ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static string ExecuteProcess(string fileName,string workDir,bool useShell, params string[] arguments)
        {
            var totalCmd = $"{fileName} {string.Join(" ", arguments)}";
            int maxWaitTime = 20;
            bool redirectOutput = !useShell;
            bool createWindow = useShell;
            try
            {
                //Debug.Log($"开始执行命令:{fileName} {string.Join(" ", arguments)}");
                bool res = false;
                string output, error;
                int exitCode = -1;
                //lock (LeidianPlayer.Instance.LDLock)
                {
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = string.Join(" ", arguments),
                        WorkingDirectory = workDir,
                        RedirectStandardError = redirectOutput,
                        RedirectStandardInput = redirectOutput,
                        RedirectStandardOutput = redirectOutput,
                        UseShellExecute = useShell,
                        CreateNoWindow = !createWindow,                  // 是否显示命令行窗口
                        WindowStyle = ProcessWindowStyle.Normal  // 窗口样式
                    };
                    process.Start();
                    StringBuilder outputSb = new StringBuilder();
                    StringBuilder errorSb = new StringBuilder();

                    if (redirectOutput)
                    {
                        // 异步读取输出和错误流
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                //BluebeanAutoConsole.Debug.Log($"process.OutputDataReceived: {e.Data}");
                                // 处理输出数据（例如记录日志）
                                outputSb.AppendLine(e.Data);
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                // 处理错误数据（例如记录日志）
                                //BluebeanAutoConsole.Debug.Log($"process.ErrorDataReceived: {e.Data}");
                                errorSb.AppendLine(e.Data);
                            }
                        };
                    }
                    
                    res = process.WaitForExit(1000 * maxWaitTime);
                    //Debug.Log($"命令执行结束:{fileName} {string.Join(" ", arguments)}");
                    if (!res)
                    {
                        Debug.Log($"Executable:Process <{totalCmd}>, outTime 超过{maxWaitTime}s");
                        process.Kill();
                        process.Close();
                        process.Dispose();
                        return "timeout";
                    }
                    exitCode = process.ExitCode;
                    error = errorSb.ToString();// process.StandardError.ReadToEnd();
                    output = outputSb.ToString();// process.StandardOutput.ReadToEnd();
                    process.Close();
                    process.Dispose();
                }

                if (exitCode != 0)
                {
                    Debug.Log($"Process failed, <{totalCmd}> exitCode:{exitCode} msg:{error}");
                }
                return output;
            }
            catch (Exception ex)
            {
                Debug.Log($"Executable:Process <{totalCmd}> exception:{ex.ToString()}");
                throw;
            }
            return "";
        }

        /// <summary>
        /// 手动解析16进制颜色字符串（支持#RRGGBB或#RRGGBBAA格式）
        /// </summary>
        public static Color HexToColor(string hex)
        {
            hex = hex.Trim().Replace("#", "");

            switch (hex.Length)
            {
                case 6: // RRGGBB
                    return new Color(
                        HexToInt(hex.Substring(0, 2)) / 255f,
                        HexToInt(hex.Substring(2, 2)) / 255f,
                        HexToInt(hex.Substring(4, 2)) / 255f
                    );

                case 8: // RRGGBBAA
                    return new Color(
                        HexToInt(hex.Substring(0, 2)) / 255f,
                        HexToInt(hex.Substring(2, 2)) / 255f,
                        HexToInt(hex.Substring(4, 2)) / 255f,
                        HexToInt(hex.Substring(6, 2)) / 255f
                    );

                default:
                    Debug.LogError($"不支持的格式: {hex}");
                    return Color.magenta;
            }
        }

        private static int HexToInt(string hex)
        {
            return System.Convert.ToInt32(hex, 16);
        }
    }
}
