using System;
using System.IO;
using System.Reflection;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class UriExtensionsTest
    {
        [Fact]
        public void Combine_works_safely()
        {
            PAssert.That(() => new Uri(@"c:\file").Combine(@"file").LocalPath == @"c:\file");
            PAssert.That(() => new Uri(@"c:\dir\").Combine(@"file").LocalPath == @"c:\dir\file");
            PAssert.That(() => new Uri(@"c:\file").Combine(@"\file").LocalPath == @"c:\file");
            PAssert.That(() => new Uri(@"c:\dir\").Combine(@"dir\").LocalPath == @"c:\dir\dir\");
        }

        [Fact]
        public void RefersToDirectory_takes_trailing_slash_into_account()
        {
            PAssert.That(() => !new Uri(@"c:\file").RefersToDirectory());
            PAssert.That(() => new Uri(@"c:\dir\").RefersToDirectory());
        }

        [Fact]
        public void EnsureRefersToDirectory_adds_needed_traling_slash()
        {
            PAssert.That(() => new Uri(@"c:\dirWithForgottenTrailingSlash").EnsureRefersToDirectory().RefersToDirectory());
            PAssert.That(() => new Uri(@"c:\dir\").EnsureRefersToDirectory().RefersToDirectory());
        }

        [Fact]
        public void ReplaceRootWith()
        {
            PAssert.That(() => new Uri(@"c:\file").ReplaceRootWith(new Uri(@"d:\")).LocalPath == @"d:\file");
            PAssert.That(() => new Uri(@"c:\dir\").ReplaceRootWith(new Uri(@"d:\")).LocalPath == @"d:\dir\");
            PAssert.That(() => new Uri(@"\\root\file").ReplaceRootWith(new Uri(@"c:\")).LocalPath == @"c:\file");
            PAssert.That(() => new Uri(@"\\root\dir\").ReplaceRootWith(new Uri(@"c:\")).LocalPath == @"c:\");
        }

        [Fact]
        public void RefersToExistingLocalDirectory()
        {
            PAssert.That(() => !new Uri(MyDllPath()).RefersToExistingLocalDirectory(), "An existing file should not appear to be an existing directory");
            PAssert.That(() => new Uri(MyDllFolderPath()).EnsureRefersToDirectory().RefersToExistingLocalDirectory(), "An existing directory should appear thus");
        }

        static string MyDllFolderPath()
            => Path.GetDirectoryName(MyDllPath()).AssertNotNull();

        static string MyDllPath()
            => Assembly.GetExecutingAssembly().Location;

        [Fact]
        public void RefersToExistingLocalFile()
        {
            PAssert.That(() => new Uri(MyDllPath()).RefersToExistingLocalFile(), "An existing file should appear thus");
            PAssert.That(() => !new Uri(MyDllFolderPath()).EnsureRefersToDirectory().RefersToExistingLocalFile(), "An existing directory should not appear to be an existing file");
        }
    }
}
