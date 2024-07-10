using System;

public interface IDebugCmd
{
    void Execute(string strParams);
    string GetHelpDesc();
    string GetName();
}
