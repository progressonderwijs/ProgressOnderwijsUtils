using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace ProgressOnderwijsUtils
{
    public static class MetaObjectProposalLogger
    {
        static readonly ConcurrentDictionary<string, string> metaObjectProposals = new ConcurrentDictionary<string, string>();
        static readonly object sync = new object();
        static Action<string> writer;

        public static void RedirectLoggingToConsole() { writer = Console.Out.WriteLine; }

        [Conditional("DEBUG")]
        public static void LogMetaObjectProposal(SqlCommand command, DataTable dt, QueryTracer tracer)
        {
            var commandText = QueryTracer.DebugFriendlyCommandText(command, false);
            bool wasAdded = false;

            var metaObjectClass = metaObjectProposals.GetOrAdd(
                commandText,
                _ => {
                    wasAdded = true;
                    return dt.DataTableToMetaObjectClassDef();
                });
            if (wasAdded) {
                Log("=======================\r\n" + commandText + "\r\n\r\n" + metaObjectClass + "\r\n\r\n");
            }
            tracer.FinishDisposableTimer(() => "METAOBJECT proposed for next query:\n" + metaObjectClass, TimeSpan.Zero);
        }

        static void Log(string text)
        {
            lock (sync) {
                if (writer == null) {
                    try {
                        writer = new StreamWriter(string.Format(@"C:\\temp\\MetaObjectProposals_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd_HHmm_ss"))) {
                            AutoFlush = true
                        }.Write;
                    } catch {
                        writer = s => { };
                    }
                }
                writer(text);
            }
        }
    }
}