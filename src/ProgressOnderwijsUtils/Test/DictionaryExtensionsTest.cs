using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils.Test
{
	[Continuous]
	public class DictionaryExtensionsTest
	{
		public class MergeShould
		{
			[Test, Continuous]
			public void ReturnTheSameDictionaryWhenMergingWithEmpty()
			{
				var original = new Dictionary<int, string>(){
					{1, "foo"},
					{2, "bar"}
				};
				var empty =  new Dictionary<int, string>();
				PAssert.That(()=> original.Merge(empty).SequenceEqual(original));
			}

			[Test, Continuous]
			public void ReturnsContentOfBothDictionaries()
			{
				var first = new Dictionary<int, string>(){
					{1, "foo"},
				};
				var second = new Dictionary<int, string>(){
					{2, "bar"}
				};
				var combined = new Dictionary<int, string>() { 
					{1, "foo"},
					{2, "bar"}
				};
				PAssert.That(() => first.Merge(second).SequenceEqual(combined));
			}

			[Test, Continuous]
			public void ReturnsValueOfLastDictionaryWhenBothDictionariesContainSameKey()
			{
				var first = new Dictionary<int, string>(){
					{2, "foo"},
				};
				var second = new Dictionary<int, string>(){
					{2, "bar"}
				};
				var combined = new Dictionary<int, string>() { 
					{2, "bar"}
				};
				PAssert.That(() => first.Merge(second).SequenceEqual(combined));
			}

			[Test, Continuous]
			public void ReturnsTheResultOfMultipleMergedArrays()
			{
				var first = new Dictionary<int, string>(){
					{1, "foo"},
				};
				var second = new Dictionary<int, string>(){
					{2, "bar"}
				};
				var third = new Dictionary<int, string>(){
					{3, "baz"}
				};
				var combined = new Dictionary<int, string>() { 
					{1, "foo"},
					{2, "bar"},
					{3, "baz"}
				};
				PAssert.That(() => first.Merge(second, third).SequenceEqual(combined));
			}


		}
	}
}
