namespace ProgressOnderwijsUtils.SchemaReflection
{
    struct EightFlags
    {
        public byte PackedValues;

        public EightFlags(byte initialValue)
            => PackedValues = initialValue;

        public bool this[int idx]
        {
            get
                => (PackedValues & 1 << idx) != 0;
            set
                => PackedValues = value ? (byte)(PackedValues | 1 << idx) : (byte)(PackedValues & ~(1 << idx));
        }
    }
}
