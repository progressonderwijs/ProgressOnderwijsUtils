using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ProgressOnderwijsUtils.Data;
using System.Collections;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public sealed class CriteriumFilter : FilterBase, IEquatable<CriteriumFilter>
	{
		readonly string _KolomNaam;
		readonly BooleanComparer _Comparer;
		readonly object _Waarde;

		public string KolomNaam { get { return _KolomNaam; } }
		public BooleanComparer Comparer { get { return _Comparer; } }
		public object Waarde { get { return _Waarde; } }

		public override bool Equals(object obj) { return Equals(obj as CriteriumFilter); }
		public override bool Equals(FilterBase other) { return Equals(other as CriteriumFilter); }
		public bool Equals(CriteriumFilter other) { return other != null && _KolomNaam == other.KolomNaam && _Comparer == other._Comparer && Equals(_Waarde, other._Waarde); }
		public override int GetHashCode()
		{
			return 3 * _KolomNaam.GetHashCode() + 13 * _Comparer.GetHashCode() + 137 * (_Waarde == null ? 0 : _Waarde.GetHashCode());
		}

		public static BooleanComparer[] StringComparers { get { return new[] { BooleanComparer.Contains, BooleanComparer.Equal, BooleanComparer.NotEqual, BooleanComparer.StartsWith, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }
		public static BooleanComparer[] NumericComparers { get { return new[] { BooleanComparer.Equal, BooleanComparer.GreaterThan, BooleanComparer.GreaterThanOrEqual, BooleanComparer.LessThan, BooleanComparer.LessThanOrEqual, BooleanComparer.NotEqual, BooleanComparer.IsNull, BooleanComparer.IsNotNull, BooleanComparer.In }; } }

		internal CriteriumFilter(string kolomnaam, BooleanComparer comparer, object waarde) { _KolomNaam = kolomnaam; _Comparer = comparer; _Waarde = waarde; }

		protected internal override QueryBuilder ToQueryBuilderImpl()
		{
			switch (Comparer)
			{
				case BooleanComparer.LessThan:
					return KolomNaam + "<" + BuildParam();
				case BooleanComparer.LessThanOrEqual:
					return KolomNaam + "<=" + BuildParam();
				case BooleanComparer.Equal:
					return KolomNaam + "=" + BuildParam();
				case BooleanComparer.GreaterThanOrEqual:
					return KolomNaam + ">=" + BuildParam();
				case BooleanComparer.GreaterThan:
					return KolomNaam + ">" + BuildParam();
				case BooleanComparer.NotEqual:
					return KolomNaam + "!=" + BuildParam();
				case BooleanComparer.In:
					if (Waarde is GroupReference)
					{
						return KolomNaam + " in (select keyint0 from statischegroepslid where groep = " + QueryBuilder.Param((Waarde as GroupReference).GroupId) + ")";
					}
					else
					{
						return KolomNaam + " in (select val from " + QueryBuilder.TableParamDynamic((Array)Waarde) + ")";
					}
				case BooleanComparer.StartsWith:
					return KolomNaam + " like " + QueryBuilder.Param(Waarde + "%");
				case BooleanComparer.Contains:
					return KolomNaam + " like " + QueryBuilder.Param("%" + Waarde + "%");
				case BooleanComparer.IsNull:
					return QueryBuilder.Create(KolomNaam + " is null");
				case BooleanComparer.IsNotNull:
					return QueryBuilder.Create(KolomNaam + " is not null");
				default:
					throw new InvalidOperationException("Geen geldige operator");
			}
		}

		public override string SerializeToString()
		{
			Debug.Assert(!KolomNaam.Contains('[') && !KolomNaam.Contains('&') && !KolomNaam.Contains('|') && !KolomNaam.Contains(';') && !KolomNaam.Contains(','));
			Debug.Assert(!Comparer.NiceString().Contains(']'));
			string waardeString = SerializeWaarde();
			waardeString = waardeString.Replace(@"*", @"**");
			return KolomNaam + "[" + Comparer.NiceString() + "]" + waardeString + "*";
		}

		static Tuple<string, string> FindWaardeStrAndLeftover(string s) //TODO: test strings ending with '*';
		{
			int i = 0;
			StringBuilder waardeStr = new StringBuilder();
			while (true)
			{
				if (s[i] != '*')
				{
					waardeStr.Append(s[i]);
					i++;
				}
				else if (s.Length > i + 1 && s[i + 1] == '*')
				{
					waardeStr.Append(s[i]);
					i += 2;
				}
				else
					return Tuple.Create(waardeStr.ToString(), s.Substring(i + 1));
			}
		}



		public static Tuple<FilterBase, string> Parse(string SerializedRep)
		{
			int kolomNaamEnd = SerializedRep.IndexOf('[');
			if (kolomNaamEnd < 0) return null;
			string kolomNaam = SerializedRep.Substring(0, kolomNaamEnd);
			if (kolomNaam.Contains('&') || kolomNaam.Contains('|') || !ColumnReference.IsOkName.IsMatch(kolomNaam))
				return null;
			SerializedRep = SerializedRep.Substring(kolomNaamEnd + 1);
			int cmpEnd = SerializedRep.IndexOf(']');
			if (cmpEnd < 0) return null;
			BooleanComparer? comparer = Filter.ParseComparerNiceString(SerializedRep.Substring(0, cmpEnd));
			if (comparer == null)
				return null;
			SerializedRep = SerializedRep.Substring(cmpEnd + 1);
			var waardeAndRemaining = FindWaardeStrAndLeftover(SerializedRep);
			object waarde = DeserializeWaardeString(waardeAndRemaining.Item1);
			SerializedRep = waardeAndRemaining.Item2;
			return Tuple.Create(Filter.CreateCriterium(kolomNaam, comparer.Value, waarde), SerializedRep);
		}

		string SerializeWaarde()
		{
			if (Waarde == null)
				return "";
			else if (Waarde is int)
				return 'i' + ((int)Waarde).ToStringInvariant();
			else if (Waarde is DateTime)
				return 'd' + ((DateTime)Waarde).ToUniversalTime().Ticks.ToStringInvariant();
			else if (Waarde is long)
				return 'l' + ((long)Waarde).ToStringInvariant();
			else if (Waarde is uint)
				return 'I' + ((uint)Waarde).ToStringInvariant();
			else if (Waarde is ulong)
				return 'L' + ((ulong)Waarde).ToStringInvariant();
			else if (Waarde is string)
				return 's' + (string)Waarde;
			else if (Waarde is ColumnReference)
				return 'c' + ((ColumnReference)Waarde).ColumnName;
			else if (Waarde is GroupReference)
				return 'g' + ((GroupReference)Waarde).GroupId.ToStringInvariant() + ':' + ((GroupReference)Waarde).Name;
			else throw new InvalidOperationException("Waarde is out of range for serialization!");
		}

		static object DeserializeWaardeString(string s)
		{
			if (s == "") return null;
			else switch (s[0])
				{
					case 'i': return int.Parse(s.Substring(1), CultureInfo.InvariantCulture);
					case 'd': return new DateTime(long.Parse(s.Substring(1), CultureInfo.InvariantCulture), DateTimeKind.Utc);
					case 'l': return long.Parse(s.Substring(1), CultureInfo.InvariantCulture);
					case 'I': return uint.Parse(s.Substring(1), CultureInfo.InvariantCulture);
					case 'L': return ulong.Parse(s.Substring(1), CultureInfo.InvariantCulture);
					case 's': return s.Substring(1);
					case 'c': return new ColumnReference(s.Substring(1));
					case 'g': return new GroupReference(int.Parse(s.Substring(1, s.IndexOf(':') - 1), CultureInfo.InvariantCulture), s.Substring(s.IndexOf(':') + 1));
					default: throw new ArgumentOutOfRangeException("s", "string starts with unknown letter " + s[0]);
				}
		}


		QueryBuilder BuildParam()
		{
			if (Waarde is ColumnReference)
				return QueryBuilder.Create(((ColumnReference)Waarde).ColumnName);
			else
				return QueryBuilder.Param(Waarde);
		}

		protected internal override bool IsFilterValid(Func<string, Type> colTypeLookup)
		{
			var primaryType = colTypeLookup(KolomNaam);
			if (primaryType == null)
				return false;
			primaryType = primaryType.StripNullability() ?? primaryType;


			if (Comparer == BooleanComparer.In)
				return Waarde is GroupReference && primaryType == typeof(int) || Waarde is Array && Waarde.GetType().GetElementType() == primaryType;
			if (!(Waarde is ColumnReference))
				return Waarde.GetType() == primaryType;
			Type secondaryType = colTypeLookup(((ColumnReference)Waarde).ColumnName);
			if (secondaryType == null)
				return false;
			secondaryType = secondaryType.StripNullability() ?? secondaryType;
			return secondaryType == primaryType;
		}

		protected internal override FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith) { return this == toReplace ? replaceWith : this; }

		protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
		{
			return filterInEditMode == this
				? Filter.CreateCombined(booleanOperator, this, c)
				: this;
		}

		public static bool StartsWithHelper(string val, string with) { return val.StartsWith(with, StringComparison.Ordinal); }
		public static bool ContainsHelper(string val, string needle) { return val.Contains(needle); }


		static readonly MethodInfo startsWithMethod = ((Func<string, string, bool>)StartsWithHelper).Method;
		static readonly MethodInfo containsMethod = ((Func<string, string, bool>)ContainsHelper).Method;

		protected internal override Expression ToMetaObjectFilterExpr<T>(Expression objParamExpr)
		{
			if (Waarde is GroupReference)
				throw new InvalidOperationException("Cannot interpret group reference IDs in LINQ: these are only stored in the database!");
			Expression coreExpr = Expression.Property(objParamExpr, KolomNaam);
			var waardeExpr = Waarde is ColumnReference ? Expression.Property(objParamExpr, ((ColumnReference)Waarde).ColumnName) : (Expression)Expression.Constant(Waarde);
			if (waardeExpr.Type != coreExpr.Type && coreExpr.Type.IfNullableGetCoreType() == waardeExpr.Type)
				waardeExpr = Expression.Convert(waardeExpr, coreExpr.Type);
			else if (waardeExpr.Type != coreExpr.Type && coreExpr.Type == waardeExpr.Type.IfNullableGetCoreType())
				coreExpr = Expression.Convert(coreExpr, waardeExpr.Type);

			switch (Comparer)
			{
				case BooleanComparer.LessThan:
					return Expression.LessThan(coreExpr, waardeExpr);
				case BooleanComparer.LessThanOrEqual:
					return Expression.LessThanOrEqual(coreExpr, waardeExpr);
				case BooleanComparer.Equal:
					return Expression.Equal(coreExpr, waardeExpr);
				case BooleanComparer.GreaterThanOrEqual:
					return Expression.GreaterThanOrEqual(coreExpr, waardeExpr);
				case BooleanComparer.GreaterThan:
					return Expression.GreaterThan(coreExpr, waardeExpr);
				case BooleanComparer.NotEqual:
					return Expression.NotEqual(coreExpr, waardeExpr);
				case BooleanComparer.In:
					throw new NotImplementedException(); //TODO: de In operatie moet nog...
				//string[] nrs = Waarde.ToString().Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				//string clause = KolomNaam + " in (" + Enumerable.Range(0, nrs.Length).Select(n => "{" + n + "}").JoinStrings(", ") + ")";
				//return QueryBuilder.Create(clause, nrs.Cast<object>().ToArray());
				case BooleanComparer.StartsWith:
					return Expression.Call(startsWithMethod, coreExpr, waardeExpr);
				case BooleanComparer.Contains:
					return Expression.Call(containsMethod, coreExpr, waardeExpr);
				case BooleanComparer.IsNull:
					return Expression.Equal(Expression.Default(typeof(object)), coreExpr);
				case BooleanComparer.IsNotNull:
					return Expression.NotEqual(Expression.Default(typeof(object)), coreExpr);
				default:
					throw new InvalidOperationException("Geen geldige operator");
			}
		}

		public override string ToString()
		{
			if (Waarde is GroupReference && Comparer == BooleanComparer.In)
				return string.Format("{0} in {1}", KolomNaam, (Waarde as GroupReference).Name);
			else if (Waarde is Array && Comparer == BooleanComparer.In)
				return KolomNaam + " in (" + (Waarde as IEnumerable).Cast<object>().Select(o => o == null ? "NULL" : o.ToString()).JoinStrings(", ") + ")";
			else
				return base.ToString();
		}
	}
}
