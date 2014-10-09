using System.Threading;
using NUnit.Framework;


[SetUpFixture]
// ReSharper disable CheckNamespace
public sealed class RegisterTestingProgressTools //may not be in a namespace!
	//nunit runs these setup/teardowns around all tests within that namespace - i.e. everything for the top-level root namespace.
// ReSharper restore CheckNamespace
{
	static int testsLoaded;
	public static bool IsInUnitTest { get { return testsLoaded > 0; } }

	[SetUp]
	public static void BeforeTests()
	{
		Interlocked.Increment(ref testsLoaded);
	}
	[TearDown]
	public static void AfterTests()
	{
		Interlocked.Decrement(ref testsLoaded);
	}
}