using System;
using System.Globalization;

namespace ProgressOnderwijsUtils.Barcode
{
    public static class BarcodeEncoder
    {
        const int EncodedLength = BarcodeEncodingValue.MAX_ENCODED_VALUE_LENGTH + 1 + 1 /* type + geencodeerde waarde + checksumvalue */;

        public struct BarcodeWaarde
        {
            public int Value { get; set; }
            public BarcodeType Type { get; set; }
        }

        public static BarcodeWaarde Decode(string value)
        {
            // Code39 wordt gegenereerd met een initiator en terminator *. Deze kan worden genegeerd
            value = value.Trim(new char[] { '*' });

            if (value.Length != EncodedLength) {
                throw new BarcodeException("Fout BarcodeWaarde-lengte");
            }

            var suppliedTypeChar = value[0];
            var suppliedEncodedValue = value.Substring(1, BarcodeEncodingValue.MAX_ENCODED_VALUE_LENGTH);
            var suppliedChecksumChar = value[EncodedLength - 1];

            var type = (BarcodeType)BarcodeEncodingValue.Get(suppliedTypeChar);
            var suppliedChecksum = BarcodeEncodingValue.Get(suppliedChecksumChar);

            var calculatedChecksum = (int)type;
            var calculatedValue = 0;
            for (var position = 0; position < BarcodeEncodingValue.MAX_ENCODED_VALUE_LENGTH; position++) {
                var exponent = BarcodeEncodingValue.MAX_ENCODED_VALUE_LENGTH - position - 1;
                var valueAtPosition = BarcodeEncodingValue.Get(suppliedEncodedValue[position]);
                calculatedValue += valueAtPosition * (int)BarcodeEncodingValue.Pow(exponent);
                calculatedChecksum += valueAtPosition;
            }
            if (Checksum(calculatedChecksum) != suppliedChecksum) {
                throw new BarcodeException("Foute Checksum");
            }

            return new BarcodeWaarde { Type = type, Value = calculatedValue };
        }

        /// <summary>
        /// Genereert uit een int32 een code die als barcode kan worden getoond. Deze code is als volgt opgebouwd:
        ///   TwwwwwwC
        /// Met: 
        ///   T: het type barcode
        ///   wwwwww: een versleuteling van de int32 waarde
        ///   C: een checksum over T en wwwwww
        /// </summary>
        public static string Encode(BarcodeType type, int value) => Encode(new BarcodeWaarde { Type = type, Value = value });

        static string Encode(BarcodeWaarde barcodeWaarde)
        {
            if (barcodeWaarde.Type == BarcodeType.Onbekend) {
                throw new BarcodeException(Enum.GetName(typeof(BarcodeType), barcodeWaarde.Type));
            }
            if (barcodeWaarde.Value < 0) {
                throw new BarcodeException(barcodeWaarde.Value.ToString(CultureInfo.InvariantCulture));
            }

            var remainingValue = barcodeWaarde.Value;
            var encodedValue = "";
            var checksum = (int)barcodeWaarde.Type;
            for (var exponent = BarcodeEncodingValue.HighestExponent(remainingValue); exponent >= 0; exponent--) {
                var exponentiation = BarcodeEncodingValue.Pow(exponent);
                if (exponentiation <= remainingValue) {
                    // ReSharper disable PossibleLossOfFraction
                    var currentValue = (int)Decimal.Truncate(remainingValue / exponentiation);
                    // ReSharper restore PossibleLossOfFraction
                    encodedValue += BarcodeEncodingValue.Get(currentValue);
                    checksum += currentValue;
                    remainingValue -= currentValue * (int)exponentiation;
                } else {
                    encodedValue += BarcodeEncodingValue.Get(0);
                }
            }
            var checksumValue = BarcodeEncodingValue.Get(Checksum(checksum));
            var barcode = BarcodeEncodingValue.Get(barcodeWaarde.Type)
                + encodedValue.PadLeft(BarcodeEncodingValue.MAX_ENCODED_VALUE_LENGTH, BarcodeEncodingValue.Get(0))
                + checksumValue;
            return barcode;
        }

        static int Checksum(int value) => value % BarcodeEncodingValue.AANTAL_KARAKTERS;
    }
}
