using System;
using System.Globalization;

namespace ProgressOnderwijsUtils
{
	public struct VariantData : IEquatable<VariantData>
	{
		public string Type { get; set; }
		public object Value { get; set; }

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} : {1}", Value, Type);
		}

		public bool Equals(VariantData other)
		{
			return Equals(other.Type, Type) && Equals(other.Value, Value);
		}

		public override bool Equals(object obj)
		{
			bool result = false;
			if (obj != null && obj.GetType() == typeof(VariantData))
			{
				result = Equals((VariantData)obj);
			}
			return result;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
			}
		}

		public static bool operator ==(VariantData left, VariantData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(VariantData left, VariantData right)
		{
			return !left.Equals(right);
		}
	}
}
