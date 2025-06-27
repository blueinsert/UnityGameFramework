using System.Collections.Generic;
using System;
// new ArgumentsParser("f i l", args);
public class ArgumentsParser
{

    private Dictionary<string, IArgumentMarshaler> _marshalers;
    private Int32 _schemaElementNum;

    private const char ARG_SEPARATOR = ' ';

    public Int32 SchemaElementNum
    {
        get { return _schemaElementNum; }
    }

    public ArgumentsParser(string schema, string args)
    {
        _marshalers = new Dictionary<string, IArgumentMarshaler>();

        string[] argsArray = args.Split(ARG_SEPARATOR);
        string[] schemaElementArray = schema.Split(ARG_SEPARATOR);
        if (!_ValidArgumentsNumber(schemaElementArray, argsArray))
            throw new ArgsException(ArgsException.ErrorCode.INVALID_ARGUMENTS_NUMBER);

        _schemaElementNum = schemaElementArray.Length;
        _ParseSchema(schemaElementArray);
        _ParseArguments(argsArray);
    }

    private bool _ValidArgumentsNumber(string[] schemaElementArray, string[] argsArray)
    {
        if (argsArray.Length > schemaElementArray.Length
            || (schemaElementArray.Length == 1 && string.IsNullOrEmpty(schemaElementArray[0])))
        {
            return false;
        }
        return true;
    }

    private void _ParseSchema(string[] schemaElementArray)
    {
        for (int i = 0; i < schemaElementArray.Length; i++)
        {
            _ParseSchemaElement(i, schemaElementArray[i].Trim());
        }
    }

    private void _ParseSchemaElement(int index, string element)
    {
        string elementId = index.ToString();
        if (string.Equals(element, "b"))
        {
            _marshalers.Add(elementId, new BooleanArgumentMarshaler());
        }
        else if (string.Equals(element, "i"))
        {
            _marshalers.Add(elementId, new Int32ArgumentMarshaler());
        }
        else if (string.Equals(element, "l"))
        {
            _marshalers.Add(elementId, new Int64ArgumentMarshaler());
        }
        else if (string.Equals(element, "f"))
        {
            _marshalers.Add(elementId, new FloatArgumentMarshaler());
        }
        else if (string.Equals(element, "d"))
        {
            _marshalers.Add(elementId, new DoubleArgumentMarshaler());
        }
        else if (string.Equals(element, "s"))
        {
            _marshalers.Add(elementId, new StringArgumentMarshaler());
        }
        else
            throw new ArgsException(ArgsException.ErrorCode.INVALID_ARGUMENT_FORMAT, element);

    }

    private void _ParseArguments(string[] argsArray)
    {
        for (int i = 0; i < argsArray.Length; i++)
        {
            IArgumentMarshaler m = _marshalers[i.ToString()];
            if (m == null)
            {
                throw new ArgsException(ArgsException.ErrorCode.UNEXPECTED_ARGUMENT, i.ToString(), null);
            }
            else
            {
                try
                {
                    m.Set(argsArray[i]);
                }
                catch (ArgsException e)
                {
                    e.ErrorArgumentId = i.ToString();
                    throw e;
                }
            }
        }
    }

    public bool GetBoolean(int index)
    {
        return BooleanArgumentMarshaler.GetValue(_marshalers[index.ToString()]);
    }

    public Int32 GetInt32(int index)
    {
        return Int32ArgumentMarshaler.GetValue(_marshalers[index.ToString()]);
    }

    public Int64 GetInt64(int index)
    {
        return Int64ArgumentMarshaler.GetValue(_marshalers[index.ToString()]);
    }

    public float GetFloat(int index)
    {
        return FloatArgumentMarshaler.GetValue(_marshalers[index.ToString()]);
    }

    public double GetDouble(int index)
    {
        return DoubleArgumentMarshaler.GetValue(_marshalers[index.ToString()]);
    }

    public string GetString(int index)
    {
        return StringArgumentMarshaler.GetValue(_marshalers[index.ToString()]);
    }
}
