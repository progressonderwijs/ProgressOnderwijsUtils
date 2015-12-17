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
    public class GeenRechtException : ProgressNetException
    {
        public GeenRechtException(string msg)
            : base(msg) { }

        protected GeenRechtException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }

    [Serializable]
    public class QueryException : ProgressNetException
    {
        [CodeDieAlleenWordtGebruiktInTests]
        public QueryException(string msg)
            : base(msg) { }

        [CodeDieAlleenWordtGebruiktInTests]
        public QueryException() { }

        public QueryException(string msg, Exception inner)
            : base(msg, inner) { }

        protected QueryException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }

    [Serializable]
    public class GenericMetaDataException : ProgressNetException
    { //TODO: this exception should provide for naming the table and the type of metadata where the error occured.

        public GenericMetaDataException(string debugMessage)
            : base(debugMessage) { }

        [CodeDieAlleenWordtGebruiktInTests]
        public GenericMetaDataException(string debugMessage, Exception innerException)
            : base(debugMessage, innerException) { }

        protected GenericMetaDataException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
