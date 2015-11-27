﻿using System;

namespace ProgressOnderwijsUtils
{
    sealed class QueryStringComponent : IQueryComponent
    {
        public readonly string val;
        public string ToSqlString(CommandFactory qnum) => val;

        internal QueryStringComponent(string val)
        {
            if (val == null) {
                throw new ArgumentNullException(nameof(val));
            }
            this.val = val;
        }

        public string ToDebugText() => val;
        public bool Equals(IQueryComponent other) => (other is QueryStringComponent) && val == ((QueryStringComponent)other).val;
        public override bool Equals(object obj) => (obj is QueryStringComponent) && Equals((QueryStringComponent)obj);
        public override int GetHashCode() => val.GetHashCode() + 31;
    }
}
