using System;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils;

[Serializable]
public sealed class ParameterizedSqlExecutionException : Exception
{
    public ParameterizedSqlExecutionException(string msg)
        : base(msg) { }

    public ParameterizedSqlExecutionException() { }

    public ParameterizedSqlExecutionException(string msg, Exception? inner)
        : base(msg, inner) { }

    ParameterizedSqlExecutionException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
        : base(serializationinfo, streamingcontext) { }
}