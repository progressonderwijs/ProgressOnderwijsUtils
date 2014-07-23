using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
	// de IIdentifier wordt met name in de AutoLoadFromDb gebruikt om de mapping van Sql Server naar .Net uit te voeren
	public interface IIdentifier
	{
		bool HasValue { get; }
		int Value { get; set; }
		string DbPrimaryKeyName { get; }
		string DbForeignKeyName { get; }
	}

	public class Identifier
	{
		public static Type BaseType { get { return typeof(int); } }
	}

	[Serializable]
	[DebuggerStepThrough]
	public class Identifier<T> : IIdentifier where T : Identifier<T>, new()
	{
		#region "  Constructor"
		public Identifier() { }

		public Identifier(int value)
		{
			Value = value;
		}

		public virtual string DbPrimaryKeyName
		{
			get
			{
				return typeof(T).Name.ToLower(CultureInfo.InvariantCulture) + "id";
			}
		}

		public virtual string DbForeignKeyName
		{
			get
			{
				return typeof(T).Name.ToLower(CultureInfo.InvariantCulture);
			}
		}

		public static T Create(int value)
		{
			return new T { Value = value };
		}
		#endregion

		private bool valueSet = false;
		private int _value;

		public bool HasValue { get { return valueSet; } }

		public int Value
		{
			get
			{
				if (!valueSet)
					throw new ArgumentException("De waarde van deze identifier wordt uitgelezen voordat deze is gezet");
				return _value;
			}
			set
			{
				valueSet = true;
				_value = value;
			}
		}

		#region " Comparison"
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			var val = (Identifier<T>)obj;
			return (object)val != null && Equals(val);
		}

		public bool Equals(int value)
		{
			return value == Value;
		}

		bool Equals(Identifier<T> obj)
		{
			return obj.Value == Value;
		}

		// Alleen expliciete casts toestaan
		public static explicit operator Identifier<T>(int value)
		{
			return Create(value);
		}

		public static bool operator ==(Identifier<T> a, Identifier<T> b)
		{
			if (((object)a == null) && ((object)b == null))
				return true;

			if (((object)a == null) || ((object)b == null))
				return false;

			return a.Equals(b);
		}

		public static bool operator !=(Identifier<T> a, Identifier<T> b)
		{
			return (!(a == b));
		}
		#endregion

		#region " Boxing"
		//
		// These are called by the JIT
		//
#pragma warning disable 169
		//
		// JIT implementation of box valuetype Identifier
		//
		[UsedImplicitly]
		static object Box(int o)
		{
			return o;
		}

		[UsedImplicitly]
		static int Unbox(object o)
		{
			return (int)o;
		}
#pragma warning restore 169
		#endregion
	}
}
