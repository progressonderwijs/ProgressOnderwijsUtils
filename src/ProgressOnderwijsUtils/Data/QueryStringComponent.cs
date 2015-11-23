using System;

namespace ProgressOnderwijsUtils
{
    sealed class QueryStringComponent : IBuildableQuery
    {
        public readonly string val;

        internal QueryStringComponent(string val)
        {
            if (val == null) {
                throw new ArgumentNullException(nameof(val));
            }
            this.val = val;
        }

        public void AppendTo(ref CommandFactory factory)
        {
            factory.AppendSql(val, 0, val.Length);
        }

        public int EstimateLength()
        {
            return val.Length;
        }
    }
}
