using System;

public class SetFontSizeCmd : IDebugCmd
{
    private const string _NAME = "sfs";
    private const string _DESC = "sfs [int]: set the font size of debug console.";
    private const string _SCHEMA = "i";

    public void Execute(string strParams)
    {
        ArgumentsParser parser = new ArgumentsParser(_SCHEMA, strParams);
        DebugConsoleView.instance.SetFontSize(parser.GetInt32(0));
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