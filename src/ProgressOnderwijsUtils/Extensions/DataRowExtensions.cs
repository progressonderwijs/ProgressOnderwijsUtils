using System.Data;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Extensions for DataRow
    /// </summary>
    public static class DataRowExtensions
    {
        [Pure]
        public static T Field<T>([NotNull] this DataRowView row, [NotNull] string fieldname)
        {
            return (T)row.Row[fieldname];
        }
    }
}
