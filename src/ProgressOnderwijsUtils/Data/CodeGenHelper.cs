namespace ProgressOnderwijsUtils;

/// <summary>
///     tooltje om handige type-wrapper te maken: used via LINQPAD
/// </summary>
public static class CodeGenHelper
{
    public static string GetColumnProperty(ColumnDefinition col, Func<ColumnDefinition, string>? colNameOverride = null)
    {
        var friendlyTypeName = colNameOverride ?? (x => x.DataType.ToCSharpFriendlyTypeName());

        return $"public {friendlyTypeName(col)} {StringUtils.Capitalize(col.Name)} {{ get; init; }}\n";
    }

    static readonly Regex newLine = new("^(?!$)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

    public static string Indent(string str, int indentCount = 1)
        => newLine.Replace(str, new string(' ', indentCount * 4));

    /// <summary>
    ///     This method makes a "best effort" auto-generated poco class that can replace the current datatable.
    /// </summary>
    public static string DataTableToPocoClassDef(this DataTable dt, string? classNameOverride = null, Func<ColumnDefinition, string>? colNameOverride = null)
    {
        classNameOverride ??= string.IsNullOrEmpty(dt.TableName) ? "XYZ" : dt.TableName;

        return "public sealed class " + classNameOverride + " : " + typeof(IWrittenImplicitly).ToCSharpFriendlyTypeName() + " "
            + "{\n"
            + Indent(
                dt.Columns.Cast<DataColumn>().Select(
                    dc => {
                        var columnDefinition = ColumnDefinition.Create(dc);
                        return GetColumnProperty(columnDefinition, colNameOverride);
                    }
                ).JoinStrings()
            )
            + "}\n";
    }
}
