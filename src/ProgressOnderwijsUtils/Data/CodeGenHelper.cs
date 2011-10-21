using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.Data
{
	//tooltje om handige type-wrapper te maken.
	public static class CodeGenHelper
	{
		/*public class Params
		{
			public readonly List<Tuple<string, object>> exampleValues;
			public void Add(string paramname)
			{
				
			}
		}
		public static string GetRowClassDef(string query,  DataTable dt, string name = null)
		{
			

		}*/

		public static string GetRowClassDef(DataTable dt, string name = null)
		{
			var columns = dt.Columns.Cast<DataColumn>().Select(col => new { Type = (col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName }).ToArray();
			var primes = Primes().Skip(1).Take(columns.Length).ToArray();
			name = name ?? (string.IsNullOrEmpty(dt.TableName)? "XYZ" : dt.TableName);
			return "public sealed class " + name + " : IEquatable<" + name + "> {\n"
				   + "\treadonly int _hashcode_auto_cached;\n"
				   + string.Join("", columns.Select(col => "\tpublic readonly " + ObjectToCode.GetCSharpFriendlyTypeName(col.Type) + " " + col.ColumnName + ";\n"))
				   + "\tpublic " + name + "(DataRow dr) {\n"
				   + "\t\tulong _hashcode_auto_building=0;\n"
				   + string.Join("", columns.Select((col, i) => "\t\t" + col.ColumnName + " = dr.Field<" + ObjectToCode.GetCSharpFriendlyTypeName(col.Type) + ">(\"" + col.ColumnName + "\");\n"
				   + "\t\t_hashcode_auto_building += " + (
						col.Type.IsValueType
							? "(ulong)" + col.ColumnName + ".GetHashCode()"
							: col.ColumnName + " == null ? " + i + " : (ulong)" + col.ColumnName + ".GetHashCode() *  " + primes[i] + "UL"
						) + ";\n")
					)
				   + "\t\t_hashcode_auto_cached = (int)(uint)(_hashcode_auto_building ^ (_hashcode_auto_building >> 32));\n"
				   + "\t}\n"
				   + "\tpublic bool Equals(" + name + " that) {\n"
				   + "\t\treturn "
				   + string.Join("\n\t\t\t&& ", columns.Select(col => "Equals(" + col.ColumnName + ", that." + col.ColumnName + ")")) + ";\n"
				   + "\t}\n"
				   + "\tpublic override int GetHashCode() { return _hashcode_auto_cached; }\n"
				   + "\tpublic override bool Equals(object that) { return that is " + name + " && Equals((" + name + ")that); }\n"
				   + "}\n";
		}

		static IEnumerable<ulong> Primes()
		{
			var primes = new List<ulong>();
			for (ulong i = 2; i != 0; i++)
				if (!primes.Any(p => i % p == 0))
				{
					primes.Add(i);
					yield return i;
				}
		}
	}
}

/*
			ulong res = 0;
			for(uint i=0;i<obj.Length;i++) {
				object val = obj[i];
				res += val == null ? i : (ulong)val.GetHashCode() * (1 + 2 * i);
			}
			return (int)(uint)(res ^ (res >> 32));*/
