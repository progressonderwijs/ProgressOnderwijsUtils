using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class NightlyOnlyAttribute : CategoryAttribute { }
}
