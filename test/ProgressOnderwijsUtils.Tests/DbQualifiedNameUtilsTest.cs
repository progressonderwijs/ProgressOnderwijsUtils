using System;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;

namespace ProgressOnderwijsUtils.Tests;

public sealed class DbQualifiedNameUtilsTest
{
    [Fact]
    public void SupportsUnqualifiedNames()
        => PAssert.That(() => "bla" == DbQualifiedNameUtils.UnqualifiedTableName("bla"));

    [Fact]
    public void SupportsStripsSchemaWhenPresent()
        => PAssert.That(() => "dtproperties" == DbQualifiedNameUtils.UnqualifiedTableName("dbo.dtproperties"));

    [Fact]
    public void SchemaFromQualifiedNameReturnsOnlySchema()
        => PAssert.That(() => "dbo" == DbQualifiedNameUtils.SchemaFromQualifiedName("dbo.dtproperties"));

    [Fact]
    public void SchemaFromQualifiedNameCrashesOnUnqualifiedName()
        => Assert.ThrowsAny<Exception>(() => DbQualifiedNameUtils.SchemaFromQualifiedName("bla"));
}