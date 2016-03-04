using System;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public class ParameterizedSqlExecutionException : Exception
    {
        [CodeThatsOnlyUsedForTests]
        public ParameterizedSqlExecutionException(string msg)
            : base(msg) { }

        [CodeThatsOnlyUsedForTests]
        public ParameterizedSqlExecutionException() { }

        public ParameterizedSqlExecutionException(string msg, Exception inner)
            : base(msg, inner) { }

        protected ParameterizedSqlExecutionException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }
}
