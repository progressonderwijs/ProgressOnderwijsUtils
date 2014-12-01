﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public class NoRowsFoundException : ProgressNetException
    {
        public NoRowsFoundException(string msg)
            : base(msg) { }

        public NoRowsFoundException() { }

        public NoRowsFoundException(string msg, Exception inner)
            : base(msg, inner) { }

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
        public QueryException(string msg)
            : base(msg) { }

        public QueryException() { }

        public QueryException(string msg, Exception inner)
            : base(msg, inner) { }

        protected QueryException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }

    [Serializable]
    public class TemplateException : Exception
    {
        public int Line { get; private set; }
        public int Position { get; private set; }

        public TemplateException(int _Line, int _Position, string message)
            : base(message)
        {
            Line = _Line;
            Position = _Position;
        }

        public TemplateException() { }

        public TemplateException(string message)
            : base(message) { }

        public TemplateException(string message, Exception innerexception)
            : base(message, innerexception) { }

        protected TemplateException(SerializationInfo serializationinfo, StreamingContext streamingcontext)
            : base(serializationinfo, streamingcontext) { }
    }

    [Serializable]
    public class GenericMetaDataException : ProgressNetException
    { //TODO: this exception should provide for naming the table and the type of metadata where the error occured.

        public GenericMetaDataException(string debugMessage)
            : base(debugMessage) { }

        public GenericMetaDataException(string debugMessage, Exception innerException)
            : base(debugMessage, innerException) { }

        protected GenericMetaDataException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
