﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class SimplerHashTest
	{
		[Test]
		public void BasicChecks()
		{
			const string strA = @" ASDF#VA#Q$T*B#$%(DFB	<script>";
			PAssert.That(() => SimplerHash.MD5ComputeHash(strA) != strA && SimplerHash.MD5VerifyHash(strA, SimplerHash.MD5ComputeHash(strA)) && !SimplerHash.MD5VerifyHash(strA.Substring(1), SimplerHash.MD5ComputeHash(strA)));
		}
	}
}
