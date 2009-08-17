using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
	public static class Utils
	{
		public static string UitgebreideFout(Exception e, string tekst)
		{
			return tekst + e.Message + "  " + e.TargetSite.Name + " " + e.StackTrace;
		}

		public static string TestErrorStackOverflow(int rounds)
		{
			//This is intended for testing error-handling in case of dramatic errors.
			return TestErrorStackOverflow(rounds + 1);
		}

		public static void TestErrorOutOfMemory()
		{
			List<byte[]> memorySlurper = new List<byte[]>();
			for (long i = 0; i < long.MaxValue;i++ ) //no way any machine has near 2^70 bytes of RAM - a zettabyte! no way, ever. ;-)
			{
				memorySlurper.Add(Encoding.UTF8.GetBytes(@"This is a simply string which is repeatedly put in memory to test the Out Of Memory condition.  It's encoded to make sure the program really touches the data and that therefore the OS really needs to allocate the memory, and can't just 'pretend'."));
			}
		}

		public static void TestErrorNormalException()
		{
			throw new ApplicationException("This is a test exception intended to test fault-tolerance.  User's shouldn't see it, of course!");
		}

		public static bool ElfProef(int getal)
		{
			int res = 0;
			for (int i = 1; getal != 0; getal /= 10, ++i)
				res += i * (getal % 10);
			return res != 0 && res % 11 == 0;
		}

		///<summary>
		/// initialiseert opties voor een reguliere expressie
		/// </summary>
		/// <example>
		/// <c>
		/// RegexOptions ro = ReOpts("i,m,c");
		/// </c>
		/// => maakt een regex operatie case insensitive,
		///    multiline and culture invariant
		/// </example>
		/// <param name="x">(string) komma gescheiden letters of lege string
		/// bv "i,m" of "", waarbij
		/// i=ignorecase, m=multiline, s=singleline, r=righttoleft,
		/// e=explicitcapture en c=cultureinvariant</param>
		/// <canblame name="Renzo Kooi" value="but gently please"/>
		/// <datelast value="2009/08/15"/>
		/// <returns>RegexOptions enumerator</returns>
		public static RegexOptions ReOpts(string x)
		{
			RegexOptions opts = new RegexOptions();

			if (x == "" || x == "none")
				return opts = RegexOptions.None;

			string[] s = x.Split(',');

			for (int i = s.Length - 1; i >= 0; i--)
			{
				switch (s[i])
				{
					case "i": opts |= RegexOptions.IgnoreCase; break;
					case "m": opts |= RegexOptions.Multiline; break;
					case "s": opts |= RegexOptions.Singleline; break;
					case "r": opts |= RegexOptions.RightToLeft; break;
					case "e": opts |= RegexOptions.ExplicitCapture; break;
					case "c": opts |= RegexOptions.CultureInvariant; break;
					default: opts = RegexOptions.None; break;
				}
			}
			return opts;
		}
	}
}