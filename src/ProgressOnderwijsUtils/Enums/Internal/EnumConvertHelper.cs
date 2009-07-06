using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ProgressOnderwijsUtils
{
	internal interface IHasLabel<TLabel> { TLabel Label { get; } }
	internal interface IConverter<TFrom, TTo> { TTo Convert(TFrom val);	}
	internal interface IEnumLabeller<TEnum, TLabel>
	{
		TLabel LabelFromEnum(TEnum val);
		TEnum EnumFromLabel(TLabel label);
	}
	internal class EnumConverterImplementation<TEnum, TLabel, TAttr, TAttrToLabelConverter>
		: IEnumLabeller<TEnum, TLabel>
		where TEnum : struct
		where TAttr : Attribute
		where TAttrToLabelConverter : IConverter<TAttr, TLabel>, new()
	{
		static EnumConverterImplementation<TEnum, TLabel, TAttr, TAttrToLabelConverter> converter = new EnumConverterImplementation<TEnum, TLabel, TAttr, TAttrToLabelConverter>();

		internal static EnumConverterImplementation<TEnum, TLabel, TAttr, TAttrToLabelConverter> Converter { get { return converter; } }

		Dictionary<TEnum, TLabel> labelLookup;
		Dictionary<TLabel, TEnum> enumValueLookup;
		EnumConverterImplementation()
		{

			Type enumType = typeof(TEnum);
			Type attrType = typeof(TAttr);
			TAttrToLabelConverter labelGenerator = new TAttrToLabelConverter();

			if (!enumType.IsEnum)
				throw new EnumLabellingException(string.Format("EnumConvertHelper used on a non-enum type {0}", enumType.FullName));

			var enumValues =
				from valFieldInfo in enumType.GetFields(BindingFlags.Public | BindingFlags.Static)
				select new
				{
					EnumValue = (TEnum)valFieldInfo.GetValue(null),
					Label =
					 labelGenerator.Convert((TAttr)
						valFieldInfo.GetCustomAttributes(attrType, true) //gets all attrs of the type attrType
						.Single() //will throw InvalidOperationException if not precisely one attr returned
						) //uses AttrToLabelConverter to read the label value from the attribute.
				};
			try
			{
				enumValues = enumValues.ToArray();//evaluate query.
			}
			catch (InvalidOperationException ioe)
			{
				throw new EnumLabellingException(string.Format("The enumeration {0} has values without a unique label attribute", enumType.FullName), ioe);
			}

			try
			{
				labelLookup = enumValues.ToDictionary(tuple => tuple.EnumValue, tuple => tuple.Label);//lookup: map enum values to labels 
			}
			catch (ArgumentException ae)//ToDictionary will throw argument exception if duplicate values or duplicate labels cause the dictionaries to have a key-collision.
			{
				throw new EnumLabellingException(string.Format("The enumeration {0} has several members with the same value", enumType.FullName), ae);
			}

			try
			{
				enumValueLookup = enumValues.ToDictionary(tuple => tuple.Label, tuple => tuple.EnumValue);//lookup: map labels to enum values
			}
			catch (ArgumentException ae)
			{
				throw new EnumLabellingException(string.Format("The enumeration {0} has several members with the same label", enumType.FullName), ae);
			}
		}

		public TLabel LabelFromEnum(TEnum val) { return labelLookup[val]; }
		public TEnum EnumFromLabel(TLabel label) { return enumValueLookup[label]; }
	}

	internal static class EnumConverterHelper<EnumT, LabelT, AttrT>
		where EnumT : struct
		where AttrT : Attribute, IHasLabel<LabelT>
	{
		internal class ConverterImpl : IConverter<AttrT, LabelT>
		{
			public LabelT Convert(AttrT val) { return val.Label; }
		}

		internal static EnumConverterImplementation<EnumT, LabelT, AttrT, ConverterImpl> Converter { get { return EnumConverterImplementation<EnumT, LabelT, AttrT, ConverterImpl>.Converter; } }
	}

}
