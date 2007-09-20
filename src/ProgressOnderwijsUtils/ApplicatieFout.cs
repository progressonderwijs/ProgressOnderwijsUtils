using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class ApplicatieFout
	{
//		bool opgelost;
//		DateTime datum;
//		string email;
		string message;
		string versie;
		//string beschrijving;
		string stacktrace;
		
		public string Versie { get { return versie; } set { versie = value; }}
		public string Message { get { return message; } set { message = value; }}
		public string Stacktrace { get { return stacktrace; } set { stacktrace = value; }}

	}
}
