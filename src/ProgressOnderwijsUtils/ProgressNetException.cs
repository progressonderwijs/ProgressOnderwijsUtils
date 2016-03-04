using System;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public class ProgressNetException : Exception
    { //TODO: this exception type might provide for TextDef integration such that throwing code can indicate the text-based error to show users.
        public ProgressNetException() { }

        public ProgressNetException(string debugMessage, Exception innerException = null)
            : base(debugMessage, innerException) { }

        protected ProgressNetException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// This exception is thrown when an impossible program state is encountered - i.e. when a programmer made a mistake and violated a assumption.
    /// </summary>
    [Serializable]
    [CodeThatsOnlyUsedForTests]
    public class PNAssertException : ProgressNetException
    {
        public PNAssertException() { }

        public PNAssertException(string debugMessage, Exception innerException = null)
            : base(debugMessage, innerException) { }

        protected PNAssertException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
