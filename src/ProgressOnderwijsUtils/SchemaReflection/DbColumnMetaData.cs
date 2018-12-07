using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed class DbColumnMetaData
    {
        struct CompressedSysColumnsValue : IMetaObject
        {
            public string ColumnName { get; set; }
            public DbObjectId DbObjectId { get; set; }
            public DbColumnId ColumnId { get; set; }
            public SqlXType User_Type_Id { get; set; }
            public short Max_Length { get; set; }
            public byte Precision { get; set; }
            public byte Scale { get; set; }
            public byte ColumnFlags { get; set; }
        }

        DbColumnMetaData(CompressedSysColumnsValue fromDb)
        {
            ColumnId = fromDb.ColumnId;
            ColumnName = fromDb.ColumnName;
            DbObjectId = fromDb.DbObjectId;
            columnFlags = new EightFlags(fromDb.ColumnFlags);
            MaxLength = fromDb.Max_Length;
            Precision = fromDb.Precision;
            Scale = fromDb.Scale;
            UserTypeId = fromDb.User_Type_Id;
        }

        public DbColumnMetaData() { }

        public static DbColumnMetaData Create(string name, Type dataType, bool isKey, int? maxLength)
        {
            var metaData = new DbColumnMetaData {
                ColumnName = name,
                UserTypeId = SqlXTypeExtensions.NetTypeToSqlXType(dataType),
                IsNullable = dataType.CanBeNull(),
                IsPrimaryKey = isKey,
            };
            if (dataType == typeof(string)) {
                metaData.MaxLength = (short)(maxLength * 2 ?? SchemaReflection.SqlTypeInfo.VARCHARMAX_MAXLENGTH_FOR_SQLSERVER);
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
        public DbColumnId ColumnId { get; set; }

        public SqlXType UserTypeId { get; set; }
        public short MaxLength { get; set; } = SchemaReflection.SqlTypeInfo.VARCHARMAX_MAXLENGTH_FOR_SQLSERVER;
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        EightFlags columnFlags;

        public bool IsNullable
        {
            get
                => columnFlags[0];
            set
                => columnFlags[0] = value;
        }

        public bool IsComputed
        {
            get
                => columnFlags[1];
            set
                => columnFlags[1] = value;
        }

        public bool IsPrimaryKey
        {
            get
                => columnFlags[2];
            set
                => columnFlags[2] = value;
        }

        public bool HasAutoIncrementIdentity
        {
            get
                => columnFlags[3];
            set
                => columnFlags[3] = value;
        }

        public bool HasDefaultValue
        {
            get
                => columnFlags[4];
            set
                => columnFlags[4] = value;
        }

        public bool IsRowVersion
            => UserTypeId == SqlXType.RowVersion;

        public override string ToString()
            => ToStringByMembers.ToStringByPublicMembers(this);

        public SqlTypeInfo SqlTypeInfo()
            => new SqlTypeInfo(UserTypeId, MaxLength, Precision, Scale, IsNullable);

        public string ToSqlColumnDefinition()
            => $"{ColumnName} {SqlTypeInfo().ToSqlTypeName()}";

        public DataColumn ToDataColumn()
            => new DataColumn(ColumnName, UserTypeId.SqlUnderlyingTypeInfo().ClrType);

        static readonly ParameterizedSql tempDb = SQL($"tempdb");

        static ParameterizedSql BaseQuery(ParameterizedSql database)
            => SQL($@"
                with pks (object_id, column_id) as (
                    select i.object_id, ic.column_id
                    from sys.index_columns ic 
                    join sys.indexes i on ic.object_id = i.object_id and ic.index_id = i.index_id and i.is_primary_key = 1
                )
                select
                    ColumnName = c.name
                    , DbObjectId = c.object_id
                    , ColumnId = c.column_id
                    , c.user_type_id
                    , c.max_length
                    , c.precision
                    , c.scale
                    , ColumnFlags = convert(tinyint, 0
                        + 1*c.is_nullable 
                        + 2*c.is_computed
                        + 4*iif(pk.column_id is not null, convert(bit, 1), convert(bit, 0))
                        + 8*c.is_identity
                        + 16*iif(c.default_object_id is not null, convert(bit, 1), convert(bit, 0))
                        )
                from {database}{(database.IsEmpty ? ParameterizedSql.Empty : SQL($"."))}sys.columns c
                left join pks pk on pk.object_id = c.object_id and pk.column_id = c.column_id
                where 1=1
            ");

        public static DbColumnMetaData[] ColumnMetaDatas(SqlCommandCreationContext conn, ParameterizedSql objectName)
            => ColumnMetaDatas(conn, objectName.CommandText());

        public static DbColumnMetaData[] ColumnMetaDatas(SqlCommandCreationContext conn, string qualifiedObjectName)
        {
            if (qualifiedObjectName.StartsWith("#", StringComparison.OrdinalIgnoreCase)) {
                return BaseQuery(tempDb).Append(SQL($@"
                    and c.object_id = object_id({$"{tempDb.CommandText()}..{qualifiedObjectName}"})
                ")).ReadMetaObjects<CompressedSysColumnsValue>(conn).ArraySelect(v => new DbColumnMetaData(v));
            } else {
                return BaseQuery(ParameterizedSql.Empty).Append(SQL($@"
                    and c.object_id = object_id({qualifiedObjectName})
                ")).ReadMetaObjects<CompressedSysColumnsValue>(conn).ArraySelect(v => new DbColumnMetaData(v));
            }
        }

        public static Dictionary<DbObjectId, DbColumnMetaData[]> LoadAll(SqlCommandCreationContext conn)
            => BaseQuery(ParameterizedSql.Empty).ReadMetaObjects<CompressedSysColumnsValue>(conn)
                .ToGroupedDictionary(col => col.DbObjectId, (_, cols) => cols.Select(v => new DbColumnMetaData(v)).ToArray());

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
                .Where(md => md.IsPrimaryKey)
                .Select(md => $"{md.ColumnName}")
                .ToArray();
            // in een contained db mag er geen named PK worden gedefinieerd voor een temp. table
            // zolang er dus geen pk's over meerdere kolommen worden gedefinieerd gaat onderstaande ook goed voor temp. tables in een contained db
            var columnDefinitionSql = ParameterizedSql.CreateDynamic(columns
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
