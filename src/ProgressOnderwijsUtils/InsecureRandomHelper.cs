using System;

namespace ProgressOnderwijsUtils
{
    public sealed class InsecureRandomHelper : RandomHelper
    {
        readonly Random random;

        public InsecureRandomHelper(int seed)
        {
            random = new Random(seed);
        }

        protected override byte[] GetBytes(int numBytes)
        {
            var bytes = new byte[numBytes];
            random.NextBytes(bytes);
            return bytes;
        }
    }
}
