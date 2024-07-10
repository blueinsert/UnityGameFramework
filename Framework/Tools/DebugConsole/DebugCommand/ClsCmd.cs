
public class ClsCmd : IDebugCmd
{
    private const string _NAME = "cls";
    private const string _DESC = "cls: clear console screen.";

    public void Execute(string strParams)
    {
        DebugConsoleMode.instance.ClearLog();
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
