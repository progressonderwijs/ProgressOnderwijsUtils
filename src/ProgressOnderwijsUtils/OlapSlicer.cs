using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class OlapSlicer
	{
		int organisatie;
		int startjaar;
		int stopjaar;
		string rijen;
		bool showopleidingen=true;
		int examentype;
		bool zuivercohort = true;
		bool cohort = true;

		/// <summary>
		/// als true, 1 cohortjaar
		/// als false meerdere cohorten
		/// default true
		/// </summary>
		public bool Cohort
		{
			get { return cohort; }
			set { cohort = value; }
		}

		public bool ZuiverCohort
		{
			get { return zuivercohort; }
			set { zuivercohort = value; }
		}

		public int ExamenType
		{
			get { return examentype; }
			set { examentype = value; }
		}

		int opleiding;

		public int Opleiding
		{
			get { return opleiding; }
			set { opleiding = value; }
		}

		public bool ShowOpleidingen
		{
			get { return showopleidingen; }
			set { showopleidingen = value; }
		}

		public int Organisatie
		{
			get { return organisatie; }
			set { organisatie = value; }
		}

		public int StartJaar
		{
			get { return startjaar; }
			set { startjaar = value; }
		}

		public int StopJaar
		{
			get { return stopjaar; }
			set { stopjaar = value; }
		}

		public string Rijen
		{
			get { return rijen; }
			set { rijen = value; }
		}

		//public rijdimensies RijDimensie
		//{
		//    get { return rijdimensie; }
		//    set { rijdimensie = value;  }
		//}

		//public enum rijdimensies
		//{
		//    OPLEIDINGEN = 0,
		//    ORGANISATIES = 1
		//}

		public OlapSlicer(int organisatie, int startjaar, string rijen, bool showopleidingen)
		{
			this.organisatie = organisatie;
			this.startjaar = startjaar;
			this.rijen = rijen;
			this.showopleidingen = showopleidingen;
		}

		public OlapSlicer(int organisatie, int startjaar, int stopjaar)
		{
			this.organisatie = organisatie;
			this.startjaar = startjaar;
			this.stopjaar = stopjaar;
		}

		public OlapSlicer(int organisatie, int startjaar, int stopjaar, int examentype)
		{
			this.organisatie = organisatie;
			this.startjaar = startjaar;
			this.stopjaar = stopjaar;
			this.examentype = examentype;
		}

		public OlapSlicer(int organisatie, int startjaar, int stopjaar, string rijen, bool showopleidingen)
		{
			this.organisatie = organisatie;
			this.startjaar = startjaar;
			this.stopjaar = stopjaar;
			this.rijen = rijen;
			this.showopleidingen = showopleidingen;
		}
	}
}
