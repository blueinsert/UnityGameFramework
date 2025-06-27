using UnityEngine;
using System.Collections;

public class TwoFingersMoveEvent
{
    static Vector2 _lastTouchPos = Vector2.zero;
    static Vector2 _curTouchPos = Vector2.zero;
    static Vector2 _touchDownPos = Vector2.zero;
    static int _lastTouchCount = 0;
    static bool _isUp = false, _isDown = false, _isLeft = false, _isRight = false;
    static float _createEventDist = 150;
    static float _lastUpdateTime = 0;

    public static bool Up
    {
        get {
            return _isUp;
        }
    }

    public static bool Down
    {
        get
        {
            return _isDown;
        }
    }

    public static bool Left
    {
        get
        {
            return _isLeft;
        }
    }

    public static bool Right
    {
        get
        {
            return _isRight;
        }
    }
    public static void Update()
    {
        if (Input.touchCount != 2)
        {
            Reset();
        }
        else
        {
            if (_lastTouchCount != 2)
            {
                _touchDownPos = Input.touches[0].position;
            }
            _lastTouchPos = _curTouchPos;
            _curTouchPos = Input.touches[0].position;

            _isUp = Time.time != _lastUpdateTime &&
                    _touchDownPos.y != 0 &&
                    _curTouchPos.y - _touchDownPos.y > _createEventDist &&
                    _lastTouchPos.y - _touchDownPos.y < _createEventDist;

            _isDown = Time.time != _lastUpdateTime && 
                    _touchDownPos.y != 0 &&
                    _touchDownPos.y - _curTouchPos.y > _createEventDist &&
                    _touchDownPos.y - _lastTouchPos.y < _createEventDist;

            if (_isUp || _isDown)
            {
                _touchDownPos.y = _curTouchPos.y;
            }

            _isRight = Time.time != _lastUpdateTime &&
                   _touchDownPos.x != 0 &&
                   _curTouchPos.x - _touchDownPos.x > _createEventDist &&
                   _lastTouchPos.x - _touchDownPos.x < _createEventDist;

            _isLeft = Time.time != _lastUpdateTime &&
                    _touchDownPos.x != 0 &&
                    _touchDownPos.x - _curTouchPos.x > _createEventDist &&
                    _touchDownPos.x - _lastTouchPos.x < _createEventDist;

            if (_isRight || _isLeft)
            {
                _touchDownPos.x = _curTouchPos.x;
            }
        }
        _lastTouchCount = Input.touchCount;
        _lastUpdateTime = Time.time;
    }

    static void Reset()
    {
        _lastTouchPos = Vector2.zero;
        _curTouchPos = Vector2.zero;
        _touchDownPos = Vector2.zero;
        _lastTouchCount = 0;
        _isUp = _isDown = _isLeft = _isRight = false;
    }
}
