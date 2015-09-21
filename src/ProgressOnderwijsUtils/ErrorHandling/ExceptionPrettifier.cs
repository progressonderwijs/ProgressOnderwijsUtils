using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.ErrorHandling
{
    public static class ExceptionPrettifier
    {
        public static string PrettyPrintException(Exception value)
        {
            try {
                var sb = new StringBuilder();
                PrettyPrintException(sb, value);
                return sb.ToString();
            } catch //This code is used in diagnostics and anything is better than nothing.
            {
                return value.ToString();
            }
        }

        static void PrettyPrintException(StringBuilder sb, Exception value)
        {
            //this function extract a stactrace and formats it just like .NET does, except that it includes nicely formatted generic type parameters (or arguments);
            //it should be a drop-in replacement for Exception.StackTrace in particular with respect to Regex stack-trace parsing.

            if (value.InnerException != null) {
                PrettyPrintException(sb, value.InnerException);
                sb.Append("--- End of inner exception stack trace ---\n\n");
            }
            sb.Append(value.Message);
            sb.Append('\n');
            foreach (StackFrame frame in new StackTrace(value, true).GetFrames()) {
                MethodBase method = frame.GetMethod();
                Type type = method.DeclaringType;
                sb.Append("   at ");
                PrintNamespacedType(sb, type);
                sb.Append('.');
                PrintMethodName(sb, method);
                PrintMethodParameters(sb, method);
                string fileName = frame.GetFileName();
                if (!string.IsNullOrEmpty(fileName)) {
                    sb.Append(" in ");
                    sb.Append(StripIrrelevantPathPrefix(fileName));
                    sb.Append(":line ");
                    sb.Append(frame.GetFileLineNumber());
                }
                sb.Append('\n');
            }
        }

        static string StripIrrelevantPathPrefix(string fileName)
        {
            const string needle = @"\progress\";
            int idx = fileName.IndexOf(needle, StringComparison.Ordinal);
            return idx < 0 ? fileName : fileName.Substring(idx + needle.Length);
        }

        static void PrintMethodParameters(StringBuilder sb, MethodBase method)
        {
            sb.Append('(');
            bool first = true;
            foreach (ParameterInfo pi in method.GetParameters()) {
                if (first) {
                    first = false;
                } else {
                    sb.Append(", ");
                }
                sb.Append(ObjectToCode.GetCSharpFriendlyTypeName(pi.ParameterType));
                sb.Append(' ');
                sb.Append(pi.Name);
            }
            sb.Append(')');
        }

        static void PrintMethodName(StringBuilder sb, MethodBase method)
        {
            sb.Append(MethodNameWithoutInterfaceName(method));
            if (method.IsGenericMethod) {
                sb.Append(
                    "<" +
                        method.GetGenericArguments()
                            .Select(ObjectToCode.GetCSharpFriendlyTypeName)
                            .JoinStrings(", ") +
                        ">");
            }
        }

        static void PrintNamespacedType(StringBuilder sb, Type type)
        {
            sb.Append(type.Namespace);
            sb.Append('.');
            sb.Append(ObjectToCode.GetCSharpFriendlyTypeName(type));
        }

        static string MethodNameWithoutInterfaceName(MethodBase method)
        {
            string name = method.Name;
            int lastDotIdx = name.LastIndexOf('.');
            return lastDotIdx < 0
                ? name
                : name.Substring(lastDotIdx + 1);
        }
    }
}
