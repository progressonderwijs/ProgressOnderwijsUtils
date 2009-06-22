using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils.Functional
{
	/// <summary>
	/// This code originates from CS coursework by Eamon.
	/// </summary>
	public class F
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static T Swallow<T>(Func<T> trial, Func<T> error)
		{
			try { return trial(); }
			catch (Exception) { return error(); }
		}

		public static T Swallow<T, TE>(Func<T> trial, Func<TE, T> error) where TE : Exception
		{
			try { return trial(); }
			catch (TE e) { return error(e); }
		}
	}
}
