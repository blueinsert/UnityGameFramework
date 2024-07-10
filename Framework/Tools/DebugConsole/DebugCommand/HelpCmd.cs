
public class HelpCmd : IDebugCmd
{
    public const string _NAME = "help";
    public const string _DESC = "help: print all command's description";

    public void Execute(string strParams)
    {
        DebugCmdManager.instance.PringAllCmdDescription();
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
