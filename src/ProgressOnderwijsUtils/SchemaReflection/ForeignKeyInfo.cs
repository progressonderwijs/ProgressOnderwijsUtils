namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed class ForeignKeyInfo : ValueBase<ForeignKeyInfo>, IMetaObject
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public override string ToString()
            => $"{TableName}.{ColumnName}";
    }
}
