﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public class SelfSanityCheck
    {
        [Fact]
        public void AllTestsAreRunnable()
        {
            var problems =
                from type in typeof(SelfSanityCheck).Assembly.GetTypes()
                from method in
                type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                BindingFlags.NonPublic)
                where method.GetCustomAttributes<FactAttribute>().Any()
                where !type.IsPublic || !method.IsPublic
                select type.Name + "." + method.Name;

            PAssert.That(() => !problems.Any());
        }
    }
}
