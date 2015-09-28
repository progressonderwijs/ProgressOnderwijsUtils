using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public class NietZoErgeException : ProgressNetException
    {
        public NietZoErgeException(string message)
            : this(message, null) { }

        public NietZoErgeException(string message, Exception inner)
            : this(Translatable.Raw(message + (inner != null ? " (" + inner.Message + ")" : "")), inner) { }

        public NietZoErgeException(ITranslatable melding)
            : this(melding, null) { }

        public NietZoErgeException(ITranslatable melding, Exception inner)
            : base(melding.Translate(Taal.NL).Text, inner)
        {
            Melding = melding;
        }

        public ITranslatable Melding { get; }
    }
}
