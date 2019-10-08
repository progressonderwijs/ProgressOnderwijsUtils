﻿using System;
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
            PAssert.That(() => new Uri("c:/dir").Combine("file").AbsolutePath == "c:/file");
            PAssert.That(() => new Uri("c:/dir/").Combine("file").AbsolutePath == "c:/dir/file");
            PAssert.That(() => new Uri("c:/dir").Combine("/file").AbsolutePath == "c:/file");
            PAssert.That(() => new Uri("c:/dir/").Combine("/file").AbsolutePath == "c:/file");
        }

        [Fact]
        public void RefersToDirectory_takes_leading_slash_into_account()
        {
            PAssert.That(() => !new Uri("c:/file").RefersToDirectory());
            PAssert.That(() => new Uri("c:/dir/").RefersToDirectory());
        }

        [Fact]
        public void EnsureRefersToDirectory_adds_needed_traling_slash()
        {
            PAssert.That(() => new Uri("c:/file").EnsureRefersToDirectory().RefersToDirectory());
            PAssert.That(() => new Uri("c:/dir/").EnsureRefersToDirectory().RefersToDirectory());
        }

        [Fact]
        public void ReplaceRootWith()
        {
            PAssert.That(() => new Uri("c:/file").ReplaceRootWith(new Uri("d:/")).AbsolutePath == "d:/file");
            PAssert.That(() => new Uri("c:/dir/").ReplaceRootWith(new Uri("d:/")).AbsolutePath == "d:/dir/");
        }

        [Fact]
        public void RefersToExistingLocalDirectory()
        {
            PAssert.That(() => !new Uri(Assembly.GetExecutingAssembly().Location).RefersToExistingLocalDirectory());
            PAssert.That(() => new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).EnsureRefersToDirectory().RefersToExistingLocalDirectory());
        }
        [Fact]
        public void RefersToExistingLocalFile()
        {
            PAssert.That(() => new Uri(Assembly.GetExecutingAssembly().Location).RefersToExistingLocalFile());
            PAssert.That(() => !new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).EnsureRefersToDirectory().RefersToExistingLocalFile());
        }
    }
}
