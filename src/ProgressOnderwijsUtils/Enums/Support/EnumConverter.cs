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
}
