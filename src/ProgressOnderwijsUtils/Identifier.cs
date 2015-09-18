using System;

namespace ProgressOnderwijsUtils
{
    public static class IdentifierTools
    {
        public static string DbPrimaryKeyName<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            switch (typeof(T).Name) {
                case "RootOrganisatie":
                    return "organisatieid";
                case "BankrekeningAfschrift":
                    return "afschriftid";
                default:
                    return typeof(T).Name + "Id";
            }
        }

        public static string DbForeignKeyName<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            if (typeof(T).Name == "RootOrganisatie") {
                return "organisatie";
            } else {
                return typeof(T).Name;
            }
        }
    }
}
