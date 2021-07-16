namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed record ForeignKeyInfo( string TableName , string ColumnName) : IWrittenImplicitly
    {
        public override string ToString()
            => $"{TableName}.{ColumnName}";
    }
}
