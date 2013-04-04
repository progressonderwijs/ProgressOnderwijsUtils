using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Test.CodeStyle;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[Continuous]
	public sealed class MetaObjectRequirements
	{
		[Test]
		public void MetaObjectsAreAbstractOrNotInherited()
		{
			var metaObjectTypes =
				from assembly in ClassNameConflicts.ProgressAssemblies
				from type in assembly.GetTypes()
				where typeof(IMetaObject).IsAssignableFrom(type)
				select type;

			var typesWithNonAbstractBaseMetaObjects = metaObjectTypes.Where(type => !type.IsAbstract && type.BaseTypes().Any(baseT => !baseT.IsAbstract && typeof(IMetaObject).IsAssignableFrom(baseT)));

			PAssert.That(() => !typesWithNonAbstractBaseMetaObjects.Any(),
				"MetaObject types must not be inherited (unless they're abstract).  Reason: metaproperties can be resolved using ANY of the concrete types of the metaobject, so that inheritance will cause subclass instances' properties to be omitted."
				);

		}
	}
}
