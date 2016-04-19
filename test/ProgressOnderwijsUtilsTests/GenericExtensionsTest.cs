using ExpressionToCodeLib;
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
            PAssert.That(() => DatabaseVersion.Ontwikkel.In(DatabaseVersion.Productie, DatabaseVersion.Ontwikkel));
            PAssert.That(() => !DatabaseVersion.Ontwikkel.In(DatabaseVersion.Productie, DatabaseVersion.Test));
            PAssert.That(() => !default(DatabaseVersion?).In(DatabaseVersion.Productie, DatabaseVersion.Test));
            PAssert.That(() => default(DatabaseVersion?).In(DatabaseVersion.Productie, DatabaseVersion.Test, null));

            PAssert.That(() => 3.In(1, 2, 3));
            PAssert.That(() => !3.In(1, 2, 4, 8));
        }
    }
}
