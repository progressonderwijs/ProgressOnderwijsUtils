using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Tests.Metrics;

public sealed class SqlClientDbNamespaceTest
{
    static TestDatabase? Classify(string dbNamespace)
        => SqlClientDbNamespace.Classify(dbNamespace, catalog => catalog switch { "Alpha" => TestDatabase.Alpha, "Beta.Archive" => TestDatabase.Beta, _ => (TestDatabase?)null, });

    [Fact]
    public void Without_an_instance_the_tag_is_the_database()
        => PAssert.That(() => Classify("Alpha") == TestDatabase.Alpha);

    [Fact]
    public void A_named_instance_precedes_the_database()
        => PAssert.That(() => Classify("MSSQLLocalDB.Alpha") == TestDatabase.Alpha);

    [Fact]
    public void A_database_with_a_dot_in_its_name_is_recognised_with_and_without_an_instance()
        => PAssert.That(() => Classify("Beta.Archive") == TestDatabase.Beta && Classify("SQL2019.Beta.Archive") == TestDatabase.Beta);

    [Fact]
    public void An_unknown_database_names_none()
        => PAssert.That(() => Classify("SQL2019.master") == null && Classify("master") == null);
}
