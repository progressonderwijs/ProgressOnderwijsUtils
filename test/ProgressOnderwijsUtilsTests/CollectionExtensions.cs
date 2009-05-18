using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProgressOnderwijsUtils.Extensions;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class CollectionExtensions
	{
		[Test]
		public void IDictionary_GetValueOrDefault()
		{
			IDictionary<int, int> sut = new Dictionary<int, int>() { { 0, 0 } };
			Assert.That(sut.GetValueOrDefault(0, 1), Is.EqualTo(0));
			Assert.That(sut.GetValueOrDefault(1, 2), Is.EqualTo(2));
		}
	}
}
