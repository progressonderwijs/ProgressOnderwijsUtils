using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils.Functional {
	/// <summary>
	/// C# won't let you construct a delegate without tieing it to a specific instance of an object.  
	/// However it's handy to be able to call tostring from inside an IEnumerable Map.
	/// These functions let you do that. They also let you call ToString if the object is null, returning 
	/// either null or the empty string depending on the function chosen.
	/// </summary>
	public class Util {
		public static string ToString<T1>(T1 obj) { return obj.ToString(); }
		public static string ToStringOrNull<T1>(T1 obj) { if (obj == null) return null;else return obj.ToString(); }
		public static string ToStringOrEmpty<T1>(T1 obj) { if (obj == null) return ""; else return obj.ToString(); }


	}
}
