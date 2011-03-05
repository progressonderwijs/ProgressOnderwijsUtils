using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public class BSADossierData
	{
		public int DossierID { get; set; }
		public string IngevoerdDoor { get; set; }
		public DateTime DatumInvoer { get; set; }
		public int TypeID { get; set; }
		public string TypeBeschrijving { get; set; }
		public string Onderwerp { get; set; }
		public string Inhoud { get; set; }
		public string Taal { get; set; }
		public int MediumTypeID { get; set; }
		public string MediumTypeBeschrijving { get; set; }
	}
}
