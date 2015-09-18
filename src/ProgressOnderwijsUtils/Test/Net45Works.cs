using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
    [Continuous]
    public class Net45Works
    {
        [Test]
        public void UseAsyncKeywork()
        {
            Assert.AreEqual(42, Utils.F(async (int x) => await Task.FromResult(123 + x))(-81).Result);
        }
    }
}
