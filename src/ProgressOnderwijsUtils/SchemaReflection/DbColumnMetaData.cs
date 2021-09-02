using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed record DbColumnMetaData(
        string ColumnName,
        SqlSystemTypeId UserTypeId,
        short MaxLength,
        byte Precision,
        byte Scale
    ) : IWrittenImplicitly
    {
        public DbObjectId DbObjectId { get; init; }
        public DbColumnId ColumnId { get; init; }
        public byte ColumnFlags { get => columnFlags.PackedValues; init => columnFlags.PackedValues = value; }
        EightFlags columnFlags;

        public bool IsNullable
        {
            get => columnFlags[0];
            set => columnFlags[0] = value;
        }

        public bool IsComputed
        {
            get => columnFlags[1];
            set => columnFlags[1] = value;
        }

        public bool IsPrimaryKey
        {
            get => columnFlags[2];
            set => columnFlags[2] = value;
        }

        public bool HasAutoIncrementIdentity
        {
            get => columnFlags[3];
            set => columnFlags[3] = value;
        }

        public bool HasDefaultValue
        {
            get => columnFlags[4];
            set => columnFlags[4] = value;
        }

        public static DbColumnMetaData Create(string name, Type dataType, bool isKey, int? maxLength)
        {
            var typeId = SqlSystemTypeIdExtensions.DotnetTypeToSqlType(dataType);

            var maxLengthForSqlServer = (short)(typeId switch {
                SqlSystemTypeId.NVarChar or SqlSystemTypeId.NChar => maxLength * 2 ?? -1,
                SqlSystemTypeId.VarChar or SqlSystemTypeId.Char or SqlSystemTypeId.VarBinary or SqlSystemTypeId.Binary => maxLength ?? -1,
                _ => 0,
            });

            var (precision, scale) =
                typeId switch {
                    SqlSystemTypeId.Decimal or SqlSystemTypeId.Numeric => (38, 2),
                    SqlSystemTypeId.DateTime2 or SqlSystemTypeId.DateTimeOffset or SqlSystemTypeId.Time => (0, 7),
                    _ => (0, 0)
                };

            return new(name, typeId, maxLengthForSqlServer, (byte)precision, (byte)scale) { IsNullable = dataType.CanBeNull(), IsPrimaryKey = isKey, };
        }

        public bool IsString
            => UserTypeId.SqlUnderlyingTypeInfo().ClrType == typeof(string);

        public bool IsUnicode
            => UserTypeId == SqlSystemTypeId.NVarChar || UserTypeId == SqlSystemTypeId.NChar;

        public bool IsRowVersion
            => UserTypeId == SqlSystemTypeId.RowVersion;

        public bool HasMaxLength
            => MaxLength > 0;

        public override string ToString()
            => ToStringByMembers.ToStringByPublicMembers(this);

        public string ToSqlColumnDefinition()
            => $"{ColumnName} {ToSqlTypeName()}";

        string ColumnPrecisionSpecifier()
            => UserTypeId switch {
                SqlSystemTypeId.Decimal or SqlSystemTypeId.Numeric => $"({Precision},{Scale})",
                SqlSystemTypeId.NVarChar or SqlSystemTypeId.NChar => MaxLength > 0 ? $"({MaxLength / 2})" : "(max)",
                SqlSystemTypeId.VarChar or SqlSystemTypeId.Char or SqlSystemTypeId.VarBinary or SqlSystemTypeId.Binary => MaxLength > 0 ? $"({MaxLength})" : "(max)",
                SqlSystemTypeId.DateTime2 or SqlSystemTypeId.DateTimeOffset or SqlSystemTypeId.Time when Scale != 7 => $"({Scale})",
                _ => ""
            };

        public string ToSqlTypeName()
            => ToSqlTypeNameWithoutNullability() + NullabilityAnnotation();

        public string ToSqlTypeNameWithoutNullability()
            => UserTypeId.SqlUnderlyingTypeInfo().SqlTypeName + ColumnPrecisionSpecifier();

        string NullabilityAnnotation()
            => IsNullable ? " null" : " not null";

        public ParameterizedSql ToSqlColumnDefinitionSql()
            => ParameterizedSql.CreateDynamic($"{ColumnName} {ToSqlTypeName()}");

        public DataColumn ToDataColumn()
            => new(ColumnName, UserTypeId.SqlUnderlyingTypeInfo().ClrType);

        public DbColumnMetaData AsStaticRowVersion()
        {
            if (IsRowVersion) {
                return this with {
                    UserTypeId = SqlSystemTypeId.Binary,
                    MaxLength = 8,
                };
            } else {
                return this;
            }
        }

        [Pure]
        public ParameterizedSql SqlColumnName()
            => ParameterizedSql.CreateDynamic(isSafeForSql.IsMatch(ColumnName) ? ColumnName : throw new NotSupportedException("this isn't safe!"));

        static readonly ParameterizedSql tempDb = SQL($"tempdb");

        public static DbColumnMetaData[] ColumnMetaDatas(SqlConnection conn, ParameterizedSql objectName)
            => ColumnMetaDatas(conn, objectName.CommandText());

        public static DbColumnMetaData[] ColumnMetaDatas(SqlConnection conn, string qualifiedObjectName)
        {
            var dbColumnMetaDatas = qualifiedObjectName.StartsWith("#", StringComparison.OrdinalIgnoreCase)
                ? RunQuery(conn, true, SQL($@"and c.object_id = object_id({$"{tempDb.CommandText()}..{qualifiedObjectName}"})"))
                : RunQuery(conn, false, SQL($@"and c.object_id = object_id({qualifiedObjectName})"));
            return Sort(dbColumnMetaDatas);
        }

        public static Dictionary<DbObjectId, DbColumnMetaData[]> LoadAll(SqlConnection conn)
            => RunQuery(conn, false, new()).ToGroupedDictionary(col => col.DbObjectId, (_, cols) => Sort(cols.ToArray()));

        static DbColumnMetaData[] Sort(DbColumnMetaData[] toArray)
        {
            Array.Sort(toArray, byColumnId);
            return toArray;
        }

        static readonly Comparison<DbColumnMetaData> byColumnId = (a, b) => ((int)a.ColumnId).CompareTo((int)b.ColumnId);
        static readonly Regex isSafeForSql = new("^[a-zA-Z0-9_]+$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        public static ParameterizedSql BaseQuery(bool fromTempDb)
            => SQL(
                $@"
                with pks (object_id, column_id) as (
                    select i.object_id, ic.column_id
                    from sys.index_columns ic 
                    join sys.indexes i on ic.object_id = i.object_id and ic.index_id = i.index_id and i.is_primary_key = 1
                )
                select
                    ColumnName = c.name
                    , DbObjectId = c.object_id
                    , ColumnId = c.column_id
                    , UserTypeId = c.user_type_id
                    , MaxLength = c.max_length
                    , c.Precision
                    , c.Scale
                    , ColumnFlags = convert(tinyint, 0
                        + 1*c.is_nullable 
                        + 2*c.is_computed
                        + 4*iif(pk.column_id is not null, convert(bit, 1), convert(bit, 0))
                        + 8*c.is_identity
                        + 16*iif(c.default_object_id <> 0, convert(bit, 1), convert(bit, 0))
                        )
                from {fromTempDb && SQL($"tempdb.")}sys.columns c
                left join pks pk on pk.object_id = c.object_id and pk.column_id = c.column_id
                where 1=1
            "
            );

        static DbColumnMetaData[] RunQuery(SqlConnection conn, bool fromTempDb, ParameterizedSql filter)
            => BaseQuery(fromTempDb).Append(filter).OfPocos<DbColumnMetaData>().WithFieldMappingMode(FieldMappingMode.IgnoreExtraPocoProperties).Execute(conn);
    }

    public static class DbColumnMetaDataExtensions
    {
        [Pure]
        public static ParameterizedSql CreateNewTableQuery(this IReadOnlyCollection<DbColumnMetaData> columns, ParameterizedSql tableName)
        {
            var keyColumns = columns
                .Where(md => md.IsPrimaryKey)
                .Select(md => $"{md.ColumnName}")
                .ToArray();
            // in een contained db mag er geen named PK worden gedefinieerd voor een temp. table
            // zolang er dus geen pk's over meerdere kolommen worden gedefinieerd gaat onderstaande ook goed voor temp. tables in een contained db
            var columnDefinitionSql = ParameterizedSql.CreateDynamic(
                columns
                    .Select(md => $"{md.ToSqlColumnDefinition()}{(keyColumns.Length == 1 && md.IsPrimaryKey ? " primary key" : "")}")
                    .JoinStrings("\r\n    , ")
            );
            var primaryKeyDefinitionSql = keyColumns.Length > 1
                ? SQL($"\r\n    , primary key ({ParameterizedSql.CreateDynamic(keyColumns.JoinStrings(", "))})")
                : ParameterizedSql.Empty;

            return SQL($"create table {tableName} (\r\n    {columnDefinitionSql}{primaryKeyDefinitionSql}\r\n);");
        }
    }
}
