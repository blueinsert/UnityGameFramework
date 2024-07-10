using System.Collections.Generic;
using System;

public interface IArgumentMarshaler
{
    void Set(string currentArgument);
}

public class BooleanArgumentMarshaler : IArgumentMarshaler
{
    private bool _booleanValue = false;

    public void Set(string currentArgument)
    {
        if (string.IsNullOrEmpty(currentArgument))
            throw new ArgsException(ArgsException.ErrorCode.MISSING_BOOLEAN);
        try
        {
            Int32 intVal = Int32.Parse(currentArgument);
            _booleanValue = Convert.ToBoolean(intVal);
        }
        catch (FormatException e)
        {
            throw new ArgsException(e.Message + " | " + currentArgument);
        }
    }

    public static bool GetValue(IArgumentMarshaler am)
    {
        if (am != null && am is BooleanArgumentMarshaler)
        {
            return ((BooleanArgumentMarshaler)am)._booleanValue;
        }
        else
        {
            return false;
        }
    }
}

public class StringArgumentMarshaler : IArgumentMarshaler
{
    private string _stringValue = "";

    public void Set(string currentArgument)
    {
        _stringValue = currentArgument;
    }

    public static string GetValue(IArgumentMarshaler am)
    {
        if (am != null && am is StringArgumentMarshaler)
        {
            return ((StringArgumentMarshaler)am)._stringValue;
        }
        else
        {
            return "";
        }

    }
}

/// <summary>
/// throw ArgsException(ArgsException.ErrorCode.MISSING_INTEGER)
/// throw ArgsException(ArgsException.ErrorCode.INVALID_INTEGER, parameter)
/// </summary>
public class Int32ArgumentMarshaler : IArgumentMarshaler
{
    private Int32 _int32Value = 0;

    public void Set(string currentArgument)
    {
        if (string.IsNullOrEmpty(currentArgument))
            throw new ArgsException(ArgsException.ErrorCode.MISSING_INTEGER32);
        try
        {
            _int32Value = Int32.Parse(currentArgument);
        }
        catch (OverflowException e)
        {
            throw new ArgsException(e.Message);
        }
        catch (FormatException e)
        {
            throw new ArgsException(e.Message + " | " + currentArgument);
        }
    }

    public static Int32 GetValue(IArgumentMarshaler am)
    {
        if (am != null && am is Int32ArgumentMarshaler)
        {
            return ((Int32ArgumentMarshaler)am)._int32Value;
        }
        else
        {
            return 0;
        }
    }
}

public class Int64ArgumentMarshaler : IArgumentMarshaler
{
    private Int64 _int64Value = 0;

    public void Set(string currentArgument)
    {
        if (string.IsNullOrEmpty(currentArgument))
            throw new ArgsException(ArgsException.ErrorCode.MISSING_INTEGER64);
        try
        {
            _int64Value = Int64.Parse(currentArgument);
        }
        catch (OverflowException e)
        {
            throw new ArgsException(e.Message);
        }
        catch (FormatException e)
        {
            throw new ArgsException(e.Message + " | " + currentArgument);
        }
    }

    public static Int64 GetValue(IArgumentMarshaler am)
    {
        if (am != null && am is Int64ArgumentMarshaler)
        {
            return ((Int64ArgumentMarshaler)am)._int64Value;
        }
        else
        {
            return 0;
        }
    }
}

public class FloatArgumentMarshaler : IArgumentMarshaler
{
    private float _floatValue = 0.0f;

    public void Set(string currentArgument)
    {
        if (string.IsNullOrEmpty(currentArgument))
            throw new ArgsException(ArgsException.ErrorCode.MISSING_FLOAT);

        try
        {
            _floatValue = float.Parse(currentArgument);
        }
        catch (OverflowException e)
        {
            throw new ArgsException(e.Message);
        }
        catch (FormatException e)
        {
            throw new ArgsException(e.Message + " | " + currentArgument);
        }
    }

    public static float GetValue(IArgumentMarshaler am)
    {
        if (am != null && am is FloatArgumentMarshaler)
        {
            return ((FloatArgumentMarshaler)am)._floatValue;
        }
        else
        {
            return 0.0f;
        }
    }
}

public class DoubleArgumentMarshaler : IArgumentMarshaler
{
    private double _doubleValue = 0.0f;

    public void Set(string currentArgument)
    {
        if (string.IsNullOrEmpty(currentArgument))
            throw new ArgsException(ArgsException.ErrorCode.MISSING_DOUBLE);
        try
        {
            _doubleValue = double.Parse(currentArgument);
        }
        catch (OverflowException e)
        {
            throw new ArgsException(e.Message);
        }
        catch (FormatException e)
        {
            throw new ArgsException(e.Message + " | " + currentArgument);
        }
    }

    public static double GetValue(IArgumentMarshaler am)
    {
        if (am != null && am is DoubleArgumentMarshaler)
        {
            return ((DoubleArgumentMarshaler)am)._doubleValue;
        }
        else
        {
            return 0.0d;
        }
    }
}