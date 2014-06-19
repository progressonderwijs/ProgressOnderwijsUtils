using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	[DebuggerStepThrough]
	public class Identifier<T> where T : Identifier<T>, new()
	{
		#region "  Constructor"
		internal Identifier() { }

		public Identifier(int value)
		{
			this.Value = value;
		}

		public static T Create(int value)
		{
			return new T { Value = value };
		}
		#endregion

		public int Value { get; internal set; }

		#region " Comparison"
		// Alleen expliciete casts toestaan
		public static explicit operator Identifier<T>(int value)
		{
			return Create(value);
		}

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

		public static bool operator ==(Identifier<T> a, Identifier<T> b)
		{
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

	[TestFixture]
	public class TestIdentifier
	{
		class TestId : Identifier<TestId> { }

		[Test]
		public static void EqualityCheck()
		{
			const int value = 12;
			var id = TestId.Create(value);
			Assert.That(id == (TestId)value, Is.True);
			// ReSharper disable EqualExpressionComparison
			Assert.That(id == id, Is.True);
			// ReSharper restore EqualExpressionComparison
			Assert.That(id.Equals(value), Is.True);
		}

		[Test]
		public static void Speed()
		{
			var value1 = (int?)12;
			var value2 = (int?)12;
			var id1 = TestId.Create(12);
			var id2 = TestId.Create(12);

			const int steps = 10000000;
			var stopwatch = new Stopwatch();

			stopwatch.Restart();
			for (var i = 0; i < steps; i++)
			{
				Assert.That(value1, Is.EqualTo(value2));
			}
			var time1 = stopwatch.ElapsedMilliseconds;

			stopwatch.Restart();
			for (var i = 0; i < steps; i++)
			{
				Assert.That(id1, Is.EqualTo(id2));
			}
			var time2 = stopwatch.ElapsedMilliseconds;
			var fraction = Math.Abs((time1 - time2) / time1);
			Assert.That(fraction, Is.LessThan(0.01));
		}
	}
}
