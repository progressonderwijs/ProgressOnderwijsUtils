using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgressOnderwijsUtils
{
	public delegate T Factory<T>();
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

		/*
		 * initialiseert opties voor een reguliere expressie
		 * zie switch-case voor mogelijke opties.
		 * RK20090815
		 */
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
		/*
		 * MultiReplace: vervang 1 of meer substrings in [initialstring]
		 * door een andere string. Zoek en vervang is een array van strings
		 * [searchreplace], waarin 1 of meer paren van 'n reguliere expressie 
		 * (zoek) en een vervangstring zitten.
		 * De 3e parameter zijn de opties, in de vorm van een string met 
		 * optie-letters gescheiden door een komma, die via ReOpts (zie hierboven)
		 * naar een RegexOptions type worden omgezet. Mag ook een lege string
		 * zijn (geen opties).
		 * RK20090815
		 */
		public static string MultiReplace(string initialstr, string[] searchreplace, string opts)
		{
			string retval = initialstr;
			if (searchreplace.Length % 2 != 0)
			{
				return initialstr;
			}

			string regex = searchreplace[0], replacewith = searchreplace[1];
			RegexOptions ro = ReOpts(opts);
			retval = Regex.Replace(retval, regex, replacewith, ro);

			if (searchreplace.Length > 2)
			{
				for (int i = 0; i < searchreplace.Length; i += 2)
				{
					string[] s_ = new string[2] { searchreplace[i], searchreplace[i + 1] };
					retval = MultiReplace(retval,s_,opts);
				}
			}
			return retval;
		}
	}
}
