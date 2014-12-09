﻿using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils
{
    // ReSharper disable UnusedMember.Global
    /// <summary>
    /// tooltje om handige type-wrapper te maken: used via LINQPAD
    /// </summary>
    public static class CodeGenHelper
    {
        public static string GetColumnProperty(ColumnDefinition col)
        {
            return "public " + ObjectToCode.GetCSharpFriendlyTypeName(col.DataType) + " " + StringUtils.Capitalize(col.Name) + " { get; set; }\n";
        }

        public static string GetColumnField(ColumnDefinition col)
        {
            return "public readonly " + ObjectToCode.GetCSharpFriendlyTypeName(col.DataType) + " " + StringUtils.Capitalize(col.Name) + ";\n";
        }

        public static string GetColumnParameter(ColumnDefinition col)
        {
            return ObjectToCode.GetCSharpFriendlyTypeName(col.DataType) + " " + StringUtils.Uncapitalize(col.Name);
        }

        static readonly Regex newLine = new Regex("^(?!$)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public static string Indent(string str, int indentCount = 1) { return newLine.Replace(str, new string('\t', indentCount * 2)); }

        public static string GetMetaObjectClassDef(this QueryBuilder q, SqlCommandCreationContext conn, string name = null)
        {
            var wrapped = "select top 0 q.* from (" + q + ") q";
            var dt = AutoLoadFromDb.ReadDataTableWithSqlMetadata(wrapped, conn);
            return DataTableToMetaObjectClassDef(dt, name);
        }

        /// <summary>
        /// This method makes a "best effort" auto-generated metaobject class that can replace the current datatable.
        /// It's not quite as accurate as GetMetaObjectClassDef on a QueryBuilder; use that in preference.
        /// </summary>
        public static string DataTableToMetaObjectClassDef(this DataTable dt, string classNameOverride = null)
        {
            classNameOverride = classNameOverride ?? (String.IsNullOrEmpty(dt.TableName) ? "XYZ" : dt.TableName);
            return ("public sealed class " + classNameOverride + " : IMetaObject "
                + "{\n"
                + Indent(
                    dt.Columns.Cast<DataColumn>().Select(
                        dc => {
                            var columnDefinition = ColumnDefinition.Create(dc);
                            var isKey = dt.PrimaryKey.Contains(dc);
                            var verplicht = !dc.AllowDBNull && !isKey && columnDefinition.DataType.CanBeNull();
                            var attrs = new[] {
                                (isKey ? "Key" : null),
                                (verplicht ? "MpVerplicht" : null),
                                (dc.ReadOnly ? "MpReadonly" : null),
                                (dc.MaxLength >= 0 && dc.MaxLength < int.MaxValue ? "MpMaxLength(" + dc.MaxLength + ")" : null),
                            }.Where(a => a != null).ToArray();
                            return (attrs.Any() ? "[" + attrs.JoinStrings(", ") + "]\n" : "")
                                + GetColumnProperty(columnDefinition)
                                ;
                        }).JoinStrings())
                + "}\n"
                ).Replace("\n", "\r\n");
        }

        public static string GetILoadFromDbByConstructorDefinition(SqlCommandCreationContext conn, QueryBuilder q, string name = null)
        {
            var wrapped = "select top 0 q.* from (" + q + ") q";
            var dt = AutoLoadFromDb.ReadDataTableWithSqlMetadata(wrapped, conn);
            var columns = dt.Columns.Cast<DataColumn>().Select(ColumnDefinition.Create).ToArray();
            name = name ?? (String.IsNullOrEmpty(dt.TableName) ? "ZZ_SAMPLE_CLASS" : dt.TableName);
            return (
                "public sealed class " + name + " : ILoadFromDbByConstructor\n"
                    + "{\n"
                    + Indent(
                        columns.Select(GetColumnField).JoinStrings()
                            + "public " + name + "(" + columns.Select(GetColumnParameter).JoinStrings(", ") + ")"
                            + "{\n"
                            + Indent(
                                columns.Select(
                                    col =>
                                        (StringUtils.Capitalize(col.Name) == StringUtils.Uncapitalize(col.Name) ? "this." : "") + StringUtils.Capitalize(col.Name) + " = "
                                            + StringUtils.Uncapitalize(col.Name) + ";\n").JoinStrings()
                                )
                            + "}\n\n"
                        ) + "}\n"
                ).Replace("\n", "\r\n");
        }

        public static string GetRowClassDef(DataTable dt, string name = null)
        {
            var columns = dt.Columns.Cast<DataColumn>().Select(ColumnDefinition.Create).ToArray();
            var primes = Utils.Primes().Skip(1).Take(columns.Length).ToArray();
            name = name ?? (String.IsNullOrEmpty(dt.TableName) ? "XYZ" : dt.TableName);
            return (
                "public sealed class " + name + " : IEquatable<" + name + "> {\n"
                    + "\treadonly int _hashcode_auto_cached;\n"
                    + String.Join("", columns.Select(col => "\tpublic readonly " + ObjectToCode.GetCSharpFriendlyTypeName(col.DataType) + " " + col.Name + ";\n"))
                    + "\tpublic " + name + "(DataRow dr) {\n"
                    + "\t\tulong _hashcode_auto_building=0;\n"
                    + String.Join(
                        "",
                        columns.Select(
                            (col, i) => "\t\t" + col.Name + " = dr.Field<" + ObjectToCode.GetCSharpFriendlyTypeName(col.DataType) + ">(\"" + col.Name + "\");\n"
                                + "\t\t_hashcode_auto_building += " + (
                                    col.DataType.IsValueType
                                        ? "(ulong)" + col.Name + ".GetHashCode()"
                                        : col.Name + " == null ? " + i + " : (ulong)" + col.Name + ".GetHashCode() *  " + primes[i] + "UL"
                                    ) + ";\n")
                        )
                    + "\t\t_hashcode_auto_cached = (int)(uint)(_hashcode_auto_building ^ (_hashcode_auto_building >> 32));\n"
                    + "\t}\n"
                    + "\tpublic bool Equals(" + name + " that) {\n"
                    + "\t\treturn "
                    + String.Join("\n\t\t\t&& ", columns.Select(col => "Equals(" + col.Name + ", that." + col.Name + ")")) + ";\n"
                    + "\t}\n"
                    + "\tpublic override int GetHashCode() { return _hashcode_auto_cached; }\n"
                    + "\tpublic override bool Equals(object that) { return that is " + name + " && Equals((" + name + ")that); }\n"
                    + "}\n"
                ).Replace("\n", "\r\n");
        }
    }

#if false
		sealed class IndentingLogger
		{
			StringBuilder text = new StringBuilder();
			int indent = 0;
			bool isLocked;
			public void Add(string line)
			{
				if (isLocked) throw new InvalidOperationException("Cannot add line; indented section is busy!");
				text.AppendLine(new string('\t', indent) + line);
			}
			public void Add(params string[] lines)
			{
				if (isLocked) throw new InvalidOperationException("Cannot add line; indented section is busy!");
				foreach (var line in lines)
					text.AppendLine(new string('\t', indent) + line);
			}
			public void Indent(Action<IndentingLogger> todo)
			{
				isLocked = true;
				try
				{
					todo(new IndentingLogger { indent = indent + 1, text = text });
				}
				finally
				{
					isLocked = false;
				}
			}
			public override string ToString() { return text.ToString(); }
		}
#endif

    // ReSharper restore UnusedMember.Global
}
