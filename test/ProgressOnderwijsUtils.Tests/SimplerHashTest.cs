﻿using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Tests
{
    
    public class SimplerHashTest
    {
        [Fact]
        public void BasicChecks()
        {
            const string strA = @" ASDF#VA#Q$T*B#$%(DFB	<script>";
            PAssert.That(
                () =>
                    SimplerHash.MD5ComputeHash(strA) != strA && SimplerHash.MD5VerifyHash(strA, SimplerHash.MD5ComputeHash(strA))
                        && !SimplerHash.MD5VerifyHash(strA.Substring(1), SimplerHash.MD5ComputeHash(strA)));
        }
    }
}
