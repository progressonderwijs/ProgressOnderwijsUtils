using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Tests.Collections
{
    //For testing SortedSet.
    struct IntOrdering : IOrdering<int>
    {
        public bool LessThan(int a, int b)
            => a < b;
    }
}
