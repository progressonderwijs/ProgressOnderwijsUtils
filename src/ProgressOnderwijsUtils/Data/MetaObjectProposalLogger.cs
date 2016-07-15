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
        public static bool LogProposals = true;
        static readonly ConcurrentDictionary<string, string> metaObjectProposals = new ConcurrentDictionary<string, string>();
        static readonly object sync = new object();
        static Action<string> writer;

        [Conditional("DEBUG")]
        public static void LogMetaObjectProposal(SqlCommand command, DataTable dt, ISqlCommandTracer tracer)
        {
            if (!LogProposals)
                return;
            var commandText = SqlCommandTracer.DebugFriendlyCommandText(command, SqlCommandTracerOptions.ExcludeArgumentValuesFromLog);
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
            tracer?.FinishDisposableTimer(() => "METAOBJECT proposed for next query:\n" + metaObjectClass, TimeSpan.Zero);
        }

        static void Log(string text)
        {
            lock (sync) {
                if (writer == null) {
                    try {
                        if (Process.GetCurrentProcess().ProcessName.StartsWith("LINQPad.")) {
                            writer = Console.Out.Write;
                        } else
                            writer = new StreamWriter($@"C:\\temp\\MetaObjectProposals_{DateTime.Now.ToString("yyyy-MM-dd_HHmm_ss")}.txt") {
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
