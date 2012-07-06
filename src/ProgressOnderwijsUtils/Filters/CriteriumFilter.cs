using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
		public bool Equals(CriteriumFilter other)
		{
			return other != null && _KolomNaam == other.KolomNaam && _Comparer == other._Comparer
				&& StructuralComparisons.StructuralEqualityComparer.Equals(_Waarde, other._Waarde);
		}
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
			if (!ColumnReference.IsOkName.IsMatch(KolomNaam))
				throw new Exception("invalid column name");
			string waardeString = SerializeWaarde(Waarde);
			waardeString = waardeString.Replace(@"*", @"**");
			return KolomNaam + "[" + Comparer.NiceString() + "]" + waardeString + "*";
		}


		interface IValSerializer
		{
			Type Type { get; }
			char Code { get; }
			string Serialize(object val);
			object Deserialize(string s);
		}
		sealed class ValSerializer<T> : IValSerializer
		{
			readonly Func<T, string> serialize;
			readonly Func<string, T> deserialize;
			readonly char code;
			public ValSerializer(Func<T, string> serialize, Func<string, T> deserialize, char code) { this.serialize = serialize; this.deserialize = deserialize; this.code = code; }

			public char Code { get { return code; } }
			public Type Type { get { return typeof(T); } }
			public string Serialize(object val) { return serialize((T)val); }
			object IValSerializer.Deserialize(string s) { return deserialize(s); }
		}
		static IValSerializer Serializer<T>(char code, Func<string, T> deserialize, Func<T, string> serialize) { return new ValSerializer<T>(serialize, deserialize, code); }

		static readonly Dictionary<Type, IValSerializer> serializerByType;
		static readonly Dictionary<char, IValSerializer> serializerByCode;
		static readonly IValSerializer ArraySerializer;
		static CriteriumFilter()
		{
			var helpers = new[]{
				Serializer('i', s=>int.Parse(s, CultureInfo.InvariantCulture), i=>i.ToStringInvariant()),
				Serializer('d', s=> new DateTime(long.Parse(s, CultureInfo.InvariantCulture), DateTimeKind.Utc), d=>d.ToUniversalTime().Ticks.ToStringInvariant()),
				Serializer('t', s=> new TimeSpan(long.Parse(s, CultureInfo.InvariantCulture)), t=>t.Ticks.ToStringInvariant()),
				Serializer('l', s=> long.Parse(s, CultureInfo.InvariantCulture), l=>l.ToStringInvariant()),
				Serializer('I', s=>uint.Parse(s, CultureInfo.InvariantCulture), I=>I.ToStringInvariant()),
				Serializer('L', s=>ulong.Parse(s, CultureInfo.InvariantCulture), L=>L.ToStringInvariant()),
				Serializer('s', s=>s, s=>s),
				Serializer('m', s=>decimal.Parse(s, CultureInfo.InvariantCulture), m=>m.ToStringInvariant()),
				Serializer('f', s=>double.Parse(s, CultureInfo.InvariantCulture), f=>f.ToStringInvariant()),
				Serializer('c', s=>new ColumnReference(s), c=>c.ColumnName),
				Serializer('g', s=>new GroupReference(int.Parse(s.Substring(0, s.IndexOf(':')), CultureInfo.InvariantCulture), s.Substring(s.IndexOf(':') + 1)), 
								g=>g.GroupId.ToStringInvariant() + ':' + g.Name),
				Serializer('#', s=> {
					var underlying = serializerByCode[s[0]];

						var elems = new List<object>();
						string remaining = s.Substring(1);//skip type code
						while (remaining.Length > 0)
						{
							var currentSegment = FindUptoNonDuplicatedTerminatorWithLeftover(remaining, '#');
							elems.Add(underlying.Deserialize(currentSegment.Item1));
							if (currentSegment.Item2[0] != ';') throw new ArgumentOutOfRangeException("s", "serialized array does not contain ';' where expected, instead" + currentSegment.Item2[0]);
							remaining = currentSegment.Item2.Substring(1);//skip ';'
						}
						var retval =Array.CreateInstance(underlying.Type, elems.Count);
						((IList)elems).CopyTo(retval,0);
						return retval;
				}, 
								arr=>
								{ var underlying = serializerByType[arr.GetType().GetElementType()];
									return underlying.Code + arr.Cast<object>().Select(elem => underlying.Serialize(elem).Replace("#", "##") + "#;").JoinStrings();
								}
					),

			};
			serializerByType = helpers.ToDictionary(serializer => serializer.Type);
			serializerByCode = helpers.ToDictionary(serializer => serializer.Code);
			ArraySerializer = serializerByType[typeof(Array)];
		}

		static object DeserializeWaardeString(string s)
		{
			if (s == "") return null;

			IValSerializer serializer;
			if (serializerByCode.TryGetValue(s[0], out serializer))
				return serializer.Deserialize(s.Substring(1));
			else
				throw new ArgumentOutOfRangeException("s", "string starts with unknown letter " + s[0]);

		}

		static string SerializeWaarde(object waarde)
		{
			if (waarde == null)
				return "";

			if (waarde is Array)
				return ArraySerializer.Code + ArraySerializer.Serialize(waarde);

			IValSerializer serializer;
			if (serializerByType.TryGetValue(waarde.GetType(), out serializer))
				return serializer.Code + serializer.Serialize(waarde);
			else
				throw new ArgumentOutOfRangeException("waarde", "waarde is van onbekend type " + waarde.GetType());
		}

		static Tuple<string, string> FindUptoNonDuplicatedTerminatorWithLeftover(string s, char terminator) //TODO: test strings ending with '*';
		{
			int i = 0;
			StringBuilder waardeStr = new StringBuilder();
			while (true)
			{
				if (s[i] != terminator)
				{
					waardeStr.Append(s[i]);
					i++;
				}
				else if (s.Length > i + 1 && s[i + 1] == terminator)
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
			var waardeAndRemaining = FindUptoNonDuplicatedTerminatorWithLeftover(SerializedRep, '*');
			object waarde = DeserializeWaardeString(waardeAndRemaining.Item1);
			SerializedRep = waardeAndRemaining.Item2;
			return Tuple.Create(Filter.CreateCriterium(kolomNaam, comparer.Value, waarde), SerializedRep);
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
			if (primaryType == null) return false;
			primaryType = primaryType.GetNonNullableUnderlyingType();

			if (Comparer == BooleanComparer.IsNotNull || Comparer == BooleanComparer.IsNull) return Waarde == null;
			if (Comparer == BooleanComparer.In) return Waarde is GroupReference && primaryType == typeof(int) || Waarde is Array && Waarde.GetType().GetElementType() == primaryType;
			//if (Waarde == null) 		
			if (!(Waarde is ColumnReference))
				return true;	//TODO:emn: HACK? maybe remove this when criterium filters allow it.
			//TODO: when this is reenabled, also update FilterTest.FilterModification() to uncomment the relevant assertion.
			//return CoreType(Waarde.GetType()) == primaryType;
			Type secondaryType = colTypeLookup(((ColumnReference)Waarde).ColumnName);
			if (secondaryType == null) return false;
			return secondaryType.GetNonNullableUnderlyingType() == primaryType;
		}

		protected internal override FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith) { return this == toReplace ? replaceWith : this; }

		protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
		{
			return filterInEditMode == this
				? Filter.CreateCombined(booleanOperator, this, c)
				: this;
		}

		// ReSharper disable MemberCanBePrivate.Global
		//not private due to access via Expression trees.
		public static bool StartsWithHelper(string val, string with) { return val.StartsWith(with, StringComparison.OrdinalIgnoreCase); }
		static readonly MethodInfo startsWithMethod = ((Func<string, string, bool>)StartsWithHelper).Method;
		public static bool ContainsHelper(string val, string needle) { return -1 != val.IndexOf(needle, StringComparison.OrdinalIgnoreCase); }
		static readonly MethodInfo containsMethod = ((Func<string, string, bool>)ContainsHelper).Method;
		// ReSharper restore MemberCanBePrivate.Global

		protected internal override Expression ToMetaObjectFilterExpr<T>(Expression objParamExpr)
		{
			if (Waarde is GroupReference)
				throw new InvalidOperationException("Cannot interpret group reference IDs in LINQ: these are only stored in the database!");
			Expression coreExpr = Expression.Property(objParamExpr, KolomNaam);
			var waardeExpr = Waarde is ColumnReference ? Expression.Property(objParamExpr, ((ColumnReference)Waarde).ColumnName) : (Expression)Expression.Constant(Waarde);
			if (waardeExpr.Type != coreExpr.Type && coreExpr.Type.IfNullableGetNonNullableType() == waardeExpr.Type)
				waardeExpr = Expression.Convert(waardeExpr, coreExpr.Type);
			else if (waardeExpr.Type != coreExpr.Type && coreExpr.Type == waardeExpr.Type.IfNullableGetNonNullableType())
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
					return Expression.Equal(Expression.Convert(Expression.Default(typeof(object)), coreExpr.Type), coreExpr);
				case BooleanComparer.IsNotNull:
					return Expression.NotEqual(Expression.Convert(Expression.Default(typeof(object)), coreExpr.Type), coreExpr);
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
