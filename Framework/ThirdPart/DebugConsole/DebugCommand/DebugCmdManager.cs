using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using System.IO;

public class DebugCmdManager
{

    private Dictionary<string, IDebugCmd> _cmdMap;
    private static DebugCmdManager _instance;
    private List<string> _oldCommands = new List<string>(50);
    private int _showOldCommandIdx = 0;

    private DebugCmdManager()
    {
        _instance = this;
    }

    public static DebugCmdManager Create(IConsoleMode debugConsoleMode)
    {
        DebugCmdManager debugMgr = new DebugCmdManager();
        debugMgr._Init(debugConsoleMode);
        return debugMgr;
    }

    void AddCmd(Type cmdType)
    {
        object o = Activator.CreateInstance(cmdType);
        IDebugCmd c = (IDebugCmd)o;
        Assert.IsNotNull(c, string.Format("Failed to create command: {0}", cmdType));
        Assert.IsNotNull(_cmdMap, "_cmdMap is null");
        _cmdMap.Add(c.GetName().ToLower(), c);
    }

    public void LoadIDebugCmds(Assembly a)
    {
        if (a == null)
            return;
        if(_cmdMap==null)
            _cmdMap = new Dictionary<string, IDebugCmd>();
        Type[] types = a.GetTypes();
        foreach (Type t in types)
        {
            if (t.GetInterface("IDebugCmd") != null)
            {
                AddCmd(t);
            }
        }
    }

    private void _Init(IConsoleMode consoleMode)
    {
        LoadIDebugCmds(Assembly.GetExecutingAssembly());
        
        LoadOldCommands();
    }

    public void RunInstruct(string instruct)
    {
        string strInstruct = instruct.Trim();
        string cmdName = _GetCommandName(strInstruct);

        string strCmdParams = _GetCommandParams(strInstruct);
        IDebugCmd cmd = _GetCommand(cmdName);

        if (cmd != null)
        {
            cmd.Execute(strCmdParams);
            _showOldCommandIdx = 0;
            if (_oldCommands.Count == 0 || 
                _oldCommands[_oldCommands.Count - 1].ToLower() != instruct.ToLower())
            {
                _oldCommands.Add(instruct);
            }
            SaveOldCommands();
        }
        else
        {
            _PrintWrongCommandHint();
        }
    }

    private void _PrintWrongCommandHint()
    {
        Debug.LogError("Wrong command!use \"help\" to show all command.");
    }

    private string _GetCommandName(string instruct)
    {
        string cmd = instruct;
        int pos = instruct.IndexOf(' ');
        if (pos != -1)
        {
            cmd = instruct.Substring(0, pos).Trim();
        }

        return cmd.ToLower();
    }

    private string _GetCommandParams(string instruct)
    {
        string cmdParams = "";
        int pos = instruct.IndexOf(' ');
        if (pos != -1)
        {
            cmdParams = instruct.Substring(pos + 1).Trim();
        }
        return cmdParams;
    }

    private IDebugCmd _GetCommand(string cmdName)
    {
        cmdName = cmdName.ToLower();
        IDebugCmd cmd;
        if (_cmdMap.TryGetValue(cmdName, out cmd))
        {
            return cmd;
        }
        return null;
    }

    public void PringAllCmdDescription()
    {
        Debug.Log("*******Command List End******");
        foreach (KeyValuePair<string, IDebugCmd> kv in _cmdMap)
        {
            Debug.Log(kv.Value.GetHelpDesc());
        }
        Debug.Log("Triple click (continuous click 3 times ) the window to toggle [Auto Scrolling View].");
        Debug.Log("*******Command List Start*******");
    }

    public Int32 GetCommandNumber()
    {
        if (_cmdMap != null)
        {
            return _cmdMap.Count;
        }
        return 0;
    }

    public static DebugCmdManager instance
    {
        get
        {
            Assert.IsNotNull(_instance, "_instance is null");
            return _instance;
        }
    }

    void SaveOldCommands()
    {
        string filename = string.Format("{0}{1}DebugConsoleCommands.txt",
                                 Application.persistentDataPath, Path.AltDirectorySeparatorChar);
        StreamWriter sw = new StreamWriter(filename);

        Assert.IsNotNull(sw, "Failed to create StreamWriter.");
        foreach (string s in _oldCommands)
        {
            sw.WriteLine(s);
        }
        sw.Close();

        //Debug.Log(string.Format("【{0}】 saved.", filename));
    }

    void LoadOldCommands()
    {
        string filename = string.Format("{0}{1}DebugConsoleCommands.txt",
                                     Application.persistentDataPath, Path.AltDirectorySeparatorChar);
        try
        {
            if (!File.Exists(filename))
                return;
            StreamReader sr = new StreamReader(filename);
            _oldCommands.Clear();
            while (!sr.EndOfStream)
            {
                string cmd = sr.ReadLine();
                if (string.IsNullOrEmpty(cmd))
                {
                    continue;
                }
                _oldCommands.Add(cmd);
            }
            sr.Close();
            //Debug.Log(string.Format("【{0}】 loaded.", filename));
        }
        catch (FileNotFoundException)
        {
            Debug.Log(string.Format("[{0}] not found.", filename));
        }
        catch (FileLoadException)
        {
            Debug.LogError(string.Format("Failed to load [{0}].", filename));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public string GetNextOldCommand()
    {
        string cmd = "";
        if (_oldCommands.Count > 1)
        {
            _showOldCommandIdx--;
            _showOldCommandIdx = Math.Max(Math.Min(_showOldCommandIdx, _oldCommands.Count), 0);
            if(_showOldCommandIdx>0)
                cmd = _oldCommands[_oldCommands.Count - _showOldCommandIdx];
        }
        return cmd;
    }

    public string GetPreviousOldCommand()
    {
        string cmd = "";
        if (_oldCommands.Count > 1)
        {
            _showOldCommandIdx++;
            _showOldCommandIdx = Math.Max(Math.Min(_showOldCommandIdx, _oldCommands.Count), 1);

            cmd = _oldCommands[_oldCommands.Count - _showOldCommandIdx];
        }
        return cmd;
    }
}
