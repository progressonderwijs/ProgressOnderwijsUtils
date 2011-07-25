using System;
using System.Globalization;

namespace ProgressOnderwijsUtils
{
	public struct VariantData : IEquatable<VariantData>
	{
		public static VariantData Create<T>(T value)
		{
			return new VariantData(typeof(T), value);
		}
		public VariantData(Type type, object value)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			//TODO: this is null-unsafe; however existing code doesn't pass in whether it can be null or not. (i.e. int? is passed as int)
			//if (value == null && !type.CanBeNull())
			//    throw new ArgumentNullException("value", "Type " + type + " does not permit null values");

			if (value != null)
			{
				Type valueType = value.GetType();
				if (!type.IsAssignableFrom(valueType) && 
					!(valueType.IsEnum && type.IsAssignableFrom(valueType.GetEnumUnderlyingType())) &&
					!(type.IsEnum && valueType.IsAssignableFrom(type.GetEnumUnderlyingType())))
					throw new ArgumentException("An object of type " + value.GetType() + " may not be placed in a variant data of type " + type);
			}
			this.type = type;
			this.value = value;
		}
		readonly Type type;
		readonly object value;
		public Type Type { get { return type; } }
		public object Value { get { return value; } }

		public override string ToString() { return ToUiString(); }
		public string ToUiString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} : {1}", Value, Type);
		}

		public bool Equals(VariantData other)
		{
			return Equals(other.Type, Type) && Equals(other.Value, Value);
		}

		public override bool Equals(object obj)
		{
			return obj is VariantData && Equals((VariantData)obj);
		}

		public override int GetHashCode()
		{
			return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
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
