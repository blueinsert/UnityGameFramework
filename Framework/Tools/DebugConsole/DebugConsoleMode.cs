using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.IO;
using System;

public interface IConsoleMode
{
    void ClearLog();
}

public class DebugConsoleMode : IConsoleMode
{
    public delegate void refreshDelegate();
    public event refreshDelegate refreshEvent;

    //private string _logText;
    private const int _maxLogLength = 10000;
    private StringBuilder _logText = new StringBuilder(_maxLogLength * 2);
    private StringBuilder _removedLogText = new StringBuilder(_maxLogLength*5);
    private DebugCmdManager _debugCmdManager;
    private static DebugConsoleMode _instance;
    private List<string> _includeFilterStrings;
    private List<string> _excludeFilterStrings;
    bool _enableRuntimeLogFile = false;
    StreamWriter _runtimeLogWriter;

    public void SetFilterString(List<string> includeStrings, List<string> excludeStrings)
    {
        if (includeStrings != null)
        {
            _includeFilterStrings = includeStrings;
        }

        if (excludeStrings != null)
        {
            _excludeFilterStrings = excludeStrings;
        }
    }

    private DebugConsoleMode()
    {
        _instance = this;
    }

    public static DebugConsoleMode Create()
    {
        DebugConsoleMode mode = new DebugConsoleMode();
        mode._Init();
        return mode;
    }

    private void _Init()
    {
        _debugCmdManager = DebugCmdManager.Create(this);
        Application.logMessageReceived += _LogCallback;
    }

    public void _LogReceived(string log)
    {
        Log(log);
    }

    public void _LogCallback(string condition, string stackTrace, UnityEngine.LogType type)
    {
        if (type == LogType.Error || type == LogType.Assert)
        {
            Log(string.Format("[E] {0}", condition));
        }
        else if (type == LogType.Warning)
        {
            Log(string.Format("[W] {0}", condition));
        }
        else
        {
            Log(string.Format("[D] {0}", condition));
        }
    }

    public void ClearLog()
    {
        _removedLogText.Append(_logText);
        _logText.Remove(0, _logText.Length);

        if (refreshEvent != null)
            refreshEvent();
    }

    void Log(string info)
    {
        if (_includeFilterStrings != null && _includeFilterStrings.Count > 0)
        {
            int count = 0;
            foreach (string s in _includeFilterStrings)
            {
                if (info.IndexOf(s) >= 0)
                {
                    count++;
                }
            }
            if (count == 0)
            {
                return;
            }
        }
        if (_excludeFilterStrings != null && _excludeFilterStrings.Count > 0)
        {
            foreach (string s in _excludeFilterStrings)
            {
                if (info.IndexOf(s) >= 0)
                {
                    return;
                }
            }
        }

        if (_logText != null && _logText.Length > _maxLogLength)
        {
            int removeLength = (int)(_logText.Length * 0.2);
            _removedLogText.Append(_logText.ToString(0, removeLength));
            _logText.Remove(0, removeLength);
        }
        _logText.AppendLine(info); ;
        Log2File(info);

        if (refreshEvent != null)
            refreshEvent();
    }

    public string GetLogText()
    {
        return _logText.ToString();
    }

    public void ProcessCmd(string instruction)
    {
        _debugCmdManager.RunInstruct(instruction);
    }

    public static DebugConsoleMode instance
    {
        get
        {
            Assert.IsNotNull(_instance, "_instance is null");
            return _instance;
        }
    }

    public void Save()
    {
        string logFilename = string.Format("{0}{1}{2}_{3}{4:D2}{5:D2}_{6:D2}{7:D2}{8:D2}.log",
                                 Application.persistentDataPath, Path.AltDirectorySeparatorChar,
                                 "DebugLog", DateTime.Now.Year, DateTime.Now.Month,
                                 DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                                 DateTime.Now.Second);
        StreamWriter sw = new StreamWriter(logFilename);

        Assert.IsNotNull(sw, "Failed to create StreamWriter.");
        sw.Write(_removedLogText);
        sw.Write(_logText);
        sw.Close();

        Log(string.Format("【{0}】 saved.", logFilename));
    }

    void Log2File(string log)
    {
        if (!_enableRuntimeLogFile)
            return;
        _runtimeLogWriter.WriteLine(log);
        _runtimeLogWriter.Flush();
    }

    public void EnableRuntimeLogFile(bool isEnable)
    {
        if (_enableRuntimeLogFile == isEnable)
        {
            return;
        }
        _enableRuntimeLogFile = isEnable;

        if (_enableRuntimeLogFile)
        {
            string filename = string.Format("{0}{1}{2}_{3}{4:D2}{5:D2}_{6:D2}{7:D2}{8:D2}.log",
                                 Application.persistentDataPath, Path.AltDirectorySeparatorChar,
                                 "RuntimeLog", DateTime.Now.Year, DateTime.Now.Month,
                                 DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                                 DateTime.Now.Second);
            _runtimeLogWriter = new StreamWriter(filename);
            Assert.IsNotNull(_runtimeLogWriter, "Failed to create StreamWriter.");
        }
        else
        {
            _runtimeLogWriter.Dispose();
            _runtimeLogWriter = null;
        }
    }

}
