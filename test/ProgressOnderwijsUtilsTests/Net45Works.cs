﻿using System.Threading.Tasks;
using ProgressOnderwijsUtils;
using Xunit;

namespace ProgressOnderwijsUtilsTests
{
    public class Net45Works
    {
        [Fact]
        public void UseAsyncKeywork()
        {
            Assert.Equal(42, Utils.F(async (int x) => await Task.FromResult(123 + x))(-81).Result);
        }
    }
}
