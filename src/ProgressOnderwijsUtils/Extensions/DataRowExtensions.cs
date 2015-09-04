using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Extensions for DataRow
    /// </summary>
    public static class DataRowExtensions
    {
        [Pure]
        public static T Field<T>(this DataRowView row, string fieldname) { return row.Row.Field<T>(fieldname); }
    }
}
