using System.Globalization;

namespace ProgressOnderwijsUtils.Barcode
{
    public static class BarcodeEncodingValue
    {
        public const int AANTAL_KARAKTERS = 36;
        internal const int MAX_ENCODED_VALUE_LENGTH = 6; // Maximum aantal posities voor een int32

        /// <summary>
        /// Geeft de hoogste exponent mogelijk voor val
        /// </summary>
        internal static int HighestExponent(int val)
        {
            var i = 0;
            var previous = 0;
            while (true) {
                var max = Pow(i);
                if (max > val) {
                    break;
                }
                if (max < 0) {
                    break;
                }
                previous = i;
                i++;
            }
            return previous;
        }

        /// <summary>
        /// Voorberekende waardes voor AANTAL_KARAKTERS^value
        /// </summary>
        public static long Pow(int value)
        {
            // De waardes vantevoren uitrekenen scheelt performance
            switch (value) {
                case 0:
                    return 1;
                case 1:
                    return 36;
                case 2:
                    return 1296;
                case 3:
                    return 46656;
                case 4:
                    return 1679616;
                case 5:
                    return 60466176;
                case 6:
                    return 2176782336; // Deze waarde is groter dan een int32, maar is nodig voor de functie HighestExponent
                default:
                    throw new BarcodeException("Te hoge waarde voor Pow: " + value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Geeft het getal bij een barcodewaarde
        /// </summary>
        public static int Get(char c)
        {
            switch (c) {
                case '0':
                    return 19;
                case '1':
                    return 25;
                case '2':
                    return 3;
                case '3':
                    return 13;
                case '4':
                    return 14;
                case '5':
                    return 18;
                case '6':
                    return 7;
                case '7':
                    return 5;
                case '8':
                    return 32;
                case '9':
                    return 2;
                case 'A':
                    return 9;
                case 'B':
                    return 34;
                case 'C':
                    return 21;
                case 'D':
                    return 0;
                case 'E':
                    return 30;
                case 'F':
                    return 17;
                case 'G':
                    return 20;
                case 'H':
                    return 1;
                case 'I':
                    return 15;
                case 'J':
                    return 24;
                case 'K':
                    return 16;
                case 'L':
                    return 4;
                case 'M':
                    return 26;
                case 'N':
                    return 27;
                case 'O':
                    return 35;
                case 'P':
                    return 31;
                case 'Q':
                    return 33;
                case 'R':
                    return 6;
                case 'S':
                    return 22;
                case 'T':
                    return 12;
                case 'U':
                    return 11;
                case 'V':
                    return 8;
                case 'W':
                    return 10;
                case 'X':
                    return 28;
                case 'Y':
                    return 29;
                case 'Z':
                    return 23;
                default:
                    throw new BarcodeException("Waarde kan niet worden gedecodeerd: " + c.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// De barcode-waarde voor het type
        /// </summary>
        internal static char Get(BarcodeType type) => Get((int)type);

        /// <summary>
        /// Geeft de barcodewaarde voor een getal
        /// </summary>
        public static char Get(int i)
        {
            switch (i) {
                case 0:
                    return 'D';
                case 1:
                    return 'H';
                case 2:
                    return '9';
                case 3:
                    return '2';
                case 4:
                    return 'L';
                case 5:
                    return '7';
                case 6:
                    return 'R';
                case 7:
                    return '6';
                case 8:
                    return 'V';
                case 9:
                    return 'A';
                case 10:
                    return 'W';
                case 11:
                    return 'U';
                case 12:
                    return 'T';
                case 13:
                    return '3';
                case 14:
                    return '4';
                case 15:
                    return 'I';
                case 16:
                    return 'K';
                case 17:
                    return 'F';
                case 18:
                    return '5';
                case 19:
                    return '0';
                case 20:
                    return 'G';
                case 21:
                    return 'C';
                case 22:
                    return 'S';
                case 23:
                    return 'Z';
                case 24:
                    return 'J';
                case 25:
                    return '1';
                case 26:
                    return 'M';
                case 27:
                    return 'N';
                case 28:
                    return 'X';
                case 29:
                    return 'Y';
                case 30:
                    return 'E';
                case 31:
                    return 'P';
                case 32:
                    return '8';
                case 33:
                    return 'Q';
                case 34:
                    return 'B';
                case 35:
                    return 'O';
                default:
                    throw new BarcodeException("Waarde kan niet worden geëncodeerd: " + i.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
