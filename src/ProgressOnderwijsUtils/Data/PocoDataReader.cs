using Microsoft.EntityFrameworkCore.Query;

namespace ProgressOnderwijsUtils;

public interface IOptionalObjectListForDebugging
{
    IReadOnlyList<object>? ContentsForDebuggingOrNull();
}

public interface IOptionalObjectProjectionForDebugging
{
    object? ProjectionForDebuggingOrNull();
}

public sealed class PocoDataReader<T> : DbDataReaderBase, IOptionalObjectListForDebugging
    where T : IReadImplicitly
{
    readonly struct Optional
    {
        public readonly bool HasValue;
        readonly T Value;

        public Optional(T value)
        {
            HasValue = true;
            Value = value;
        }

        public static readonly Optional Empty = new();

        public T GetValue()
            => HasValue ? Value : throw new InvalidOperationException("Has no value");
    }

    readonly CancellationToken _cancellationToken;
    readonly IEnumerator<T> pocos;
    readonly IReadOnlyList<T>? objectsOrNull_ForDebugging;
    Optional current;
    public int RowsProcessed { get; private set; }

    public PocoDataReader(IEnumerable<T> objects, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        pocos = objects.GetEnumerator();
        objectsOrNull_ForDebugging = objects as IReadOnlyList<T>;
        current = Optional.Empty;
    }

    public override void Close()
    {
        pocos.Dispose();
        isClosed = true;
        current = Optional.Empty;
    }

    protected override bool ReadImpl()
    {
        _cancellationToken.ThrowIfCancellationRequested();
        var hasnext = pocos.MoveNext();
        if (hasnext) {
            current = new(pocos.Current);
            RowsProcessed++;
        } else {
            current = Optional.Empty;
        }
        return hasnext;
    }

    public override DataTable GetSchemaTable()
        => schemaTable;

    public override int FieldCount
        => columnInfos.Length;

    public override Type GetFieldType(int ordinal)
        => columnInfos[ordinal].ColumnType;

    public override string GetName(int ordinal)
        => columnInfos[ordinal].Name;

    public override int GetOrdinal(string name)
        => columnIndexByName[name];

    public override int GetInt32(int ordinal)
        => ((Func<T, int>)columnInfos[ordinal].TypedNonNullableGetter)(current.GetValue());

    public override object GetValue(int ordinal)
        => columnInfos[ordinal].GetUntypedColumnValue(current.GetValue()) ?? DBNull.Value;

    public override bool IsDBNull(int ordinal)
    {
        var func = columnInfos[ordinal].WhenNullable_IsColumnDBNull;
        return func != null && func(current.GetValue());
    }

    public override TColumn GetFieldValue<TColumn>(int ordinal)
        => columnInfos[ordinal].TypedNonNullableGetter is Func<T, TColumn> getter
            ? getter(current.GetValue())
            : throw new InvalidOperationException($"Tried to access field {columnInfos[ordinal].Name} of type {columnInfos[ordinal].ColumnType.ToCSharpFriendlyTypeName()} as type {typeof(TColumn).ToCSharpFriendlyTypeName()}.");

    readonly struct ColumnInfo
    {
        public readonly string Name;

        //ColumnType is non nullable with enum types replaced by their underlying type
        public readonly Type ColumnType;
        public readonly Func<T, object?> GetUntypedColumnValue;

        //WhenNullable_IsColumnDBNull is itself null if column non-nullable
        public readonly Func<T, bool>? WhenNullable_IsColumnDBNull;

        //TypedNonNullableGetter is of type Func<T, _> such that typeof(_) == ColumnType - therefore cannot return nulls!
        public readonly Delegate TypedNonNullableGetter;

        public ColumnInfo(IPocoProperty<T> pocoProperty)
        {
            var propertyType = pocoProperty.DataType;
            var pocoParameter = Expression.Parameter(typeof(T));
            var propertyValue = pocoProperty.PropertyAccessExpression(pocoParameter);
            Name = pocoProperty.Name;
            var propertyConverter = AutomaticValueConverters.GetOrNull(propertyType.GetNonNullableType());
            var isNonNullable = propertyType.IsValueType && propertyType.IfNullableGetNonNullableType() == null;

            if (propertyConverter != null) {
                ColumnType = propertyConverter.ProviderClrType;
                var propertyValueAsNoNullable = Expression.Convert(propertyValue, propertyConverter.ModelClrType);
                var columnValueAsNonNullable = ReplacingExpressionVisitor.Replace(propertyConverter.ConvertToProviderExpression.Parameters.Single(), propertyValueAsNoNullable, propertyConverter.ConvertToProviderExpression.Body);
                var columnBoxedAsObject = Expression.Convert(columnValueAsNonNullable, typeof(object));
                Expression columnBoxedAsColumnType;
                if (isNonNullable) {
                    columnBoxedAsColumnType = columnBoxedAsObject;
                    WhenNullable_IsColumnDBNull = null;
                } else {
                    var propertyIsNotNull = AutomaticValueConverters.IsExpressionNonNull(propertyValue);
                    columnBoxedAsColumnType = Expression.Condition(propertyIsNotNull, columnBoxedAsObject, Expression.Constant(DBNull.Value, typeof(object)));
                    WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(Expression.Not(propertyIsNotNull), pocoParameter).CompileFast();
                }

                GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, pocoParameter).Compile();
                TypedNonNullableGetter = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), propertyConverter.ProviderClrType), columnValueAsNonNullable, pocoParameter).CompileFast();
            } else {
                ColumnType = propertyType.GetNonNullableUnderlyingType();
                var propertyValueAsNoNullable = Expression.Convert(propertyValue, ColumnType);
                var columnValueAsNonNullable = Expression.Convert(propertyValue, propertyType.GetUnderlyingType());
                var columnBoxedAsObject = Expression.Convert(columnValueAsNonNullable, typeof(object));
                Expression columnBoxedAsColumnType;
                if (isNonNullable) {
                    columnBoxedAsColumnType = columnBoxedAsObject;
                    WhenNullable_IsColumnDBNull = null;
                } else {
                    var propertyIsNotNull = AutomaticValueConverters.IsExpressionNonNull(propertyValue);
                    columnBoxedAsColumnType = Expression.Coalesce(columnValueAsNonNullable, Expression.Constant(DBNull.Value, typeof(object)));
                    WhenNullable_IsColumnDBNull = Expression.Lambda<Func<T, bool>>(Expression.Not(propertyIsNotNull), pocoParameter).CompileFast();
                }
                GetUntypedColumnValue = Expression.Lambda<Func<T, object>>(columnBoxedAsColumnType, pocoParameter).Compile();
                TypedNonNullableGetter = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), ColumnType), propertyValueAsNoNullable, pocoParameter).CompileFast();
            }
        }
    }

    static readonly ColumnInfo[] columnInfos;
    static readonly Dictionary<string, int> columnIndexByName;
    static readonly DataTable schemaTable;

    static PocoDataReader()
    {
        var properties = PocoUtils.GetProperties<T>();
        var columnInfosBuilder = new List<ColumnInfo>(properties.Count);
        columnIndexByName = new(properties.Count, StringComparer.OrdinalIgnoreCase);
        schemaTable = CreateEmptySchemaTable();
        var i = 0;
        foreach (var pocoProperty in properties) {
            if (pocoProperty.CanRead) {
                var columnInfo = new ColumnInfo(pocoProperty);
                var isKey = pocoProperty.IsKey;
                var allowDbNull = columnInfo.WhenNullable_IsColumnDBNull != null;
                var isUnique = isKey && properties.None(other => other != pocoProperty && other.IsKey);
                columnIndexByName.Add(columnInfo.Name, i);
                _ = schemaTable.Rows.Add(
                    columnInfo.Name,
                    i,
                    -1,
                    null,
                    null,
                    columnInfo.ColumnType,
                    null,
                    false,
                    allowDbNull,
                    true,
                    false,
                    isUnique,
                    isKey,
                    false,
                    null,
                    null,
                    null,
                    "val"
                );
                columnInfosBuilder.Add(columnInfo);
                i++;
            }
        }
        columnInfos = columnInfosBuilder.ToArray();
    }

    static DataTable CreateEmptySchemaTable()
    {
        var dt = new DataTable();
        dt.Columns.AddRange(
            new[] {
                new DataColumn("ColumnName", typeof(string)),
                new DataColumn("ColumnOrdinal", typeof(int)),
                new DataColumn("ColumnSize", typeof(int)),
                new DataColumn("NumericPrecision", typeof(short)),
                new DataColumn("NumericScale", typeof(short)),
                new DataColumn("DataType", typeof(Type)),
                new DataColumn("ProviderType", typeof(int)),
                new DataColumn("IsLong", typeof(bool)),
                new DataColumn("AllowDBNull", typeof(bool)),
                new DataColumn("IsReadOnly", typeof(bool)),
                new DataColumn("IsRowVersion", typeof(bool)),
                new DataColumn("IsUnique", typeof(bool)),
                new DataColumn("IsKey", typeof(bool)),
                new DataColumn("IsAutoIncrement", typeof(bool)),
                new DataColumn("BaseCatalogName", typeof(string)),
                new DataColumn("BaseSchemaName", typeof(string)),
                new DataColumn("BaseTableName", typeof(string)),
                new DataColumn("BaseColumnName", typeof(string)),
            }
        );
        return dt;
    }

    IReadOnlyList<object>? IOptionalObjectListForDebugging.ContentsForDebuggingOrNull()
        => objectsOrNull_ForDebugging?.SelectIndexable(o => (o as IOptionalObjectProjectionForDebugging)?.ProjectionForDebuggingOrNull() ?? o);
}
