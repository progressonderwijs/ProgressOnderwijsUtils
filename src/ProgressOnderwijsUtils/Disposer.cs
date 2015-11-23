using System;

namespace ProgressOnderwijsUtils
{
    public sealed class Disposer : IDisposable
    {
        readonly Action action;

        public Disposer(Action action)
        {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }
            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }
}
