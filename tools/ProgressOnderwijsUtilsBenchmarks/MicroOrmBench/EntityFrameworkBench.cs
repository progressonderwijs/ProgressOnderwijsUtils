using Microsoft.EntityFrameworkCore;

namespace ProgressOnderwijsUtilsBenchmarks.MicroOrmBench;

sealed class EntityFrameworkBench : DbContext
{
    public DbSet<WideExampleObject> WideExampleObjects
        => Set<WideExampleObject>();

    public DbSet<ExampleObject> ExampleObjects
        => Set<ExampleObject>();

    public EntityFrameworkBench(SqlConnection conn)
        : base(
            new DbContextOptionsBuilder<EntityFrameworkBench>()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSqlServer(conn)
                .Options
        ) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<ExampleObject>().HasNoKey();
        _ = modelBuilder.Entity<WideExampleObject>().HasNoKey();
    }

    public static void RunQuery(Benchmarker benchmarker)
    {
        benchmarker.BenchSqlServer(
            "EF FromSqlInterpolated (fresh contexts)",
            (sqlConn, rows) =>
                new EntityFrameworkBench(sqlConn).ExampleObjects.FromSqlInterpolated(ExampleObject.InterpolatedQuery(rows)).ToArray().Length
        );
        benchmarker.BenchEFSqlServer(
            "EF FromSqlInterpolated (reused context)",
            (ctx, rows) =>
                ctx.ExampleObjects.FromSqlInterpolated(ExampleObject.InterpolatedQuery(rows)).ToArray().Length
        );
    }

    public static void RunWideQuery(Benchmarker benchmarker)
    {
        benchmarker.BenchSqlServer(
            "EF FromSqlInterpolated (26-col, fresh contexts)",
            (sqlConn, rows) =>
                new EntityFrameworkBench(sqlConn).WideExampleObjects.FromSqlInterpolated(WideExampleObject.InterpolatedQuery(rows)).ToArray().Length
        );
        benchmarker.BenchEFSqlServer(
            "EF FromSqlInterpolated (26-col, reused context)",
            (ctx, rows) =>
                ctx.WideExampleObjects.FromSqlInterpolated(WideExampleObject.InterpolatedQuery(rows)).ToArray().Length
        );
    }
}
