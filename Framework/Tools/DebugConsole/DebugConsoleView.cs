using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions;
using System.IO;
using UnityEngine.Profiling;

public class DebugConsoleView
{
    public event EventHandler<KeyDownEventArgs> OnKeyDown;
    public event EventHandler<KeyUpEventArgs> OnKeyUp;

    public Action OnOKButtonClick;
    public Action OnCloseButtonClick;

    private const Int32 CONSOLE_MARGIN_X = 5;
    private const Int32 CONSOLE_MARGIN_Y = 5;

    private const Int32 SWITCHBTN_MARGIN_X = 5;
    private const Int32 SWITCHBTN_MARGIN_Y = 5;
    private const Int32 SWITCHBTN_WIDTH = 30;
    private const Int32 SWITCHBTN_HEIGHT = 30;

    private int _lastFrameTouchTapCount = 0;
    private const string CONSOLE_INPUTFIELD_NAME = "ConsoleInputField";
    private bool _ConsoleInputFieldFocusIsSet = false;

    private const int OKBUTTON_SIZE = 50;

    private int _consoleWidth;
    private int _consoleHeight;
    private int _preViewWidth = -1;
    private int _preViewHeight = -1;

    private Rect _windowRect;

    private string _instructionText = "";
    private Vector2 _outputScrollViewVector = Vector2.zero;
    private string _logText = "";
    private bool _isSwitchOn = false;
    private bool _isAutoScrollView = true;


    bool m_isForceHide = false;

    public bool IsForceHide
    {
        get { return m_isForceHide; }
        set
        {
            if (m_isForceHide == value)
                return;
            m_isForceHide = value;
            if (m_isForceHide)
                IsSwitchOn = false;
        }
    }

        public bool IsSwitchOn
    {
        set
        {
            if (IsForceHide && value)
                return;
            if (_isSwitchOn == value)
                return;
            _isSwitchOn = value;
            SaveData();
        }
        get
        {
            return _isSwitchOn;
        }
    }

    private bool IsAutoScrollView
    {
        set
        {
            if (_isAutoScrollView == value)
            {
                return;
            }

            _isAutoScrollView = value;
            Debug.Log(string.Format("Debug console AutoScroll: {0}", value));
        }
        get
        {
            return _isAutoScrollView;
        }
    }
    private static DebugConsoleView _instance;
    private float _leftAndRightButtonDownTime = 0;
    private float _fiveFingersDownTime = 0;
    private float _nextUpdateFPSTime = 0;
    private float _fps = 60;
    private double _allocatedMemorySize = 0;
    private double _maxAllocatedMemorySize = 0;
    private GUIStyle m_labelStyle = null;
    private GUIStyle m_textfieldStyle = null;
#if UNITY_IOS
    const float ScreenDpi2FontSize = 0.07f;
#elif UNITY_ANDROID
    const float ScreenDpi2FontSize = 0.060f;
#else
    const float ScreenDpi2FontSize = 0.14f;
#endif
    private DebugConsoleView()
    {
        _instance = this;
    }

    public static DebugConsoleView Create()
    {
        DebugConsoleView view = new DebugConsoleView();
        view._Init();
        return view;
    }

    private void _Init()
    {
        LoadData();
    }

    public void Show()
    {
        if (m_labelStyle == null)
        {
            m_labelStyle = new GUIStyle(GUI.skin.label);
            m_labelStyle.fontSize = (int)(Screen.dpi * ScreenDpi2FontSize);
        }
        if (m_textfieldStyle == null)
        {
            m_textfieldStyle = new GUIStyle(GUI.skin.textField);
            m_textfieldStyle.fontSize = (int)(Screen.dpi * ScreenDpi2FontSize);
        }

        int viewWidth = Screen.width;
        int viewHeight = Screen.height;

        _consoleWidth = viewWidth / 2 - CONSOLE_MARGIN_X * 4; // 2;
        _consoleHeight = viewHeight / 2; // - CONSOLE_MARGIN_Y * 2 - 60;

        if (_preViewWidth == -1 || _preViewWidth != viewWidth
           || _preViewHeight == -1 || _preViewHeight != viewHeight)
        {
            _windowRect = new Rect(CONSOLE_MARGIN_X, CONSOLE_MARGIN_Y, _consoleWidth, _consoleHeight);
            _preViewWidth = viewWidth;
            _preViewHeight = viewHeight;
        }

        _ShowConsole(CONSOLE_MARGIN_X, CONSOLE_MARGIN_Y, _consoleWidth, _consoleHeight);

#if !UNITY_ANDROID && !UNITY_IOS
        _ShowSwitchInput();
#else
		_ShowSwitchTouch();
#endif
    }

