using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class KoppelTabelHelpers
    {
        [Pure]
        public static DataTable ToDataTable(this IEnumerable<KoppelTabelEntry> entries) => MetaObject.ToDataTable(entries, null);

        public static DataTable EnumToIntKoppelTabel<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            //TODO:EMN:improve this API.
            return values.CreateSelectItemList()
                .Select(v => new KoppelTabelEntry { Id = v.Value.ToInt32(null), Tekst = v.Label.Translate(taal).Text }
                ).ToDataTable();
        }

        public static IReadOnlyList<SelectItem<TEnum>> CreateSelectItemList<TEnum>(this IEnumerable<TEnum> values)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(SelectItem.GetSelectItem).ToArray();
        }

        public static DataTable ToIntKoppelTabel_OrderedByText<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(
                v =>
                    new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = EnumHelpers.GetLabel<TEnum>(v).Translate(taal).Text }
                )
                .OrderBy(entry => entry.Tekst)
                .ToDataTable();
        }

        public static DataTable ToIntKoppelTabelExpandedText<TEnum>(IEnumerable<TEnum> values, Taal taal)
            where TEnum : struct, IConvertible, IComparable
        {
            return values.Select(
                v => {
                    var tv = EnumHelpers.GetLabel<TEnum>(v).Translate(taal);
                    return new KoppelTabelEntry { Id = v.ToInt32(null), Tekst = tv.Text + ": " + tv.ExtraText };
                }).ToDataTable();
        }
    }


    public class KoppelTabelEntry : IMetaObject, IComparable<KoppelTabelEntry>, IToSelectItem<int?>
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public int CompareTo(KoppelTabelEntry other) => Id.CompareTo(other.Id);
        public SelectItem<int?> ToSelectItem() => SelectItem.Create((int?)Id, Converteer.ToText(Tekst));
    }

    public struct KoppelTabelTaalEntry : IMetaObject
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public Taal Taal { get; set; }
        public KoppelTabelEntry ToEntryWithoutTaal() => new KoppelTabelEntry { Id = Id, Tekst = Tekst };
    }

}
