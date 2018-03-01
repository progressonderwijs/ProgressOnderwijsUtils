﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class SqlCommandDebugStringifier {
        [NotNull]
        public static string DebugFriendlyCommandText([NotNull] SqlCommand sqlCommand, SqlTracerAgumentInclusion includeSensitiveInfo)
            => CommandParamStringOrEmpty(sqlCommand, includeSensitiveInfo) + sqlCommand.CommandText;

        [NotNull]
        static string CommandParamStringOrEmpty(SqlCommand sqlCommand, SqlTracerAgumentInclusion includeSensitiveInfo)
        {
            if (includeSensitiveInfo == SqlTracerAgumentInclusion.IncludingArgumentValues) {
                return CommandParamString(sqlCommand);
            } else {
                return "";
            }
        }

        [NotNull]
        static string CommandParamString([NotNull] SqlCommand sqlCommand)
            => sqlCommand.Parameters.Cast<SqlParameter>().Select(DeclareParameter).JoinStrings();

        [NotNull]
        static string DeclareParameter([NotNull] SqlParameter par)
        {
            var declareVariable = "DECLARE " + par.ParameterName + " AS " + SqlParamTypeString(par);
            if (par.SqlDbType != SqlDbType.Structured) {
                return declareVariable
                    + " = " + InsecureSqlDebugString(par.Value, true) + ";\n";
            } else {
                return declareVariable
                    + "/*" + par.Value.GetType().ToCSharpFriendlyTypeName() + "*/;\n"
                    + "insert into " + par.ParameterName + " values "
                    + ValuesClauseForTableValuedParameter((par.Value as IOptionalObjectListForDebugging)?.ContentsForDebuggingOrNull());
            }
        }

        [NotNull]
        static string ValuesClauseForTableValuedParameter([CanBeNull] IReadOnlyList<object> tableValue)
        {
            if (tableValue == null) {
                return "(/* UNKNOWN? */);\n";
            }
            const int maxValuesToInsert = 20;
            var valuesString = tableValue.Take(maxValuesToInsert).Select(v => $"({InsecureSqlDebugString(v, false)})").JoinStrings(", ");
            var valueCountCommentIfNecessary = (tableValue.Count <= maxValuesToInsert ? "" : "\n    /* ... and more; " + tableValue.Count + " total */") + ";\n";
            return valuesString + valueCountCommentIfNecessary;
        }

        [NotNull]
        static string SqlParamTypeString([NotNull] SqlParameter par)
            => par.SqlDbType == SqlDbType.Structured
                ? par.TypeName
                : par.SqlDbType + (par.SqlDbType == SqlDbType.NVarChar ? "(max)" : "");

        [NotNull]
        public static string InsecureSqlDebugString([CanBeNull] object p, bool includeReadableEnumValue)
        {
            if (p is DBNull || p == null) {
                return "NULL";
            } else if (p is DateTime) {
                return "'" + ((DateTime)p).ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture) + "'";
            } else if (p is string) {
                return "'" + ((string)p).Replace("'", "''") + "'";
            } else if (p is long) {
                return ((long)p).ToStringInvariant();
            } else if (p is int) {
                return ((int)p).ToStringInvariant();
            } else if (p is bool) {
                return (bool)p ? "1" : "0";
            } else if (p is Enum) {
                return ((IConvertible)p).ToInt64(null).ToStringInvariant() + (includeReadableEnumValue ? "/*" + ObjectToCode.PlainObjectToCode(p) + "*/" : "");
            } else if (p is IFormattable) {
                return ((IFormattable)p).ToString(null, CultureInfo.InvariantCulture);
            } else {
                try {
                    return "{!" + p + "!}";
                } catch (Exception e) {
                    return $"[[Exception in {nameof(SqlCommandTracer)}.{nameof(InsecureSqlDebugString)}: {e.Message}]]";
                }
            }
        }
    }
}