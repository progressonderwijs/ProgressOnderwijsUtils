using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Data;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class FileAndVariantDataTest
    {
        [Test]
        public void FileDataTest()
        {
            FileData
                empty = default(FileData),
                basic = new FileData("xyz", new byte[] { 1, 2, 3 }, "ab"),
                same = new FileData("Xxyz".Substring(1), new byte[] { 1, 2, 3 }, "aab".Substring(1)),
                diffdata = new FileData("xyz", new byte[] { 1, 2, 4 }, "ab"),
                difftype = new FileData("xyz", new byte[] { 1, 2, 3 }, "abc"),
                diffname = new FileData("xYz", new byte[] { 1, 2, 3 }, "ab")
                ;

            PAssert.That(() => same == basic && basic == same);
            PAssert.That(() => same.GetHashCode() == basic.GetHashCode());

            PAssert.That(() => empty != basic && basic != empty);
            PAssert.That(() => empty.GetHashCode() != basic.GetHashCode());

            PAssert.That(() => diffdata != basic && diffdata != empty);
            PAssert.That(() => diffdata.GetHashCode() != basic.GetHashCode());

            PAssert.That(() => difftype != basic && difftype != empty);
            PAssert.That(() => difftype.GetHashCode() != basic.GetHashCode());

            PAssert.That(() => diffname != basic && diffname != empty);
            PAssert.That(() => diffname.GetHashCode() != basic.GetHashCode()); //TODO: case-sensitivity?
        }

        [Test]
        public void VariantDataTest()
        {
            VariantData
                empty = default(VariantData),
                nullObj = new VariantData(null),
                int1 = new VariantData(1),
                int1b = new VariantData(1),
                str1 = new VariantData("1"),
                str2 = new VariantData("2"),
                str2b = new VariantData("2"),
                nullObjb = new VariantData(null)
                ;

            PAssert.That(() => int1 == int1b && int1b == int1);
            PAssert.That(() => int1.GetHashCode() == int1b.GetHashCode());

            PAssert.That(() => str2 == str2b && str2b == str2);
            PAssert.That(() => str2.GetHashCode() == str2b.GetHashCode());

            PAssert.That(() => empty != int1 && int1 != empty);
            PAssert.That(() => empty.GetHashCode() != int1.GetHashCode());

            PAssert.That(() => str2b != int1 && int1 != str1);
            PAssert.That(() => str1.GetHashCode() != int1.GetHashCode());

            PAssert.That(() => nullObj == nullObjb && nullObj.GetHashCode() == nullObjb.GetHashCode());
            PAssert.That(() => Equals(nullObj, nullObjb) && Equals(int1, int1b));

            //enums are tricky:
            PAssert.That(
                () => new VariantData(DatabaseVersion.Undefined) != new VariantData(0) &&
                    new VariantData(DatabaseVersion.Undefined).GetHashCode() == new VariantData(0).GetHashCode());
        }
    }
}
