using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed class DbColumnMetaData : IMetaObject
    {
        public static DbColumnMetaData Create(string name, Type dataType, bool isKey, int? maxLength)
        {
            var metaData = new DbColumnMetaData {
                ColumnName = name,
                User_Type_Id = SqlXTypeExtensions.NetTypeToSqlXType(dataType),
                Is_Nullable = dataType.CanBeNull(),
                Is_Primary_Key = isKey,
            };
            if (dataType == typeof(string)) {
                metaData.Max_Length = (short)(maxLength * 2 ?? SchemaReflection.SqlTypeInfo.VARCHARMAX_MAXLENGTH_FOR_SQLSERVER);
            }
            if (dataType == typeof(decimal) || dataType == typeof(decimal?) || dataType == typeof(double) || dataType == typeof(double?)) {
                metaData.Precision = 38;
                metaData.Scale = 2;
            }
            return metaData;
        }

        public DbObjectId DbObjectId { get; set; }
        public string ColumnName { get; set; }

        /// <summary>
        /// This id is 1-based and may contain gaps due to dropping of columns.
        /// </summary>
        public ColumnIndex ColumnId { get; set; }

        /// <summary>
        /// This is the actual zero-based index within the table.
        /// </summary>
        public int Ordinal { get; set; }
        
        public SqlXType User_Type_Id { get; set; }
        public short Max_Length { get; set; } = SchemaReflection.SqlTypeInfo.VARCHARMAX_MAXLENGTH_FOR_SQLSERVER;
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public bool Is_Nullable { get; set; } = true;
        public bool Is_Computed { get; set; }
        public bool Is_Primary_Key { get; set; }

        public bool Is_RowVersion
            => User_Type_Id == SqlXType.RowVersion;

        public override string ToString()
            => ToStringByMembers.ToStringByPublicMembers(this);

        public SqlTypeInfo SqlTypeInfo()
            => new SqlTypeInfo(User_Type_Id, Max_Length, Precision, Scale, Is_Nullable);

        public string ToSqlColumnDefinition()
            => $"{ColumnName} {SqlTypeInfo().ToSqlTypeName()}";

        public DataColumn ToDataColumn()
            => new DataColumn(ColumnName, User_Type_Id.SqlUnderlyingTypeInfo().ClrType);

        static readonly ParameterizedSql tempDb = SQL($"tempdb");

        static ParameterizedSql BaseQuery(ParameterizedSql database)
            => SQL($@"
                with pks (object_id, column_id) as (
                    select i.object_id, ic.column_id
                    from sys.index_columns ic 
                    join sys.indexes i on ic.object_id = i.object_id and ic.index_id = i.index_id and i.is_primary_key = 1
                )
                select
                    DbObjectId = c.object_id
                    , ColumnName = c.name
                    , ColumnId = c.column_id
                    , Ordinal = convert(int, row_number() over(partition by c.object_id order by c.column_id)) - 1
                    , c.user_type_id
                    , c.max_length
                    , c.precision
                    , c.scale
                    , c.is_nullable
                    , c.is_computed
                    , is_primary_key = iif(pk.column_id is not null, convert(bit, 1), convert(bit, 0))
                from {database}{(database.IsEmpty ? ParameterizedSql.Empty : SQL($"."))}sys.columns c
                left join pks pk on pk.object_id = c.object_id and pk.column_id = c.column_id
                where 1=1
            ");

        public static DbColumnMetaData[] ColumnMetaDatas(SqlCommandCreationContext conn, ParameterizedSql objectName)
            => ColumnMetaDatas(conn, objectName.CommandText());

        public static DbColumnMetaData[] ColumnMetaDatas(SqlCommandCreationContext conn, string qualifiedObjectName)
        {
            if (qualifiedObjectName.StartsWith("#")) {
                return BaseQuery(tempDb).Append(SQL($@"
                        and c.object_id = object_id({$"{tempDb.CommandText()}..{qualifiedObjectName}"})
                    order by c.column_id
                ")).ReadMetaObjects<DbColumnMetaData>(conn);
            } else {
                return BaseQuery(ParameterizedSql.Empty).Append(SQL($@"
                        and c.object_id = object_id({qualifiedObjectName})
                    order by c.column_id
                ")).ReadMetaObjects<DbColumnMetaData>(conn);
            }
        }

        public static Dictionary<DbObjectId, DbColumnMetaData[]> LoadAll(SqlCommandCreationContext conn)
            => BaseQuery(ParameterizedSql.Empty).ReadMetaObjects<DbColumnMetaData>(conn)
                .ToGroupedDictionary(col => col.DbObjectId, (_, cols) => cols.ToArray());

        static readonly Regex isSafeForSql = new Regex("^[a-zA-Z0-9_]+$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        [Pure]
        public ParameterizedSql SqlColumnName()
            => ParameterizedSql.CreateDynamic(isSafeForSql.IsMatch(ColumnName) ? ColumnName : throw new NotSupportedException("this isn't safe!"));
    }

    public static class DbColumnMetaDataExtensions
    {
        [Pure]
        public static ParameterizedSql CreateNewTableQuery(this IReadOnlyCollection<DbColumnMetaData> columns, ParameterizedSql tableName)
        {
            var keyColumns = columns
                .Where(md => md.Is_Primary_Key)
                .Select(md => $"{md.ColumnName}")
                .ToArray();
            // in een contained db mag er geen named PK worden gedefinieerd voor een temp. table
            // zolang er dus geen pk's over meerdere kolommen worden gedefinieerd gaat onderstaande ook goed voor temp. tables in een contained db
            var columnDefinitionSql = ParameterizedSql.CreateDynamic(columns
                .Select(md => $"{md.ToSqlColumnDefinition()}{(keyColumns.Length == 1 && md.Is_Primary_Key ? " primary key" : "")}")
                .JoinStrings("\r\n    , ")
            );
            var primaryKeyDefinitionSql = keyColumns.Length > 1
                ? SQL($"\r\n    , primary key ({ParameterizedSql.CreateDynamic(keyColumns.JoinStrings(", "))})")
                : ParameterizedSql.Empty;

            return SQL($"create table {tableName} (\r\n    {columnDefinitionSql}{primaryKeyDefinitionSql}\r\n);");
        }
    }
}
