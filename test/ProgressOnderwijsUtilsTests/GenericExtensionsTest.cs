﻿using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Data;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class GenericExtensionsTest
    {
        [Test]
        public void InStruct()
        {
            PAssert.That(() => PnetOmgeving.TestingContinuously.In(PnetOmgeving.Productie, PnetOmgeving.TestingContinuously));
            PAssert.That(() => !PnetOmgeving.TestingContinuously.In(PnetOmgeving.Productie, PnetOmgeving.Test));
            PAssert.That(() => !default(PnetOmgeving?).In(PnetOmgeving.Productie, PnetOmgeving.Test));
            PAssert.That(() => default(PnetOmgeving?).In(PnetOmgeving.Productie, PnetOmgeving.Test, null));

            PAssert.That(() => 3.In(1, 2, 3));
            PAssert.That(() => !3.In(1, 2, 4, 8));
        }
    }
}
