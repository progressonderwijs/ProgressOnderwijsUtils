namespace ProgressOnderwijsUtils;

/// <summary>
/// Extensions for DataRow
/// </summary>
public static class DataRowExtensions
{
    [Pure]
    public static T Field<T>(this DataRowView row, string fieldname)
        => (T)row.Row[fieldname];
}