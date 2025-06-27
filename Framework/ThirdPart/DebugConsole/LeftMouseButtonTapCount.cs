using UnityEngine;
using System.Collections;

public class LeftMouseButtonTapCount
{
    private static int _leftMouseButtonDownCount = 0;
    private static float _leftMouseButtonDownTime = 0;
    private static int _lastFrameLeftMouseButtonDownCount = 0;
    private static float _tabInterval = 0.35f;

    public static float TabInterval
    {
        set
        {
            _tabInterval = value;
        }
    }

    public static int Count
    {
        get
        {
            return _leftMouseButtonDownCount > _lastFrameLeftMouseButtonDownCount ? _leftMouseButtonDownCount : 0;
        }
    }

    public static void Update()
    {
        if (Time.time - _leftMouseButtonDownTime > _tabInterval)
        {
            _leftMouseButtonDownCount = 0;
        }

        _lastFrameLeftMouseButtonDownCount = _leftMouseButtonDownCount;
        if (Time.time > _leftMouseButtonDownTime && Input.GetMouseButtonDown(0))
        {
            _leftMouseButtonDownCount++;
            _leftMouseButtonDownTime = Time.time;
        }

    }
}
