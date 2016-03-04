using System;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public class NoRowsFoundException : ProgressNetException
    {
        public NoRowsFoundException() { }

        protected NoRowsFoundException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }

    [Serializable]
    public class ParameterizedSqlExecutionException : ProgressNetException
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

    [Serializable]
    public class GenericMetaDataException : ProgressNetException
    { //TODO: this exception should provide for naming the table and the type of metadata where the error occured.

        public GenericMetaDataException(string debugMessage)
            : base(debugMessage) { }

        [CodeThatsOnlyUsedForTests]
        public GenericMetaDataException(string debugMessage, Exception innerException)
            : base(debugMessage, innerException) { }

        protected GenericMetaDataException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
