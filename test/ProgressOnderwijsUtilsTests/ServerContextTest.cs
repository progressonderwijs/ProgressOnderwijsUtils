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
	public class ServerContextTest
	{
		[Test]
		public void CheckBasics()
		{
#pragma warning disable 612,618
			//intentionally use default(EnumType) to verify that using unintialized enums cases exceptions.
			PAssert.That(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, DatabaseVersion.DevTestDB) != ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, DatabaseVersion.TestDB));
			Assert.Throws<ArgumentException>(() => ServerContext.ConstructExplicitly(default(ServerLocation), DatabaseVersion.DevTestDB));
			Assert.Throws<ArgumentException>(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, default(DatabaseVersion)));
			Assert.Throws<ArgumentException>(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, default(DatabaseVersion)));

			Assert.Throws<ArgumentException>(() => default(DatabaseVersion).ToIdentifier() );
			
			PAssert.That(() => ServerContext.ConstructExplicitly(ServerLocation.MachineInDMZ, DatabaseVersion.DevTestDB).ToString() == "MachineInDMZ/DevTestDB");

			var versions=Enum.GetValues(typeof(DatabaseVersion)).Cast<DatabaseVersion>().Where(dbver=>dbver!=DatabaseVersion.Undefined);
			PAssert.That(() => versions.All(dbVer => dbVer == DatabaseVersionIdentifiers.DatabaseVersionFromIdentifier(dbVer.ToIdentifier())));
			PAssert.That(() => DatabaseVersionIdentifiers.DatabaseVersionFromIdentifier(null) == DatabaseVersion.Undefined && DatabaseVersionIdentifiers.DatabaseVersionFromIdentifier("xyz") == DatabaseVersion.Undefined);
#pragma warning restore 612,618
		}
	}
}
