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
	public class PodTest
	{
		public static void ComparePod(object a, object b){
			
			Type aType= a.GetType();
			Type bType=b.GetType();
			PAssert.That(()=>aType.GetProperties().Select(pi=>pi.Name).OrderBy(s=>s).SequenceEqual(bType.GetProperties().Select(pi=>pi.Name).OrderBy(s=>s)));
			
			object[] empty = new object[0];

			PAssert.That(
				()=>
					!(from aProp in aType.GetProperties()
					join bProp in bType.GetProperties() on aProp.Name equals bProp.Name
					where !Equals(aProp.GetValue(a,empty),bProp.GetValue(b,empty))
					select aProp.Name).Any());
		}
		[Test]
		public void SanityCheck()
		{
			Assert.Throws<PAssertFailedException>(()=>ComparePod(new LabelCode("abc", false), new { waarde = "abc", Vrijveld = false }));//case-sensitive
			Assert.Throws<PAssertFailedException>(() => ComparePod(new LabelCode("abc", false), new { Waarde = "Abc", Vrijveld = false }));//value-sensitive
			Assert.Throws<PAssertFailedException>(() => ComparePod(new LabelCode(null, false), new { Waarde = "", Vrijveld = false }));//no weirdness
		}

		[Test]
		public void LabelCodeTest()
		{
			ComparePod(new LabelCode("abc", false), new { Waarde = "abc", Vrijveld = false });
			ComparePod(new LabelCode(null, true), new { Waarde = default(string), Vrijveld = true });
			ComparePod(new LabelCode("", true), new { Waarde = "", Vrijveld = true });
		}
	}
}
