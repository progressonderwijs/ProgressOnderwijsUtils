using System;
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

    public struct KoppelTabelEntry : IMetaObject, IComparable<KoppelTabelEntry>
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public int CompareTo(KoppelTabelEntry other) => Id.CompareTo(other.Id);
    }
}
