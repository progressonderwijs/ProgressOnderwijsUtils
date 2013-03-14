using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	class ToegangsRol
	{
	}

	/*
	 * Ontwikkelmethodiek:
	 *  
	 * - Code generator die van DB toegangrecht een grote enum maakt
	 * - Zorg ervoor dan nieuwe rechten en oude rechten identiek namen hebben (makkelijker refactoren later).
	 * 
	 * - Hierarchyclass maken die declaratief toestaat om ouder rollen te defineren
	 * - Code die de transitive closure van de rollen structuur bepaald en zo dus de relevante onderliggende rollen van een gegeven voorouderrol
	 * - wat "common sense" unit tests"
	 * - wat unit tests die voor elk account, voor elke organisatie checked dat exact dezelfde rechten uitgekeerd worden.
	 * - wat unit tests die checken of de db wel met de enum in sync is.
	 * 
	 * - alle queries naar toegantrecht etc. aanpassen
	 * - later: alle tabellen behalve de kern recht tabel verwijderen.
	 * 
	 * */
}
