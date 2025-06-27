using UnityEngine;
using System.Collections;

public class DebugConsoleCtrl
{

    private DebugConsoleView _consoleView;
    private DebugConsoleMode _consoleMode;

    private DebugConsoleCtrl()
    {

    }

    public static DebugConsoleCtrl Create(DebugConsoleView consoleView, DebugConsoleMode consoleMode)
    {
        DebugConsoleCtrl ctrl = new DebugConsoleCtrl();
        ctrl._Init(consoleView, consoleMode);
        return ctrl;
    }

    private void _Init(DebugConsoleView consoleView, DebugConsoleMode consoleMode)
    {
        _consoleView = consoleView;
        _consoleMode = consoleMode;
        _consoleView.OnKeyDown += _ConsoleView_OnKeyDown;
        _consoleView.OnKeyUp += _ConsoleView_OnKeyUp;
        _consoleView.OnOKButtonClick = _ConsoleView_OnBtnOKClick;
        _consoleView.OnCloseButtonClick = _ConsoleView_OnBtnCloseClick;
        _consoleMode.refreshEvent += _ConsoleModeLogRefresh;
    }

    private void _ConsoleView_OnKeyUp(object sender, KeyUpEventArgs e)
    {

    }

    private void _ConsoleView_OnKeyDown(object sender, KeyDownEventArgs e)
    {
        if (e.KeyCode == KeyCode.Return)
        {
            _ProcessCmd();
        }
        else if (e.KeyCode == KeyCode.UpArrow)
        {
            _consoleView.ShowPreviousCommand();
        }
        else if (e.KeyCode == KeyCode.DownArrow)
        {
            _consoleView.ShowNextCommand();
        }
    }

    private void _ConsoleView_OnBtnCloseClick()
    {
        if(_consoleView!=null)
            _consoleView.IsSwitchOn = false;
    }

    private void _ConsoleView_OnBtnOKClick()
    {
        _ProcessCmd();
    }

    private void _ProcessCmd()
    {
        Debug.Log(_consoleView.InstructionText);
        _consoleMode.ProcessCmd(_consoleView.InstructionText);
        _consoleView.InstructionText = "";
    }

    private void _ConsoleModeLogRefresh()
    {
        _consoleView.LogText = _consoleMode.GetLogText();
    }
}
