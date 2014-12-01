using System;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ExcludeFromNCoverAttribute : CategoryAttribute { }
}
