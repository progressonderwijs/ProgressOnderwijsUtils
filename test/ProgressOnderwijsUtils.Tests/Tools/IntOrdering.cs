using Progress.Business.Tools;

namespace Progress.Business.Tests.Tools
{
    //For testing SortedSet.
    struct IntOrdering : IOrdering<int>
    {
        public bool LessThan(int a, int b) => a < b;
        public bool Equal(int a, int b) => a == b;
    }
}
