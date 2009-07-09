using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProgressOnderwijsUtils.Enums.Interal;
using ProgressOnderwijsUtils;
using NUnit.Framework.Constraints;

namespace ProgressOnderwijsUtilsTests
{

	[TestFixture]
	public class EnumsConverter
	{
		class EnumStringLabelAttribute : Attribute, IHasLabel<string>
		{
			public EnumStringLabelAttribute(string label) { this.Label = label; }
			public string Label { get; private set; }
			public static string FromEnum<TEnum>(TEnum enumV)
				where TEnum : struct
			{
				return EnumConverterHelper.LabelFromEnum<TEnum, string, EnumStringLabelAttribute>(enumV);
			}

			public static TEnum ToEnum<TEnum>(string val)
				where TEnum : struct
			{
				return EnumConverterHelper.EnumFromLabel<TEnum, string, EnumStringLabelAttribute>(val);
			}
		}

		class EnumIntLabelAttribute : Attribute, IHasLabel<int>
		{
			public EnumIntLabelAttribute(int label) { this.Label = label; }
			public int Label { get; private set; }
			public static int FromEnum<TEnum>(TEnum enumV)
				where TEnum : struct
			{
				return EnumConverterHelper.LabelFromEnum<TEnum, int, EnumIntLabelAttribute>(enumV);
			}

			public static TEnum ToEnum<TEnum>(int val)
				where TEnum : struct
			{
				return EnumConverterHelper.EnumFromLabel<TEnum, int, EnumIntLabelAttribute>(val);
			}


		}

		class EnumStrIntLabelAttribute : Attribute, IHasLabel<int>, IHasLabel<string>
		{
			string str;
			public EnumStrIntLabelAttribute(int label, string str) { this.Label = label; this.str = str; }
			public int Label { get; private set; }
			string IHasLabel<string>.Label { get { return str; } }

			public static int IntFromEnum<TEnum>(TEnum enumV)
	where TEnum : struct
			{
				return EnumConverterHelper.LabelFromEnum<TEnum, int, EnumStrIntLabelAttribute>(enumV);
			}

			public static TEnum IntToEnum<TEnum>(int val)
				where TEnum : struct
			{
				return EnumConverterHelper.EnumFromLabel<TEnum, int, EnumStrIntLabelAttribute>(val);
			}
			
			public static string StrFromEnum<TEnum>(TEnum enumV)
	where TEnum : struct
			{
				return EnumConverterHelper.LabelFromEnum<TEnum, string, EnumStrIntLabelAttribute>(enumV);
			}

			public static TEnum StrToEnum<TEnum>(string val)
				where TEnum : struct
			{
				return EnumConverterHelper.EnumFromLabel<TEnum, string, EnumStrIntLabelAttribute>(val);
			}

		}


		enum SomeEnumWithInt { [EnumIntLabel(0)] ValueA, [EnumIntLabel(2)] ValueB, [EnumIntLabel(23)] ValueC }
		enum SomeEnumWithStr { [EnumStringLabel("A")] ValueA, [EnumStringLabel("B")] ValueB, [EnumStringLabel("XYZ")] ValueC }

		enum SomeEnumWithStrAndInt { [EnumIntLabel(0)][EnumStringLabel("A")] ValueA, [EnumIntLabel(1)][EnumStringLabel("B")] ValueB }
		enum SomeEnumWithStrInt { [EnumStrIntLabel(9, "A")] ValueA, [EnumStrIntLabel(8, "B")] ValueB }
		enum DupLabelEnum { [EnumIntLabel(0)] ValueA, [EnumIntLabel(0)] ValueB }
		enum DupValueEnum { [EnumIntLabel(0)] ValueA, [EnumIntLabel(1)] ValueB = 0 }
		enum MissingLabelEnum { [EnumIntLabel(0)] ValueA, ValueB }



