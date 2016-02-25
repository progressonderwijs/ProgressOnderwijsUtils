using System;

namespace ProgressOnderwijsUtils
{
    sealed class TaalFormatter : IFormatProvider, ICustomFormatter
    {
        readonly Taal taal;

        TaalFormatter(Taal taal)
        {
            this.taal = taal;
        }

        public object GetFormat(Type formatType)
            => this;

        public string Format(string format, object arg, IFormatProvider formatProvider)
            => Converteer.ToString(arg, format, taal);

        public static readonly TaalFormatter NL = new TaalFormatter(Taal.NL);
        public static readonly TaalFormatter EN = new TaalFormatter(Taal.EN);
        public static readonly TaalFormatter DU = new TaalFormatter(Taal.DU);
    }
}
