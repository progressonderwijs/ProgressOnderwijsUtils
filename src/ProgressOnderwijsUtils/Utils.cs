using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils;

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
			throw new ApplicationException("This is a test exception intended to test fault-tolerance.  " +
				                           "User's shouldn't see it, of course!");
		}

		public static bool ElfProef(int getal)
		{
			int res = 0;
			for (int i = 1; getal != 0; getal /= 10, ++i)
				res += i * (getal % 10);
			return res != 0 && res % 11 == 0;
		}

		///<summary>
		/// Utility om van een serie strings een serie
		/// Tuples van strings te maken (paren, om precies
		/// te zijn). 
		/// Te gebruiken bij Tools.StringExtensions.MultiReplace,
		/// om de tweede parameter wat gemakkelijker samen te stellen
		/// </summary>
		/// <param name="p">[stringa,stringb,stringc,stringd, ... , stringxyz]</param>
		/// <example>
		/// Om twee paren van Regex-string en vervangstring te maken bv:
		/// <code>
		/// Tuple<string,string>[] tupz = 
		///				Tools.Utils.ToTuples(@"\r\n" ," \n" ,
		///									 @"\email","e-mail");
		/// </code>
		///	 of direct in de MultiReplace-extension van 'n string:
		/// <code>
		///	 [string].MultiReplace(
		///						RegexOptions.Multiline | RegexOptions.IgnoreCase,
		///						==> Tools.Utils.ToTuples(@"\r\n", "\n") <==
		///					  );
		/// </code>	
		/// </example>
		/// <canblame>Renzo Kooi</canblame>
		/// <datelast value="2009/08/18"/>
		/// <returns>Array van Tuples van twee strings</returns>
		public static Tuple<string, string>[] ToTuples(params string[] p)
		{
			int i = 0, plen = p.Length;
			plen = (plen % 2 != 0) ? plen-- : plen;
			Tuple<string, string>[] tupz = new Tuple<string, string>[p.Length / 2];
			do
			{
				tupz[i / 2] = TupleF.Create<string, string>(p[i], p[i + 1]);
			} while ((i += 2) < plen);
			return tupz;
		}
	}
}