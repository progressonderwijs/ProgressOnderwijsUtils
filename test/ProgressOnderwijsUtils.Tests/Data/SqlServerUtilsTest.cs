using Xunit;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class SqlServerUtilsTest : TransactedLocalConnection
{
    [Fact]
    public void KillOtherUserProcessesOnDb_works_on_non_existing_catalog()
        => SqlServerUtils.KillOtherUserProcessesOnDb(Connection, nameof(KillOtherUserProcessesOnDb_works_on_non_existing_catalog));
}