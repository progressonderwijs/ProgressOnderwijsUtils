namespace ProgressOnderwijsUtils.Tests;

public sealed class DbQualifiedNameUtilsTest
{
    [Fact]
    public void SupportsUnqualifiedNames()
        => PAssert.That(() => "bla" == DbQualifiedNameUtils.UnqualifiedObjectName("bla"));

    [Fact]
    public void SupportsStripsSchemaWhenPresent()
        => PAssert.That(() => "dtproperties" == DbQualifiedNameUtils.UnqualifiedObjectName("dbo.dtproperties"));

    [Fact]
    public void SchemaFromQualifiedNameReturnsOnlySchema()
        => PAssert.That(() => "dbo" == DbQualifiedNameUtils.SchemaFromQualifiedName("dbo.dtproperties"));

    [Fact]
    public void SchemaFromQualifiedNameCrashesOnUnqualifiedName()
        => Assert.ThrowsAny<Exception>(() => DbQualifiedNameUtils.SchemaFromQualifiedName("bla"));

    [Fact]
    public void Qualified_to_unqualified()
    {
        var qualified = DbQualifiedNameUtils.QualifiedObjectName("dbo", "dtproperties");
        var unqualified = DbQualifiedNameUtils.UnqualifiedObjectName(qualified);

        PAssert.That(() => unqualified == "dtproperties");
    }
}