    void _ShowSwitchInput()
    {
        bool isLeftAndRightButtonDown = Input.GetMouseButton(0) && Input.GetMouseButton(1);

        if (isLeftAndRightButtonDown)
        {
            _leftAndRightButtonDownTime += Time.deltaTime;
        }
        else
        {
            _leftAndRightButtonDownTime = 0;
        }

        if (_leftAndRightButtonDownTime > 2 && _leftAndRightButtonDownTime - Time.deltaTime < 2)
        {   // 鼠标左键和右键同时被按下超过1秒
            IsSwitchOn = !IsSwitchOn;
        }

        LeftMouseButtonTapCount.Update();
        
        if (IsSwitchOn && LeftMouseButtonTapCount.Count == 3)
        {
            // 鼠标左键三击屏幕, 切换【自动滚屏】开关
            IsAutoScrollView = !IsAutoScrollView;
        }
    }

    private void _ShowSwitchTouch()
    {
        if (Input.touchCount == 5)
        {
            _fiveFingersDownTime += Time.deltaTime;
        }
        else
        {
            _fiveFingersDownTime = 0;
        }

        if (IsSwitchOn && Input.touchCount > 0
            && Input.touches[0].tapCount > _lastFrameTouchTapCount
            && Input.touches[0].tapCount == 3)
        {
            // 三击屏幕, 切换【自动滚屏】开关
            IsAutoScrollView = !IsAutoScrollView;
        }

        if (_fiveFingersDownTime > 2 && _fiveFingersDownTime - Time.deltaTime < 2)
        {   // 鼠标左键和右键同时被按下超过1秒
            IsSwitchOn = !IsSwitchOn;
            
            if(!IsSwitchOn)
                DebugConsoleMode.instance.ProcessCmd("Save");
            
        }

        if (Input.touchCount > 0)
        {
            _lastFrameTouchTapCount = Input.touches[0].tapCount;
        }
        if (IsSwitchOn)
        {
            TwoFingersMoveEvent.Update();
            if (TwoFingersMoveEvent.Up)
            {
                ShowPreviousCommand();
            }
            else if (TwoFingersMoveEvent.Down)
            {
                ShowNextCommand();
            }

        }
    }

    private void _ShowConsole(int x, int y, int width, int height)
    {

        //windowRect = new Rect (x, y, width, height);
        if (IsSwitchOn)
        {
            _windowRect = GUILayout.Window(0, _windowRect, _WindowFunction, "Console Window");
        }
        else
        {
            _ConsoleInputFieldFocusIsSet = false;
        }
    }

    private Rect _ComputeSwitchBtnRect(Int32 viewWidth, Int32 viewHeight)
    {
        int btnX = SWITCHBTN_MARGIN_X;
        //int btnY = viewHeight - SWITCHBTN_MARGIN_Y - SWITCHBTN_HEIGHT;
        int btnY = viewHeight / 2 + SWITCHBTN_MARGIN_Y;
        return new Rect(btnX, btnY, SWITCHBTN_WIDTH, SWITCHBTN_HEIGHT);
    }

    private string _GetSwitchButtonText()
    {
        string text = "";
        if (IsSwitchOn == false)
        {
            text = "O";
        }
        else if (IsSwitchOn == true)
        {
            text = "|";
        }
        return text;
    }

    private void _ProcessKeyboard()
    {
        if (Event.current != null
            && Event.current.isKey)
        {
            if (Event.current.rawType == EventType.KeyDown)
            {
                KeyDownEventArgs args = new KeyDownEventArgs(Event.current.keyCode);
                if (OnKeyDown != null)
                    OnKeyDown(this, args);
            }
            else if (Event.current.rawType == EventType.KeyUp)
            {
                KeyUpEventArgs args = new KeyUpEventArgs(Event.current.keyCode);
                if (OnKeyUp != null)
                    OnKeyUp(this, args);
            }
        }
    }

    private void _WindowFunction(int id)
    {
        _DrawMainConsoleCtrl();
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
    }

    private void _DrawMainConsoleCtrl()
    {
#if !UNITY_ANDROID && !UNITY_IOS
        _ProcessKeyboard();
#endif
        GUILayout.BeginVertical();
        {
            _DrawInputBox_And_OKButton();
            _DrawOutputArea();
        }
        GUILayout.EndVertical();
    }

    private void _DrawInputBox_And_OKButton()
    {
        GUILayout.BeginHorizontal();
        {
            _DrawCloseButton();
            _DrawInputBox();
            _DrawOKButton();
        }
        GUILayout.EndHorizontal();
        if (!_ConsoleInputFieldFocusIsSet)
        {
            GUI.FocusControl(CONSOLE_INPUTFIELD_NAME);
            _ConsoleInputFieldFocusIsSet = true;
        }
    }

