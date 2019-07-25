#nullable disable
namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed class ForeignKeyInfo : ValueBase<ForeignKeyInfo>, IWrittenImplicitly
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public override string ToString()
            => $"{TableName}.{ColumnName}";
    }
}
