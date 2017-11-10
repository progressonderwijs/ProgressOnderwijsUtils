using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class HashCodeHelper
    {
        public static int ComputeHash([NotNull] params object[] obj)
        {
            ulong res = 0;
            for (uint i = 0; i < obj.Length; i++) {
                var val = obj[i];
                res += val == null ? i : (ulong)val.GetHashCode() * (1 + 2 * i);
            }
            return (int)(uint)(res ^ res >> 32);
        }
    }
}
