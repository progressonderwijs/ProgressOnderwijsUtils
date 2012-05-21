using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Text
{
	public sealed class ConcatTranslatable : ITranslatable
	{
		readonly ITranslatable[] parts;
		//public ITranslatable[] Parts { get { return parts; } }

		public ConcatTranslatable(params ITranslatable[] parts) { this.parts = parts;}
		public string GenerateUid() { return parts.Select(it=> it.GenerateUid()).JoinStrings(); }

		public TextVal Translate(ITranslationKeyLookup conn, Taal taal)
		{
			var translation = parts.Select(it => it.Translate(conn, taal)).ToArray();
			return new TextVal(translation.Select(tv => tv.Text).JoinStrings(), translation.Select(tv => tv.ExtraTextOrDefault).JoinStrings());
		}
	}
}
