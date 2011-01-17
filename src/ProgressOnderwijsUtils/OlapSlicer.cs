using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public sealed class OlapSlicer
	{
		public const int BACHELOR = 5;
		public const int DOCTORAAL = 8;
		public const int MASTER = 11;
		public const int PROPEDEUTISCH = 2;
		public const int PROPEDEUTISCHBACHELOR = 3;
		public enum StudieStaakType { zonderstudiestakers = 1, alleenstudiestakers = 2, metstudiestakers = 3 }
		public bool Rendement { get; set; }
		public StudieStaakType StudieStaak { get; private set; }
		public bool Samenvatting { get; set; }
		public bool LangeLijst { get; set; }
		public bool PreciesEenCohort { get; set; }
		public bool ZuiverCohort { get; set; }
		public int ExamenType { get; set; }
		public int Opleiding { get; set; }
		public bool ShowOpleidingen { get; private set; }
		public int Organisatie { get; private set; }
		public int StartJaar { get; private set; }
		public int StopJaar { get; private set; }
		public string Rijen { get; set; }
		public OlapSlicer(int organisatie, int startjaar, int stopjaar, int examentype)
		{
			ShowOpleidingen = true;
			ZuiverCohort = true;
			PreciesEenCohort = true;
			Samenvatting = true;
			StudieStaak = StudieStaakType.metstudiestakers;
			Organisatie = organisatie;
			StartJaar = startjaar;
			StopJaar = stopjaar;
			ExamenType = examentype;
		}
	}
}
