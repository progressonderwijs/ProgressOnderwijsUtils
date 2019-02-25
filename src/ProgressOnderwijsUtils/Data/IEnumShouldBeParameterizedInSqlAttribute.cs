namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// If an enum is annotated with an attribute [MyCustomAttr("example")],
    /// and the attributes implements this marker interface (IEnumShouldBeParameterizedInSqlAttribute),
    /// then when sql is generated enum values are not represented as literals but parameterized (the way an int or long might be)
    /// </summary>
    public interface IEnumShouldBeParameterizedInSqlAttribute { }
}
