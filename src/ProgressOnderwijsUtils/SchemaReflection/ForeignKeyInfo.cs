namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed class ForeignKeyInfo : ValueBase<ForeignKeyInfo>, IWrittenImplicitly
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public string TableName { get; set; }
        public string ColumnName { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

        public override string ToString()
            => $"{TableName}.{ColumnName}";
    }
}