		[Test]
		public void CheckCorrectCases()
		{
			Assert.That(EnumIntLabelAttribute.ToEnum<SomeEnumWithInt>(2) == SomeEnumWithInt.ValueB, "should be: 2 ==>ValueB");
			Assert.That(EnumIntLabelAttribute.FromEnum(SomeEnumWithInt.ValueC) == 23, "should be: ValueC ==>23");
			Assert.That(EnumStringLabelAttribute.ToEnum<SomeEnumWithStr>("B") == SomeEnumWithStr.ValueB, "should be: B ==>ValueB");
			Assert.That(EnumStringLabelAttribute.FromEnum(SomeEnumWithStr.ValueC) == "XYZ", "should be: ValueC ==> XYZ");

			foreach(var value in Enum.GetValues(typeof(SomeEnumWithStrAndInt)).Cast<SomeEnumWithStrAndInt>()) {
				Assert.AreEqual(value, EnumIntLabelAttribute.ToEnum<SomeEnumWithStrAndInt>(EnumIntLabelAttribute.FromEnum(value)), " conversion to /from should be equal");
				Assert.AreEqual(value, EnumStringLabelAttribute.ToEnum<SomeEnumWithStrAndInt>(EnumStringLabelAttribute.FromEnum(value)), " conversion to /from should be equal");
			}
			foreach (var value in Enum.GetValues(typeof(SomeEnumWithStrInt)).Cast<SomeEnumWithStrInt>())
			{
				Assert.AreEqual(value, EnumStrIntLabelAttribute.IntToEnum<SomeEnumWithStrInt>(EnumStrIntLabelAttribute.IntFromEnum(value)), " conversion to /from should be equal");
				Assert.AreEqual(value, EnumStrIntLabelAttribute.StrToEnum<SomeEnumWithStrInt>(EnumStrIntLabelAttribute.StrFromEnum(value)), " conversion to /from should be equal");
			}

			//Assert.That(EnumConverterHelper.EnumFromLabel<SomeEnumWithStr,string,EnumStringLabelAttribute>(
		}

		[Test]
		public void ForbidDupLabels()
		{
			EnumLabellingException nogood=null;
			TypeInitializationException outerEx = null;
			try
			{
				EnumIntLabelAttribute.FromEnum(DupLabelEnum.ValueA);
			}
			catch (TypeInitializationException tie)
			{
				outerEx = tie;
				nogood = (EnumLabellingException)tie.InnerException;
			}

			Assert.NotNull(nogood, " from enum should have thrown exception!");
			Assert.That(nogood.Message.Contains("has several members with the same label"));

			bool didthrow = false;
			try
			{
				EnumIntLabelAttribute.ToEnum<DupLabelEnum>(0);
			}
			catch (TypeInitializationException tie)
			{
				Assert.AreSame(outerEx, tie);
				didthrow = true;
				//nogood = (EnumLabellingException)tie.InnerException;
			}
			Assert.That(didthrow);

			Assert.Throws(Is.TypeOf<TypeInitializationException>(), () => { EnumIntLabelAttribute.FromEnum(DupLabelEnum.ValueA); });
			Assert.Throws(Is.TypeOf<TypeInitializationException>(), () => { EnumIntLabelAttribute.ToEnum<DupLabelEnum>(0); });

			Assert.Throws(Is.TypeOf<TypeInitializationException>(), () => { EnumIntLabelAttribute.ToEnum<DupValueEnum>(0); });
			Assert.Throws(Is.TypeOf<TypeInitializationException>(), () => { EnumIntLabelAttribute.FromEnum(DupValueEnum.ValueA); });

			Assert.Throws(Is.TypeOf<TypeInitializationException>(), () => { EnumIntLabelAttribute.FromEnum(MissingLabelEnum.ValueA); });
			Assert.Throws(Is.TypeOf<TypeInitializationException>(), () => { EnumIntLabelAttribute.FromEnum(MissingLabelEnum.ValueA); });

		}

	}
}
