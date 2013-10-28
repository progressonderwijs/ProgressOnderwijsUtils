using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
	[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class ExcludeFromTestAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class ExcludeFromNCoverAttribute : CategoryAttribute
	{
	}
}
