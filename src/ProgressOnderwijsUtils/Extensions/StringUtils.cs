using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils.Extensions
{
	public static class StringUtils
	{
		/// <summary>
		/// Removes all 'diakriet' from the string.
		/// </summary>
		/// <param name="input">the string to change</param>
		/// <returns>the changed string</returns>
		public static string VerwijderDiakrieten(string input)
		{
			return
				new string(
					input
						.Normalize(NormalizationForm.FormD)
						.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
						.ToArray()
					).Normalize(NormalizationForm.FormC);
		}

		static readonly Regex capLetter = new Regex(@"(?<=[a-z])[A-Z0-9]|(?<=[A-Z])[A-Z0-9](?=[a-rt-z]|s[a-z])", RegexOptions.Compiled);
		public static string PrettyPrintCamelCased(string rawString) { return capLetter.Replace(rawString, m => " " + m.Value); }

		public static string VervangRingelS(string str, bool upper)
		{
			return str.Replace("ß", upper ? "SS" : "ss");
		}

		static readonly Regex COLLAPSE_WHITESPACE = new Regex(@"\s+", RegexOptions.Compiled);

		/// <summary>
		/// HTML-alike whitespace collapsing of this string; however, this method also trims.
		/// </summary>
		public static string NormalizeWhitespace(this string str)
		{
			return COLLAPSE_WHITESPACE.Replace(str, " ").Trim();
		}
		//modified from:http://www.merriampark.com/ldcsharp.htm by Eamon Nerbonne

		public static int LevenshteinDistance(string s, string t, int? substitutionCost = null)
		{
			int subsCost = substitutionCost ?? 1;
			int n = s.Length; //length of s
			int m = t.Length; //length of t
			int[,] d = new int[n + 1, m + 1]; // matrix
			if (n == 0) return m;
			if (m == 0) return n;
			for (int i = 0; i <= n; i++)
				d[i, 0] = i;
			for (int j = 0; j <= m; j++)
				d[0, j] = j;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
				{
					int cost = (t[j] == s[i] ? 0 : subsCost); // cost
					d[i + 1, j + 1] = Math.Min(Math.Min(d[i, j + 1] + 1, d[i + 1, j] + 1), d[i, j] + cost);
				}
			return d[n, m];
		}

		public static double LevenshteinDistanceScaled(string s, string t)
		{
			return LevenshteinDistance(s, t) / (double)Math.Max(1, Math.Max(s.Length, t.Length));
		}

		/// <summary>
		/// Vervang in een [naam]string beginletters door hoofdletters,
		/// rekening houdend met tussenvoegsels en interpunctie
		/// </summary>
		/// <remarks>
		/// tussenvoegsels zouden ook uit database kunnen worden gehaald:
		/// [SELECT voorvoegsels FROM student group by voorvoegsels]
		/// </remarks>
		/// <param name="inp"></param>
		/// <returns></returns>
		public static string Name2UpperCasedName(string inp)
		{
			//string wat opschonen
			inp = Regex.Replace(inp, @"\s+", " ");
			inp = Regex.Replace(inp, @"\-+", "-");
			inp = Regex.Replace(inp, @"('s)([a-zA-Z]+)", "$1 $2"); //'sgravenhage bv
			inp = Regex.Replace(inp, @"^\-+|\-+$", "").Trim();
			const string expression = @"d'|o'
										| 's | 's-|'s| op 't | op ten | op de
										| van het | van der | van de | van den | van ter
										| auf dem | auf der | von der | von den
										| in het | in 't | in de
										| uit de | uit den | uit het 
										| voor de | voor 't 
										| aan het | aan 't | aan de | aan den | bij de | de la 
										| del | van | von | het | de 
										| der | den | des | di | dos | do | du | el | le | la
										| lo | los | op | te | ten | ter | uit 
										| vd | v.d. | v\/d
										| au | aux | a | à | à la | a la 
										| \- |\s|\s+|\-+";
			string[] newstr = Regex.Split(inp, Regex.Replace(expression, @"\s+", " "));
			return newstr.Aggregate(inp, (current, t) =>
				Regex.Replace(current, t, t.Length > 0 ? t.Substring(0, 1).ToUpper() + t.Substring(1).ToLower() : t)
				);
		}

		static bool isVowel(char c) { return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U'; }

		public static string Depluralize(string pluralstring)
		{
			if (pluralstring.EndsWith("s")) return pluralstring.Remove(pluralstring.Length - 1);
			if (pluralstring.EndsWith("en"))
				if (pluralstring.Length >= 4 && isVowel(pluralstring[pluralstring.Length - 4]) && (pluralstring.Length < 5 || !isVowel(pluralstring[pluralstring.Length - 5])))
					return pluralstring.Remove(pluralstring.Length - 3) + pluralstring.Substring(pluralstring.Length - 4, 2).ToLowerInvariant();
				else if (pluralstring.Length >= 4 && pluralstring[pluralstring.Length - 4] == pluralstring[pluralstring.Length - 3])
					return pluralstring.Remove(pluralstring.Length - 3);
				else
					return pluralstring.Remove(pluralstring.Length - 2);
			else
				return pluralstring;
		}
	}
}
