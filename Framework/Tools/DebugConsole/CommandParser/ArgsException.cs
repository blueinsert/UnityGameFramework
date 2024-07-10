using System;

public class ArgsException : Exception
{
    public enum ErrorCode
    {
        OK,
        INVALID_ARGUMENT_FORMAT,
        UNEXPECTED_ARGUMENT,
        INVALID_ARGUMENT_NAME,
        INVALID_ARGUMENTS_NUMBER,
        INVALID_BOOLEAN,
        MISSING_BOOLEAN,
        OVERFLOW_INTEGER32,
        INVALID_INTEGER32,
        MISSING_INTEGER32,
        OVERFLOW_INTEGER64,
        INVALID_INTEGER64,
        MISSING_INTEGER64,
        OVERFLOW_FLOAT,
        INVALID_FLOAT,
        MISSING_FLOAT,
        OVERFLOW_DOUBLE,
        INVALID_DOUBLE,
        MISSING_DOUBLE
    }

    private string _errorArgumentId = "\0";
    private string _errorParameter = null;
    private ErrorCode _errorCode = ErrorCode.OK;

    public string ErrorArgumentId
    {
        get { return _errorArgumentId; }
        set { _errorArgumentId = value; }
    }

    public string ErrorParameter
    {
        get { return _errorParameter; }
        set { _errorParameter = value; }
    }

    public ArgsException() { }

    public ArgsException(string message) : base(message) { }

    public ArgsException(ErrorCode errorCode)
    {
        _errorCode = errorCode;
    }

    public ArgsException(ErrorCode errorCode, string errorParameter)
    {
        _errorCode = errorCode;
        _errorParameter = errorParameter;
    }

    public ArgsException(ErrorCode errorCode, string errorArgumentId, string errorParameter)
    {
        _errorCode = errorCode;
        _errorArgumentId = errorArgumentId;
        _errorParameter = errorParameter;
    }

    public ErrorCode GetErrorCode()
    {
        return _errorCode;
    }

    public void SetErrorCode(ErrorCode errorCode)
    {
        _errorCode = errorCode;
    }

    public string ErrorMessage()
    {
        switch (_errorCode)
        {
            case ErrorCode.OK:
                return "TILT: Should not get here";
            case ErrorCode.UNEXPECTED_ARGUMENT:
                return string.Format("Argument -{0} unexpected.", _errorArgumentId);
            case ErrorCode.INVALID_BOOLEAN:
                return string.Format("Argument -{0} expects an boolean but was '{1}'.", _errorArgumentId, _errorParameter);
            case ErrorCode.MISSING_BOOLEAN:
                return string.Format("Could not find boolean parameter for paramIndex-{0}.", _errorArgumentId);
            case ErrorCode.INVALID_INTEGER32:
                return string.Format("Argument -{0} expects an integer32 but was '{1}'.", _errorArgumentId, _errorParameter);
            case ErrorCode.OVERFLOW_INTEGER32:
                return string.Format("integer32 parameter -{0} is overflow.", _errorArgumentId);
            case ErrorCode.MISSING_INTEGER32:
                return string.Format("Could not find integer32 parameter for paramIndex-{0}", _errorArgumentId);
            case ErrorCode.INVALID_INTEGER64:
                return string.Format("Argument -{0} expects an integer64 but was '{1}'.", _errorArgumentId, _errorParameter);
            case ErrorCode.OVERFLOW_INTEGER64:
                return string.Format("integer64 parameter -{0} is overflow.", _errorArgumentId);
            case ErrorCode.MISSING_INTEGER64:
                return string.Format("Could not find integer64 parameter for paramIndex-{0}", _errorArgumentId);
            case ErrorCode.INVALID_FLOAT:
                return string.Format("Argument -{0} expects a float but was '{1}'", _errorArgumentId, _errorParameter);
            case ErrorCode.OVERFLOW_FLOAT:
                return string.Format("float parameter -{0} is overflow.", _errorArgumentId);
            case ErrorCode.MISSING_FLOAT:
                return string.Format("Could not find float parameter for paramIndex-{0}", _errorArgumentId);
            case ErrorCode.INVALID_DOUBLE:
                return string.Format("Argument -{0} expects a double but was '{1}'", _errorArgumentId, _errorParameter);
            case ErrorCode.OVERFLOW_DOUBLE:
                return string.Format("double parameter -{0} is overflow.", _errorArgumentId);
            case ErrorCode.MISSING_DOUBLE:
                return string.Format("Could not find double parameter for paramIndex-{0}", _errorArgumentId);
            case ErrorCode.INVALID_ARGUMENT_NAME:
                return string.Format("'{0}' is not a valid argument name.", _errorArgumentId);
            case ErrorCode.INVALID_ARGUMENT_FORMAT:
                return string.Format("'{0}' is not a valid argument format.", _errorParameter);
            case ErrorCode.INVALID_ARGUMENTS_NUMBER:
                return string.Format("Invalid Arguments Number.");
        }
        return "";
    }
}