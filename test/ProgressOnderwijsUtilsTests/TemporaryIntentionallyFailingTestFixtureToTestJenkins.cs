
#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProgressOnderwijsUtilsTests
{
	public class TemporaryIntentionallyFailingTestFixtureToTestJenkins1
	{
		[TestFixtureSetUp]
		public void TFSU() {
			throw new Exception("Intentional failure to check jenkins.");
		}
		[Test]
		public void ATest() { 
		
		}
	}

	public class TemporaryIntentionallyFailingTestFixtureToTestJenkins2
	{
		[TestFixtureTearDown]
		public void TFTD()
		{
			throw new Exception("Intentional failure to check jenkins.");
		}
		[Test]
		public void ATest()
		{

		}
	}

	public class TemporaryIntentionallyFailingTestFixtureToTestJenkins3
	{
		[SetUp]
		public void SetUp()
		{
			throw new Exception("Intentional failure to check jenkins.");
		}
		[Test]
		public void ATest()
		{

		}
	}

	public class TemporaryIntentionallyFailingTestFixtureToTestJenkins4
	{
		[TearDown]
		public void TearDown()
		{
			throw new Exception("Intentional failure to check jenkins.");
		}
		[Test]
		public void ATest()
		{

		}
	}
}
#endif
