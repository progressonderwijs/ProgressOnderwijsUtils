using System;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public sealed class ConcatTranslatable : ITranslatable
    {
        readonly ITranslatable[] parts;

        public ConcatTranslatable(params ITranslatable[] parts)
        {
            this.parts = parts;
            foreach (ITranslatable p in parts) {
                if (p == null) { //Perf: no LINQ
                    throw new ArgumentNullException(nameof(parts), "element of parts is null");
                }
            }
        }

        public string GenerateUid() => parts.Select(it => it.GenerateUid()).JoinStrings();

        public TextVal Translate(Taal taal)
        {
            var translation = parts.Select(it => it.Translate(taal)).ToArray();
            return new TextVal(translation.Select(tv => tv.Text).JoinStrings(), translation.Select(tv => tv.ExtraText ?? "").JoinStrings());
        }
    }
}
