using System;

namespace ProgressOnderwijsUtils
{
    public struct VariantData : IEquatable<VariantData>
    {
        readonly object value;
        public VariantData(object value) { this.value = value; }
        public object Value => value;
        public T GetValue<T>() => (T)value;
        public override string ToString() => $"{Value} : {value?.GetType()}";
        public bool Equals(VariantData other) =>Equals(other.Value, Value);
        public override bool Equals(object obj) => obj is VariantData && Equals((VariantData)obj);
        public override int GetHashCode() => 123456789 + (Value?.GetHashCode() ?? 0);
        public static bool operator ==(VariantData left, VariantData right) { return left.Equals(right); }
        public static bool operator !=(VariantData left, VariantData right) { return !left.Equals(right); }
        public static explicit operator string(VariantData data) => data.ToString();
    }
}
