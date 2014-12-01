using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Geeft het collegejaar van een bepaalde datum
        /// voorbeelden :
        /// 04-05-2009 -> 2008
        /// 01-09-2008 -> 2008
        /// </summary>
        public static int CollegeJaar(this DateTime datetime)
        {
            //Als startdatum januari t/m augustus dan is het collegejaar een jaar eerder
            return datetime.Year - (datetime.Month < 9 ? 1 : 0);
        }
    }
}
