﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Approvers;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using ApprovalTests.Writers;

namespace ProgressOnderwijsUtilsTests
{
    static class ApprovalTest
    {
        class SaneNamer : IApprovalNamer
        {
            public string SourcePath { get; set; }
            public string Name { get; set; }
        }

        public static void Verify(string text, object IGNORE_PAST_THIS = null, [CallerFilePath] string filepath = null, [CallerMemberName] string membername = null)
        {
            var writer = WriterFactory.CreateTextWriter(text);
            var filename = Path.GetFileNameWithoutExtension(filepath);
            var filedir = Path.GetDirectoryName(filepath);
            var namer = new SaneNamer {Name = filename + "." + membername, SourcePath = filedir};
            var reporter = new DiffReporter();
            Approvals.Verify(writer, namer, reporter);
            Approver.Verify(new FileApprover(writer, namer, true), reporter);
        }
    }
}