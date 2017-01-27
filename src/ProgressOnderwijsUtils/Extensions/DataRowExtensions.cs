using System.Data;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    using static System.Data.DataRowExtensions;
    /// <summary>
    /// Extensions for DataRow
    /// </summary>
    public static class DataRowExtensions
    {
        [Pure]
        public static T Field<T>(this DataRowView row, string fieldname)
        {
            return row.Row.Field<T>(fieldname);
        }
    }
}
