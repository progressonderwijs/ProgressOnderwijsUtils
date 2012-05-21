using System;

namespace ProgressOnderwijsUtils
{
	public sealed class TextDefSimple : ITranslatable
	{
		static readonly TextDefSimple emptyText = new TextDefSimple(TextVal.EmptyText);
		readonly TextVal tv;

		public static TextDefSimple EmptyText { get { return emptyText; } }

		public TextDefSimple(string text, string extratext) { tv = new TextVal(text, extratext); }
		//TODO: make new constructor with default extratext
		//Should default be null or empty string?


		public TextDefSimple(TextVal tv) { this.tv = tv; }

		public string GenerateUid() { return "TV:" + tv.Text + "\n" + tv.ExtraText; }
		public override string ToString() { return GenerateUid(); }

		public TextVal Translate(ITranslationKeyLookup conn, Taal taal) { return tv; }
	}
}
