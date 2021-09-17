using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils
{
    public static class HandlerUtils
    {
        /// <summary>
        /// Debounces an event handler (action).  The provided handler no earlier than "delay" after any call to the generated handler.
        /// An extra call to handler resets the clock (i.e. the delay is reset to the full amount on every input call).
        /// Only one output event can be called simultaneously, so if the handler takes longer than "delay" then the next call will start after the previous has ended.
        /// Only one such
        /// </summary>
        public static Action Debounce(TimeSpan delay, Action handler)
        {
            var sync = new object();
            // ReSharper disable once TooWideLocalVariableScope
            bool needsHandler;
            var totalNr = 0;
            return () => {
                var myNr = Interlocked.Increment(ref totalNr);
                _ = Task.Delay(delay).ContinueWith(
                    _ => {
                        var oldState = Interlocked.CompareExchange(ref totalNr, myNr, myNr);
                        if (oldState == myNr) {
                            needsHandler = true;

                            lock (sync) {
                                if (needsHandler) {
                                    needsHandler = false;
                                    handler();
                                }
                            }
                        }
                    });
            };
        }
    }
}
