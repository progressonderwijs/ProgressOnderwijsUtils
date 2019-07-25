#nullable disable
using System;
using System.Diagnostics;

namespace ProgressOnderwijsUtils
{
    public static class WorkAroundResharperSuckiness
    {
        //NOT [Pure] so resharper shuts up; the aim of this method is to make resharper
        //shut up about "Parameter 'Foobaar' is used only for precondition checks"
        [DebuggerHidden]
        public static void ThrowPreconditionViolation(this Exception e)
            => throw e;
    }
}
