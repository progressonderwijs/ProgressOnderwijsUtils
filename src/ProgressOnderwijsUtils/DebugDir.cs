using System;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ProgressOnderwijsUtils 
{
	public static class DebugDir 
	{
		public const string PROGRESSDEBUGDIR = "c:\\progress\\debug";
		public const string PROGRESSERRORDIR = "c:\\progress\\error";

		const string DEBUGFILE = "debug.txt";


		const int MAX_LENGTH_LOGENTRY = 16000;

		
		/// <summary>
		/// Schrijf de string naar de debugfile 
		/// </summary>
		/// <param name="tekst"></param>
		public static void Schrijf( string tekst) 
		{
#if DEBUG
			Schrijf(PROGRESSDEBUGDIR, tekst);	
#endif
		}

		static void Schrijf(string debugdir, string tekst) 
		{
			DateTime dt = DateTime.Now;		
			string monthyear=((dt.Month)).ToString() + "-" + dt.Year.ToString(); 
			string filepath = debugdir +"\\"  + monthyear +DEBUGFILE  ;
			try 
			{
				//Maak directory aan 								
				Directory.CreateDirectory(debugdir);
				File.AppendAllText(filepath, dt.ToString("dd'-'MM'-'yyyy' 'HH:mm:ss:ffff") + " : " + tekst + "\n");
			}
			catch (Exception e) 
			{
				//fout wegschrijven naar eventlog
				schrijfevent("debugdir.Schrijf ",e.Message +" bestand :"  +filepath);				
			}
		} //end Schrijf

		public  static void schrijfevent(string source, string entry) 
		{
			String log = "Application";
			if ( !EventLog.SourceExists(source) ) 
			{
				EventLog.CreateEventSource(source,log);
			}

			EventLog aLog = new EventLog();
			aLog.Source = source;

			if ( aLog.Log.ToUpper() != log.ToUpper() ) 
			{
				Console.WriteLine("Some other application is using the source!");
				//return 1;
			}
			//Maximum lengte van entry = 16384
			// wij maken daar MAX_LENGTH_LOGENTRY van
			String limitedentry = entry.Substring(0,Math.Min(entry.Length,MAX_LENGTH_LOGENTRY));
			aLog.WriteEntry(limitedentry ,EventLogEntryType.Error);
		}
	} //end class
}
