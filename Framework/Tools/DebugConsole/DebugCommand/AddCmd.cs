using UnityEngine;
using System;

public class AddCmd : IDebugCmd
{
    private const string _NAME = "intadd";
    private const string _DESC = "intadd [int] [int]: add two int num.";
    private const string _SCHEMA = "i i";

    public void Execute(string strParams)
    {
        ArgumentsParser parser = new ArgumentsParser(_SCHEMA, strParams);
        Int32 num1 = parser.GetInt32(0);
        Int32 num2 = parser.GetInt32(1);
        Int32 sum = num1 + num2;
        //DebugConsole.instance.Log(sum.ToString());
        Debug.Log(sum.ToString());
    }

    public string GetHelpDesc()
    {
        return _DESC;
    }

    public string GetName()
    {
        return _NAME;
    }
}