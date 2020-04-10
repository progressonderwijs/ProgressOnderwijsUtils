using System;

namespace ProgressOnderwijsUtils
{
    public static class DateTimeShortAgeTag
    {
        /// <summary>
        /// The purpose of an AgeTag is to provide an unlikely-to-collide unique token based on modification time.
        /// A tick is 10^-7 seconds; so Ticks>>23 ~ 0.84 seconds
        /// now, we take 0xffffff of those pseudo seconds, or 2^24 * 2^23 * 10^-7 seconds, i.e. 162.9 days, or about 0.5 years.
        /// after this time, the AgeTag will loop.  So our Tag is granular to the second, and unique within a half year.
        /// </summary>
        public static string ToAgeTag(DateTime datetime)
        {
            var granularTicks = (uint)(datetime.Ticks >> 23) & 0xffffff; //3byte code
            return Utils.ToSortableShortString(granularTicks);
        }
    }
}
