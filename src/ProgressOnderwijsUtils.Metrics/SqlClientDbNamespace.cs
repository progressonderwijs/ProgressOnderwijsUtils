namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Reads the database from the <c>db.namespace</c> tag of the OpenTelemetry SqlClient instrumentation, for use in the
/// <see cref="MetricsDimension{T}"/> of the database durations. That tag is not simply the database: when the data
/// source names an instance, as <c>(localdb)\MSSQLLocalDB</c> or <c>server\SQL2019</c> do, the instrumentation
/// writes <c>{instance}.{database}</c>, so <c>MSSQLLocalDB.tempdb</c>. Reading the tag as the database then
/// silently counts every measurement on the other slot.
/// </summary>
public static class SqlClientDbNamespace
{
    /// <summary>
    /// The database a <c>db.namespace</c> tag names, according to <paramref name="classifyDatabase"/>, or null when it
    /// names none of them. The tag cannot tell whether it starts with an instance, because a database name may itself
    /// contain a dot; so the whole tag is tried first and only then the part after the first dot (instance names
    /// contain no dot).
    /// </summary>
    public static T? Classify<T>(string dbNamespace, Func<string, T?> classifyDatabase)
        where T : struct
        => classifyDatabase(dbNamespace)
            ?? (dbNamespace.IndexOf('.', StringComparison.Ordinal) is var dot and >= 0 ? classifyDatabase(dbNamespace[(dot + 1)..]) : null);
}
