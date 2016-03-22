using System;

namespace ProgressOnderwijsUtils
{
    public static class IdentifierTools
    {
        public static string DbPrimaryKeyName<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException("Id-type moet een enum zijn");
            }
            switch (typeof(T).Name) {
                case "RootOrganisatie":
                    return "organisatieid";
                case "BankrekeningAfschrift":
                    return "afschriftid";
                case "Betalinginformatie":
                    return "StudentInschrijvingBetalingInformatieId";
                case "VolgOnderwijs":
                    return "onderwijsid";
                case "InschrijvingOnderwijs": //TODO: move uit tools en nameof(Id.InschrijvingOnderwijs):
                    return "onderwijsid";
                case "OverigeVordering":
                    return "vorderingoverigid";
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
