namespace ProgressOnderwijsUtils.SchemaReflection;

[AttributeUsage(AttributeTargets.Enum)]
public sealed class DbIdEnumAttribute : Attribute, IEnumShouldBeParameterizedInSqlAttribute { }
