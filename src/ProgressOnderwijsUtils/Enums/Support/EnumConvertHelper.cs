using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Enums.Support
{
	public interface IHasLabel<TLabel> { TLabel Label { get; } }

	//to use these enum-labelling tools, you will need to:
	// - declare an attribute class (inheriting from IHasLabel<YourLabel'sType> and Attribute)
	// - make two static helper functions to wrap 
	//     EnumConverterHelper.LabelFromEnum<TEnum, TLabel, TAttr> and 
	//     EnumConverterHelper.EnumFromLabel<TEnum, TLabel, TAttr>
	public static class EnumConverter
	{
		public static TLabel LabelFromEnum<TEnum, TLabel, TAttr>(TEnum val)
			where TEnum : struct
			where TAttr : Attribute, IHasLabel<TLabel>
		{
			return EnumConverterHelper<TEnum, TLabel, TAttr>.Converter.LabelFromEnum(val);
		}
		public static TEnum EnumFromLabel<TEnum, TLabel, TAttr>(TLabel label)
			where TEnum : struct
			where TAttr : Attribute, IHasLabel<TLabel>
		{
			return EnumConverterHelper<TEnum, TLabel, TAttr>.Converter.EnumFromLabel(label);
		}
	}

	internal static class EnumConverterHelper<TEnum, TLabel, TAttr>
		where TEnum : struct
		where TAttr : Attribute, IHasLabel<TLabel>
	{
		internal class ConverterImpl : IConverter<TAttr, TLabel>
		{
			public TLabel Convert(TAttr val) { return val.Label; }
		}

		internal static EnumConverterImplementation<TEnum, TLabel, TAttr, ConverterImpl> Converter { get { return EnumConverterImplementation<TEnum, TLabel, TAttr, ConverterImpl>.Converter; } }
	}

}
