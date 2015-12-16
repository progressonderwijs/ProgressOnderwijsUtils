namespace ProgressOnderwijsUtils
{
    public enum DatumFormaat
    {
        AlleenDatum = 0,
        AlleenTijd = 1,
        DatumEnTijdInMinuten = 2,
        DatumEnTijdInSeconden = 3,
        DatumEnTijdInMilliseconden = 4,
        //met ToolTips
        DatumToolTipTijd = 5,
        JaarToolTipDatum = 6,
        SMDatum = 7, // formaat: EEJJMMDD
        SMDatumTijd = 8, // formaat: EEJJMMDDhhmmss
        ClieopDatum = 9, // formaat: DDMMJJ
        MT940Datum = 10, // formaat: JJMMDD
        DatumZonderJaar = 11,
        ISODate = 13, // formaat: JJJJ-MM-DD
        ISODateTime = 14, // formaat: JJJJ-MM-DDTHH:MM:SS
    }
}