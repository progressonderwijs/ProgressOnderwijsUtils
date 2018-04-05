using System.Threading.Tasks;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
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
