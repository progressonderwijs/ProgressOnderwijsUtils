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


    public class KoppelTabelEntry : IMetaObject, IComparable<KoppelTabelEntry>, IToSelectItem<int?>
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public int CompareTo(KoppelTabelEntry other) => Id.CompareTo(other.Id);
        public SelectItem<int?> ToSelectItem() => SelectItem.Create((int?)Id, Translatable.Raw(Tekst));
    }

    public struct KoppelTabelTaalEntry : IMetaObject
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public Taal Taal { get; set; }
        public KoppelTabelEntry ToEntryWithoutTaal() => new KoppelTabelEntry { Id = Id, Tekst = Tekst };
    }

}
