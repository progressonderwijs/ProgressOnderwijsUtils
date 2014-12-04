using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProgressOnderwijsUtils.DeploymentScriptingUtils
{
    /// <summary>
    /// Useful in LINQpad queries
    /// </summary>
    public static class WinProcessUtil
    {
        public struct ExecutionResult
        {
            public string StandardOutputContents, StandardErrorContents;
            public int ExitCode;
        }

        public static ExecutionResult ExecuteProcessSynchronously(string filename, string arguments, string input)
        {
            using (
                var proc = Process.Start(
                    new ProcessStartInfo {
                        CreateNoWindow = true, //don't need UI.
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true, //so we can capture and control the new process's standard I/O
                        UseShellExecute = false, //required to be able to redirect streams
                        FileName = filename,
                        Arguments = arguments,
                    }
                    )) {
                if (input != null) {
                    proc.StandardInput.Write(input);
                }
                proc.StandardInput.Close();
                var outval = proc.StandardOutput.ReadToEnd();
                var errval = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                return new ExecutionResult { StandardOutputContents = outval, StandardErrorContents = errval, ExitCode = proc.ExitCode };
            }
        }
    }
}
