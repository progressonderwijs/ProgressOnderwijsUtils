using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	class ServerContextTest
	{
		[Test]
		public void CheckBasics()
		{
#pragma warning disable 612,618
			PAssert.That(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, DatabaseVersion.Test2DB) != ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, DatabaseVersion.TestDB));
			Assert.Throws<ArgumentException>(() => ServerContext.ConstructExplicitly(default(ServerLocation), DatabaseVersion.Test2DB));
			Assert.Throws<ArgumentException>(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, default(DatabaseVersion)));
			Assert.Throws<ArgumentException>(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, default(DatabaseVersion)));

			Assert.Throws<ArgumentException>(() => default(DatabaseVersion).ToIdentifier() );
			
			PAssert.That(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, DatabaseVersion.Test2DB).ToString() == "MachineInDMZ/Test2DB");

			var versions=Enum.GetValues(typeof(DatabaseVersion)).Cast<DatabaseVersion>().Where(dbver=>dbver!=DatabaseVersion.Undefined);
			PAssert.That(() => versions.All(dbVer => dbVer == DatabaseVersionIdentifiers.DatabaseVersionFromIdentifier(dbVer.ToIdentifier())));
			PAssert.That(() => DatabaseVersionIdentifiers.DatabaseVersionFromIdentifier(null) == DatabaseVersion.Undefined && DatabaseVersionIdentifiers.DatabaseVersionFromIdentifier("xyz") == DatabaseVersion.Undefined);
#pragma warning restore 612,618
		}
	}
}
