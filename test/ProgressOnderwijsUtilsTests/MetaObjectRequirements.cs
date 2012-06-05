using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public sealed class MetaObjectRequirements
	{

		[Test]
		public void MetaObjectsAreSealedOrAbstract()
		{
			var metaObjectTypes =
				from exampleType in new[] {
// ReSharper disable RedundantNameQualifier
					typeof(ProgressOnderwijsUtils.DeepEquals), 
					typeof(Progress.Business.BusinessConnection),
					typeof(Progress.WebApp.RequestVars),
					typeof(Progress.Gadgets.Context),
					typeof(Progress.Services.Global), 
					typeof(Progress.WebFramework.FrameworkSession), 
					typeof(ProgressOnderwijsUtilsTests.MetaObjectRequirements) 
// ReSharper restore RedundantNameQualifier
				}
				let assembly = Assembly.GetAssembly(exampleType)
				from type in assembly.GetTypes()
				where typeof(IMetaObject).IsAssignableFrom(type)
				select type;

			var unsealedNonAbstractMetaObjectTypes = metaObjectTypes.Where(type => !type.IsSealed && !type.IsAbstract);

			PAssert.That(() => !unsealedNonAbstractMetaObjectTypes.Any(),
				"MetaObject types must be sealed or abstract.  Reason: metaproperties can be resolved using ANY of the concrete types of the metaobject, so that inheritance will can cause subclass instances' properties to be omitted."
				);

		}
	}
}
