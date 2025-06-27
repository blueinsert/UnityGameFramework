using System;
using System.Collections.Generic;

public class FilterLogCmd : IDebugCmd
{
    private const string _NAME = "filter";
    private const string _DESC = "filter include [string]: set include filter string,\n filter exclude [string]: set exclude filter string\nfilter clear: clear all filters.\n filter string format: s1;s2;s3...";
    private const string _SCHEMA = "s s";

    public void Execute(string strParams)
    {
        ArgumentsParser parser = new ArgumentsParser(_SCHEMA, strParams);
        string s1 = parser.GetString(0);
        string s2 = parser.GetString(1);
        if (s1.ToLower() == "include")
        {
            List<string> strArr = new List<string>(s2.Split(';'));
            DebugConsoleMode.instance.SetFilterString(strArr, null);

        }
        else if (s1.ToLower() == "exclude")
        {
            List<string> strArr = new List<string>(s2.Split(';'));
            DebugConsoleMode.instance.SetFilterString(null, strArr);
        }
        else if (s1.ToLower() == "clear")
        {
            DebugConsoleMode.instance.SetFilterString(new List<string>(), new List<string>());
        }
        //DebugConsoleMode.instance.Log(sum.ToString());
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