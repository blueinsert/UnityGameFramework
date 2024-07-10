using UnityEngine;
using System;

public class KeyDownEventArgs : EventArgs
{

    private readonly KeyCode _keyCode;

    public KeyDownEventArgs(KeyCode keyCode)
    {
        _keyCode = keyCode;
    }

    public KeyCode KeyCode { get { return _keyCode; } }
}

public class KeyUpEventArgs : EventArgs
{

    private readonly KeyCode _keyCode;

    public KeyUpEventArgs(KeyCode keyCode)
    {
        _keyCode = keyCode;
    }

    public KeyCode KeyCode { get { return _keyCode; } }
}