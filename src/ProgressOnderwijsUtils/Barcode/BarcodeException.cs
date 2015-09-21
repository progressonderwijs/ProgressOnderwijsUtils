using System;

namespace ProgressOnderwijsUtils.Barcode
{
    [Serializable]
    public class BarcodeException : NietZoErgeException
    {
        public BarcodeException(string message)
            : base(message) { }
    }
}
