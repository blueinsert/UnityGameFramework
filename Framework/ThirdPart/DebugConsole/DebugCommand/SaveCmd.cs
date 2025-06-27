using UnityEngine;
using System.Collections;

public class SaveCmd : IDebugCmd
{

    public void Execute(string strParams)
    {
        DebugConsoleMode.instance.Save();
    }

    public string GetHelpDesc()
    {
        return "save : Save the current log";
    }

    public string GetName()
    {
        return "Save";
    }

}