    private void _DrawInputBox()
    {
        int inputBoxWidth = _consoleWidth - OKBUTTON_SIZE;
        GUI.SetNextControlName(CONSOLE_INPUTFIELD_NAME);
        _instructionText = GUILayout.TextField(_instructionText, m_textfieldStyle, GUILayout.Width(inputBoxWidth), GUILayout.Height(Math.Max(22, Screen.dpi*0.15f)));
    }

    private void _DrawOKButton()
    {
        if (GUILayout.Button("OK", GUILayout.Width(Math.Max(32, Screen.dpi*0.2f)), GUILayout.Height(Math.Max(22, Screen.dpi*0.15f))))
        {
            if (OnOKButtonClick != null)
                OnOKButtonClick();
        }
    }

    private void _DrawCloseButton()
    {
        if (GUILayout.Button("X", GUILayout.Width(Math.Max(22, Screen.dpi * 0.15f)), GUILayout.Height(Math.Max(22, Screen.dpi * 0.15f))))
        {
            if (OnCloseButtonClick != null)
                OnCloseButtonClick();
        }
    }

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern long Report_memory();
#endif


    public static long GetRuntimeMemorySize()
    {
#if UNITY_IOS
            return Report_memory();
#endif
        return Profiler.GetTotalAllocatedMemoryLong();
    }

    private void _DrawOutputArea()
    {
        if (IsAutoScrollView)
        {
            _outputScrollViewVector.y = float.MaxValue;
        }

        GUI.skin.verticalScrollbar.fixedWidth = Screen.dpi * 0.1f;
        GUI.skin.verticalScrollbarThumb.fixedWidth = GUI.skin.verticalScrollbar.fixedWidth;
        _outputScrollViewVector = GUILayout.BeginScrollView(_outputScrollViewVector, false, true);
        GUILayout.Label(_logText, m_labelStyle);
        GUILayout.EndScrollView();

        GUILayout.Label(string.Format("FPS: {0:D3}    MEM: {1:D4} MB    MAX_MEM: {2:D4} MB",
            (int)_fps, (int)_allocatedMemorySize, (int)_maxAllocatedMemorySize), m_labelStyle);

        if (Time.time > _nextUpdateFPSTime)
        {
            _fps = 1 / Time.deltaTime;
            _allocatedMemorySize = GetRuntimeMemorySize() * 0.0000009536743164;
            _maxAllocatedMemorySize = Math.Max(_maxAllocatedMemorySize, _allocatedMemorySize);
            _nextUpdateFPSTime = Time.time + 1;
        }
    }

    public string LogText
    {
        set
        {
            //_logText = string.Format("<size={0}>{1}</size>", _fontSize, value);
            _logText = value;
        }
    }

    public string InstructionText
    {
        get { return _instructionText; }
        set { _instructionText = value; }
    }

    public void SetFontSize(int size)
    {
        if (m_labelStyle == null)
            return;
        m_labelStyle.fontSize = size;
    }

    public static DebugConsoleView instance
    {
        get
        {
            Assert.IsNotNull(_instance, "_instance is null");
            return _instance;
        }
    }

    public void ShowNextCommand()
    {
        _instructionText = DebugCmdManager.instance.GetNextOldCommand();
    }

    public void ShowPreviousCommand()
    {
        _instructionText = DebugCmdManager.instance.GetPreviousOldCommand();
    }

    void SaveData()
    {
        try
        {
            string filename = string.Format("{0}{1}DebugConsoleView.txt",
                                     Application.persistentDataPath, Path.AltDirectorySeparatorChar);
            StreamWriter sw = new StreamWriter(filename);

            Assert.IsNotNull(sw, "Failed to create StreamWriter.");
            sw.WriteLine(IsSwitchOn);
            sw.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("DebugConsoleView.SaveData exception={0}", e.Message));
        }
    }

    void LoadData()
    {
        string filename = string.Format("{0}{1}DebugConsoleView.txt",
                                     Application.persistentDataPath, Path.AltDirectorySeparatorChar);
        try
        {
            if (!File.Exists(filename))
                return;
            StreamReader sr = new StreamReader(filename);
            IsSwitchOn = bool.Parse(sr.ReadLine());
            sr.Close();
        }
        catch (FileNotFoundException)
        {
            Debug.LogError(string.Format("DebugConsoleView.LoadData [{0}] not found.", filename));
        }
        catch (FileLoadException)
        {
            Debug.LogError(string.Format("DebugConsoleView.LoadDataFailed to load [{0}].", filename));
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("DebugConsoleView.LoadData exception={0}", e.Message));
        }
    }
}
