using System.Threading;
using NUnit.Framework;

[SetUpFixture]
// ReSharper disable CheckNamespace
public sealed class RegisterTestingProgressTools //may not be in a namespace!
    //nunit runs these setup/teardowns around all tests within that namespace - i.e. everything for the top-level root namespace.
    // ReSharper restore CheckNamespace
{
    static int testsLoaded;
    public static bool IsInUnitTest => testsLoaded > 0;

    [OneTimeSetUp]
    public static void BeforeTests()
    {
        Interlocked.Increment(ref testsLoaded);
    }

    [OneTimeTearDown]
    public static void AfterTests()
    {
        Interlocked.Decrement(ref testsLoaded);
    }
}
