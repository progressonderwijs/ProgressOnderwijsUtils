using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class KoppelTabelHelpers
    {
        [Pure]
        public static DataTable ToDataTable(this IEnumerable<KoppelTabelEntry> entries) => MetaObject.ToDataTable(entries, null);
    }

    public struct KoppelTabelEntry : IMetaObject
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
    }
}
