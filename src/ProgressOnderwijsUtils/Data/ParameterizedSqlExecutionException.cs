using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public class ParameterizedSqlExecutionException : Exception
    {
        public ParameterizedSqlExecutionException(string msg)
            : base(msg) { }

        public ParameterizedSqlExecutionException() { }

        public ParameterizedSqlExecutionException(string msg, Exception inner)
            : base(msg, inner) { }

        protected ParameterizedSqlExecutionException([NotNull] SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }
}
