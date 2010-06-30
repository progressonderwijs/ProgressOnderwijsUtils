using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class NightlyOnlyAttribute : CategoryAttribute
	{
	}
}
