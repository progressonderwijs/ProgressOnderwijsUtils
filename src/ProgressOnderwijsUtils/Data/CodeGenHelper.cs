using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    ///     tooltje om handige type-wrapper te maken: used via LINQPAD
    /// </summary>
    public static class CodeGenHelper
    {
        [NotNull]
        public static string GetColumnProperty([NotNull] ColumnDefinition col, Func<ColumnDefinition, string>? colNameOverride = null)
        {
            Func<ColumnDefinition, string> friendlyTypeNameDefault = x => x.DataType.ToCSharpFriendlyTypeName();
            var friendlyTypeName = colNameOverride ?? friendlyTypeNameDefault;

            return "public " + friendlyTypeName(col) + " " + StringUtils.Capitalize(col.Name) + " { get; set; }\n";
        }

        static readonly Regex newLine = new Regex("^(?!$)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        [NotNull]
        public static string Indent([NotNull] string str, int indentCount = 1)
            => newLine.Replace(str, new string(' ', indentCount * 4));

        /// <summary>
        ///     This method makes a "best effort" auto-generated poco class that can replace the current datatable.
        /// </summary>
        [NotNull]
        public static string DataTableToPocoClassDef([NotNull] this DataTable dt, string? classNameOverride = null, Func<ColumnDefinition, string>? colNameOverride = null)
        {
            classNameOverride = classNameOverride ?? (string.IsNullOrEmpty(dt.TableName) ? "XYZ" : dt.TableName);

            return ("public sealed class " + classNameOverride + " : " + typeof(IWrittenImplicitly).ToCSharpFriendlyTypeName() + " "
                    + "{\n"
                    + Indent(
                        dt.Columns.Cast<DataColumn>().Select(
                            dc => {
                                var columnDefinition = ColumnDefinition.Create(dc);
                                var isKey = dt.PrimaryKey.Contains(dc);
                                var verplicht = !dc.AllowDBNull && !isKey && columnDefinition.DataType.CanBeNull();
                                var attrs = new[] {
                                    isKey ? "Key" : null,
                                    verplicht ? "MpVerplicht" : null,
                                    dc.ReadOnly ? "MpReadonly" : null,
                                    dc.MaxLength >= 0 && dc.MaxLength < int.MaxValue ? "MpMaxLength(" + dc.MaxLength + ")" : null,
                                }.Where(a => a != null).ToArray();
                                return (attrs.Any() ? "[" + attrs.JoinStrings(", ") + "]\n" : "")
                                    + GetColumnProperty(columnDefinition, colNameOverride)
                                    ;
                            }).JoinStrings())
                    + "}\n"
                ).Replace("\n", "\r\n");
        }
    }
}
