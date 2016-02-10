using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ExpressionToCodeLib;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils
{
    [Serializable]
    public sealed class CriteriumFilter : FilterBase, IEquatable<CriteriumFilter>
    {
        readonly string _KolomNaam;
        readonly BooleanComparer _Comparer;
        readonly object _Waarde;
        public string KolomNaam => _KolomNaam;
        public BooleanComparer Comparer => _Comparer;
        public object Waarde => _Waarde;
        public override bool Equals(object obj) => Equals(obj as CriteriumFilter);
        public override bool Equals(FilterBase other) => Equals(other as CriteriumFilter);

        public bool Equals(CriteriumFilter other)
        {
            return other != null && _KolomNaam == other.KolomNaam && _Comparer == other._Comparer
                && StructuralComparisons.StructuralEqualityComparer.Equals(_Waarde, other._Waarde);
        }

        public override int GetHashCode() => 3 * _KolomNaam.GetHashCode() + 13 * _Comparer.GetHashCode() + 137 * (_Waarde == null ? 0 : _Waarde.GetHashCode());

        public static BooleanComparer[] StringComparers
        {
            get
            {
                return new[] {
                    BooleanComparer.Contains,
                    BooleanComparer.Equal,
                    BooleanComparer.NotEqual,
                    BooleanComparer.StartsWith,
                    BooleanComparer.EndsWith,
                    BooleanComparer.IsNull,
                    BooleanComparer.IsNotNull,
                    BooleanComparer.In,
                    BooleanComparer.NotIn
                };
            }
        }

        public static BooleanComparer[] NumericComparers
        {
            get
            {
                return new[] {
                    BooleanComparer.Equal,
                    BooleanComparer.GreaterThan,
                    BooleanComparer.GreaterThanOrEqual,
                    BooleanComparer.LessThan,
                    BooleanComparer.LessThanOrEqual,
                    BooleanComparer.NotEqual,
                    BooleanComparer.IsNull,
                    BooleanComparer.IsNotNull,
                    BooleanComparer.In,
                    BooleanComparer.NotIn,
                    BooleanComparer.HasFlag,
                };
            }
        }

        public static BooleanComparer[] BooleanComparers
        {
            get
            {
                return new[] { BooleanComparer.Equal, BooleanComparer.NotEqual, BooleanComparer.IsNull, BooleanComparer.IsNotNull, };
            }
        }

        internal CriteriumFilter(string kolomnaam, BooleanComparer comparer, object waarde)
        {
            _KolomNaam = kolomnaam;
            _Comparer = comparer;
            _Waarde = _Comparer == BooleanComparer.IsNotNull || _Comparer == BooleanComparer.IsNotNull ? null : waarde;

            if ((Comparer == BooleanComparer.In || Comparer == BooleanComparer.NotIn)
                && !(Waarde is GroupReference)) {
                try {
                    SqlParameterComponent.ToTableValuedParameterFromPlainValues((Array)Waarde);
                } catch (Exception e) {
                    throw new ArgumentException("Cannot create an in filter with this value", e);
                }
            }
        }

        ParameterizedSql KolomNaamSql() => ParameterizedSql.CreateDynamic(KolomNaam);

        protected internal override ParameterizedSql ToParameterizedSqlImpl()
        {
            switch (Comparer) {
                case BooleanComparer.Equal:
                    if (Waarde == null) {
                        goto case BooleanComparer.IsNull;
                    }
                    return KolomNaamSql() + SQL($"=") + BuildParam();
                case BooleanComparer.IsNull:
                    return KolomNaamSql() + SQL($"is null");
                case BooleanComparer.NotEqual:
                    if (Waarde == null) {
                        goto case BooleanComparer.IsNotNull;
                    }
                    return KolomNaamSql() + SQL($"!=") + BuildParam();
                case BooleanComparer.IsNotNull:
                    return KolomNaamSql() + SQL($"is not null");

                case BooleanComparer.LessThan:
                    return KolomNaamSql() + SQL($"<") + BuildParam();
                case BooleanComparer.LessThanOrEqual:
                    return KolomNaamSql() + SQL($"<=") + BuildParam();
                case BooleanComparer.GreaterThanOrEqual:
                    return KolomNaamSql() + SQL($">=") + BuildParam();
                case BooleanComparer.GreaterThan:
                    return KolomNaamSql() + SQL($">") + BuildParam();

                case BooleanComparer.In:
                    if (Waarde is GroupReference) {
                        return SQL($"{KolomNaamSql()} in (select keyint0 from statischegroepslid where groep = {((GroupReference)Waarde).GroupId})");
                    } else {
                        return SQL($"{KolomNaamSql()} in {ParameterizedSql.TableParamDynamic((Array)Waarde)}");
                    }
                case BooleanComparer.NotIn:
                    if (Waarde is GroupReference) {
                        return SQL($"{KolomNaamSql()} not in (select keyint0 from statischegroepslid where groep = {((GroupReference)Waarde).GroupId})");
                    } else {
                        return SQL($"{KolomNaamSql()} not in {ParameterizedSql.TableParamDynamic((Array)Waarde)}");
                    }

                case BooleanComparer.StartsWith:
                    return SQL($"{KolomNaamSql()} like {Waarde + "%"}");
                case BooleanComparer.EndsWith:
                    return SQL($"{KolomNaamSql()} like {"%" + Waarde}");
                case BooleanComparer.Contains:
                    return SQL($"{KolomNaamSql()} like {"%" + Waarde + "%"}");
                case BooleanComparer.HasFlag:
                    return SQL($"({KolomNaamSql()} & {BuildParam()}) = {BuildParam()}");
                default:
                    throw new InvalidOperationException("Geen geldige operator");
            }
        }

        public override string SerializeToString()
        {
            Debug.Assert(!KolomNaam.Contains('[') && !KolomNaam.Contains('&') && !KolomNaam.Contains('|') && !KolomNaam.Contains(';') && !KolomNaam.Contains(','));
            Debug.Assert(!Comparer.NiceString().Contains(']'));
            if (!ColumnReference.IsOkName.IsMatch(KolomNaam)) {
                throw new Exception("invalid column name");
            }
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

            public ValSerializer(Func<T, string> serialize, Func<string, T> deserialize, char code)
            {
                this.serialize = serialize;
                this.deserialize = deserialize;
                this.code = code;
            }

            public char Code => code;
            public Type Type => typeof(T);
            public string Serialize(object val) => serialize((T)val);
            object IValSerializer.Deserialize(string s) => deserialize(s);
        }

        static IValSerializer Serializer<T>(char code, Func<string, T> deserialize, Func<T, string> serialize)
        {
            return new ValSerializer<T>(serialize, deserialize, code);
        }

        static readonly Dictionary<Type, IValSerializer> serializerByType;
        static readonly Dictionary<char, IValSerializer> serializerByCode;
        static readonly IValSerializer ArraySerializer;

        static CriteriumFilter()
        {
            var helpers = new[] {
                Serializer('b', bool.Parse, b => b.ToString()),
                Serializer('i', s => int.Parse(s, CultureInfo.InvariantCulture), i => i.ToStringInvariant()),
                Serializer('d', s => new DateTime(long.Parse(s, CultureInfo.InvariantCulture), DateTimeKind.Utc), d => d.ToUniversalTime().Ticks.ToStringInvariant()),
                Serializer('t', s => new TimeSpan(long.Parse(s, CultureInfo.InvariantCulture)), t => t.Ticks.ToStringInvariant()),
                Serializer('l', s => long.Parse(s, CultureInfo.InvariantCulture), l => l.ToStringInvariant()),
                Serializer('I', s => uint.Parse(s, CultureInfo.InvariantCulture), I => I.ToStringInvariant()),
                Serializer('L', s => ulong.Parse(s, CultureInfo.InvariantCulture), L => L.ToStringInvariant()),
                Serializer('s', s => s, s => s),
                Serializer('m', s => decimal.Parse(s, CultureInfo.InvariantCulture), m => m.ToStringInvariant()),
                Serializer('f', s => double.Parse(s, CultureInfo.InvariantCulture), f => f.ToStringInvariant()),
                Serializer('c', s => new ColumnReference(s), c => c.ColumnName),
                Serializer('n', Filter.CurrentTimeToken.Parse, n => ""),
                Serializer(
                    'g',
                    s => new GroupReference(int.Parse(s.Substring(0, s.IndexOf(':')), CultureInfo.InvariantCulture), s.Substring(s.IndexOf(':') + 1)),
                    g => g.GroupId.ToStringInvariant() + ':' + g.Name),
                Serializer(
                    '#',
                    s => {
                        var underlying = serializerByCode[s[0]];

                        var elems = new List<object>();
                        string remaining = s.Substring(1); //skip type code
                        while (remaining.Length > 0) {
                            var currentSegment = FindUptoNonDuplicatedTerminatorWithLeftover(remaining, '#');
                            elems.Add(underlying.Deserialize(currentSegment.Item1));
                            if (currentSegment.Item2[0] != ';') {
                                throw new ArgumentOutOfRangeException(nameof(s), "serialized array does not contain ';' where expected, instead" + currentSegment.Item2[0]);
                            }
                            remaining = currentSegment.Item2.Substring(1); //skip ';'
                        }
                        var retval = Array.CreateInstance(underlying.Type, elems.Count);
                        ((IList)elems).CopyTo(retval, 0);
                        return retval;
                    },
                    arr => {
                        var underlying = serializerByType[arr.GetType().GetElementType().GetUnderlyingType()];
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
            if (s == "") {
                return null;
            }

            IValSerializer serializer;
            if (serializerByCode.TryGetValue(s[0], out serializer)) {
                return serializer.Deserialize(s.Substring(1));
            } else {
                throw new ArgumentOutOfRangeException(nameof(s), "string starts with unknown letter " + s[0]);
            }
        }

        static string SerializeWaarde(object waarde)
        {
            if (waarde == null) {
                return "";
            }

            if (waarde is Array) {
                return ArraySerializer.Code + ArraySerializer.Serialize(waarde);
            }

            IValSerializer serializer;
            var underlyingType = waarde is Enum ? waarde.GetType().GetEnumUnderlyingType() : waarde.GetType();

            if (serializerByType.TryGetValue(underlyingType, out serializer)) {
                return serializer.Code + serializer.Serialize(waarde);
            } else {
                throw new ArgumentOutOfRangeException(nameof(waarde), "waarde is van onbekend type " + waarde.GetType());
            }
        }

        static Tuple<string, string> FindUptoNonDuplicatedTerminatorWithLeftover(string s, char terminator) { //TODO: test strings ending with '*';
            int i = 0;
            var waardeStr = new StringBuilder();
            while (true) {
                if (s[i] != terminator) {
                    waardeStr.Append(s[i]);
                    i++;
                } else if (s.Length > i + 1 && s[i + 1] == terminator) {
                    waardeStr.Append(s[i]);
                    i += 2;
                } else {
                    return Tuple.Create(waardeStr.ToString(), s.Substring(i + 1));
                }
            }
        }

        public static Tuple<FilterBase, string> Parse(string SerializedRep)
        {
            int kolomNaamEnd = SerializedRep.IndexOf('[');
            if (kolomNaamEnd < 0) {
                return null;
            }
            string kolomNaam = SerializedRep.Substring(0, kolomNaamEnd);
            if (kolomNaam.Contains('&') || kolomNaam.Contains('|') || !ColumnReference.IsOkName.IsMatch(kolomNaam)) {
                return null;
            }
            SerializedRep = SerializedRep.Substring(kolomNaamEnd + 1);
            int cmpEnd = SerializedRep.IndexOf(']');
            if (cmpEnd < 0) {
                return null;
            }
            BooleanComparer? comparer = Filter.ParseComparerNiceString(SerializedRep.Substring(0, cmpEnd));
            if (comparer == null) {
                return null;
            }
            SerializedRep = SerializedRep.Substring(cmpEnd + 1);
            var waardeAndRemaining = FindUptoNonDuplicatedTerminatorWithLeftover(SerializedRep, '*');
            object waarde = DeserializeWaardeString(waardeAndRemaining.Item1);
            SerializedRep = waardeAndRemaining.Item2;
            return Tuple.Create(Filter.CreateCriterium(kolomNaam, comparer.Value, waarde), SerializedRep);
        }

        ParameterizedSql BuildParam()
        {
            if (Waarde is ColumnReference) {
                return ParameterizedSql.CreateDynamic(((ColumnReference)Waarde).ColumnName);
            } else if (Waarde is Filter.CurrentTimeToken) {
                return ParameterizedSql.Param(DateTime.Now);
            } else {
                return ParameterizedSql.Param(Waarde);
            }
        }

        protected internal override bool IsFilterValid(Func<string, Type> colTypeLookup)
        {
            var primaryType = colTypeLookup(KolomNaam);
            if (primaryType == null) {
                return false;
            }
            primaryType = primaryType.GetNonNullableUnderlyingType();

            if (Comparer == BooleanComparer.IsNotNull || Comparer == BooleanComparer.IsNull) {
                return Waarde == null;
            }
            if (Comparer == BooleanComparer.In || Comparer == BooleanComparer.NotIn) {
                return Waarde is GroupReference && primaryType == typeof(int) || Waarde is Array && Waarde.GetType().GetElementType() == primaryType;
            }
            if (!(Waarde is ColumnReference)) {
                return Waarde == null && primaryType.CanBeNull() || Waarde != null && Waarde.GetType().GetNonNullableUnderlyingType() == primaryType;
            }

            //TODO: when this is reenabled, also update FilterTest.FilterModification() to uncomment the relevant assertion.
            Type secondaryType = colTypeLookup(((ColumnReference)Waarde).ColumnName);
            if (secondaryType == null) {
                return false;
            }
            return secondaryType.GetNonNullableUnderlyingType() == primaryType;
        }

        protected internal override FilterBase ReplaceImpl(FilterBase toReplace, FilterBase replaceWith) => ReferenceEquals(this, toReplace) ? replaceWith : this;

        protected internal override FilterBase AddToImpl(FilterBase filterInEditMode, BooleanOperator booleanOperator, FilterBase c)
        {
            return ReferenceEquals(filterInEditMode, this)
                ? Filter.CreateCombined(booleanOperator, this, c)
                : this;
        }

        // ReSharper disable MemberCanBePrivate.Global
        //not private due to access via Expression trees.
        public static bool StartsWithHelper(string val, string with) => val != null && with != null && val.StartsWith(with, StringComparison.OrdinalIgnoreCase);
        public static bool EndsWithHelper(string val, string with) => val != null && with != null && val.EndsWith(with, StringComparison.OrdinalIgnoreCase);
        public static bool ContainsHelper(string val, string needle) => val != null && needle != null && -1 != val.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        public static bool HasFlagHelper(long val, long flag) => (val & flag) == flag;
        // ReSharper restore MemberCanBePrivate.Global
        static readonly MethodInfo stringStartsWithMethod = ((Func<string, string, bool>)StartsWithHelper).Method;
        static readonly MethodInfo stringEndsWithMethod = ((Func<string, string, bool>)EndsWithHelper).Method;
        static readonly MethodInfo stringContainsMethod = ((Func<string, string, bool>)ContainsHelper).Method;
        static readonly MethodInfo hasFlagMethod = ((Func<long, long, bool>)HasFlagHelper).Method;

        protected internal override Expression ToMetaObjectFilterExpr<T>(
            Expression objParamExpr,
            Expression dateTimeNowTokenValue,
            Func<int, Func<int, bool>> getStaticGroupContainmentVerifier)
        {
            Expression coreExpr = Expression.Property(objParamExpr, KolomNaam);

            if (Comparer == BooleanComparer.In || Comparer == BooleanComparer.NotIn) {
                var inExpr = Waarde is GroupReference
                    ? StaticGroupReferenceMembershipExpression(getStaticGroupContainmentVerifier, coreExpr)
                    : ArrayMembershipExpression(coreExpr);
                return Comparer == BooleanComparer.In ? inExpr : Expression.Not(inExpr);
            } else {
                return ScalarComparerMetaObjectFilterExpr(objParamExpr, dateTimeNowTokenValue, coreExpr);
            }
        }

        Expression ScalarComparerMetaObjectFilterExpr(Expression objParamExpr, Expression dateTimeNowTokenValue, Expression coreExpr)
        {
            var waardeExpr = ConvertedWaardeExpr(objParamExpr, dateTimeNowTokenValue, ref coreExpr);

            return ConstructComparerExpressionTree(coreExpr, waardeExpr, Comparer);
        }

        static Expression ConstructComparerExpressionTree(Expression coreExpr, Expression waardeExpr, BooleanComparer comparer)
        {
            switch (comparer) {
                case BooleanComparer.LessThan:
                    return Expression.LessThan(coreExpr, waardeExpr);
                case BooleanComparer.LessThanOrEqual:
                    return Expression.LessThanOrEqual(coreExpr, waardeExpr);
                case BooleanComparer.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(coreExpr, waardeExpr);
                case BooleanComparer.GreaterThan:
                    return Expression.GreaterThan(coreExpr, waardeExpr);

                case BooleanComparer.IsNull:
                case BooleanComparer.Equal:
                    return Expression.Equal(coreExpr, waardeExpr);

                case BooleanComparer.IsNotNull:
                case BooleanComparer.NotEqual:
                    return Expression.NotEqual(coreExpr, waardeExpr);

                case BooleanComparer.StartsWith:
                    return Expression.Call(stringStartsWithMethod, coreExpr, waardeExpr);
                case BooleanComparer.EndsWith:
                    return Expression.Call(stringEndsWithMethod, coreExpr, waardeExpr);
                case BooleanComparer.Contains:
                    return Expression.Call(stringContainsMethod, coreExpr, waardeExpr);

                case BooleanComparer.HasFlag:
                    return Expression.Call(hasFlagMethod, coreExpr, waardeExpr);

                default:
                    throw new InvalidOperationException("Geen geldige operator");
            }
        }

        Expression ConvertedWaardeExpr(Expression objParamExpr, Expression dateTimeNowTokenValue, ref Expression coreExpr)
        {
            var waardeExpr = Waarde is ColumnReference
                ? Expression.Property(objParamExpr, ((ColumnReference)Waarde).ColumnName)
                : Waarde == Filter.CurrentTimeToken.Instance
                    ? dateTimeNowTokenValue
                    : Waarde == null
                        ? Expression.Default(coreExpr.Type.MakeNullableType() ?? coreExpr.Type)
                        : (Expression)Expression.Constant(Waarde);

            if (ComparerUsesUnderlyingType(Comparer)) {
                coreExpr = CastAwayEnum(coreExpr);
            }

            var waardeIsNullable = waardeExpr.Type.IfNullableGetNonNullableType() != null;
            var colIsNullable = coreExpr.Type.IfNullableGetNonNullableType() != null;

            if (waardeIsNullable != colIsNullable) {
                if (waardeIsNullable) {
                    coreExpr = Expression.Convert(coreExpr, coreExpr.Type.MakeNullableType());
                } else {
                    waardeExpr = Expression.Convert(waardeExpr, waardeExpr.Type.MakeNullableType());
                }
            }

            bool isNullable = colIsNullable || waardeIsNullable;

            if (waardeExpr.Type != coreExpr.Type) { //e.g. enums
                if (waardeExpr.Type.GetNonNullableUnderlyingType() == coreExpr.Type.GetNonNullableUnderlyingType()) {
                    var underlying = waardeExpr.Type.GetNonNullableUnderlyingType();
                    if (isNullable) {
                        underlying = underlying.MakeNullableType();
                    }

                    waardeExpr = Expression.Convert(waardeExpr, underlying);
                    coreExpr = Expression.Convert(coreExpr, underlying);
                } else {
                    throw new InvalidOperationException(
                        "cannot find conversion for column " + KolomNaam + " type " + ObjectToCode.GetCSharpFriendlyTypeName(coreExpr.Type) + " and value '"
                            + ObjectToCode.ComplexObjectToPseudoCode(Waarde) + "'of type " + ObjectToCode.GetCSharpFriendlyTypeName(waardeExpr.Type));
                }
            }
            return waardeExpr;
        }

        static Expression CastAwayEnum(Expression expr)
        {
            var type = expr.Type;
            var underlyingType = type.GetUnderlyingType();
            return type == underlyingType
                ? expr
                : Expression.Convert(expr, underlyingType);
        }

        static bool ComparerUsesUnderlyingType(BooleanComparer comparer)
        {
            return comparer == BooleanComparer.GreaterThan
                || comparer == BooleanComparer.GreaterThanOrEqual
                || comparer == BooleanComparer.LessThan
                || comparer == BooleanComparer.LessThanOrEqual;
        }

        Expression ArrayMembershipExpression(Expression coreExpr)
        {
            var elemType = coreExpr.Type;
            var altElemType = elemType.MakeNullableType() ?? coreExpr.Type.IfNullableGetNonNullableType();
            var listType = typeof(IEnumerable<>).MakeGenericType(elemType);
            var altListType = altElemType == null ? null : typeof(IEnumerable<>).MakeGenericType(altElemType);
            if (!listType.IsInstanceOfType(Waarde) && (altListType == null || !altListType.IsInstanceOfType(Waarde))) {
                throw new InvalidOperationException(
                    "Kan geen IN query maken voor kolom " + KolomNaam + " omdat kolom van type " + elemType + " is en de filter "
                        + (Waarde == null ? "NULL" : "van type " + ObjectToCode.GetCSharpFriendlyTypeName(Waarde.GetType())) + " is.");
            }
            var setType = typeof(ISet<>).MakeGenericType(elemType);
            var genericEnumerableOfTypeMethod = ((Func<IEnumerable, IEnumerable<object>>)Enumerable.OfType<object>).Method.GetGenericMethodDefinition();
            var setExpr = setType.IsInstanceOfType(Waarde)
                ? (Expression)Expression.Constant(Waarde)
                : Expression.New(
                    typeof(HashSet<>).MakeGenericType(elemType).GetConstructor(new[] { listType }),
                    listType.IsInstanceOfType(Waarde)
                        ? Expression.Constant(Waarde)
                        : (Expression)Expression.Call(genericEnumerableOfTypeMethod.MakeGenericMethod(elemType), Expression.Constant(Waarde))
                    )
                ;
            var setContainsMethod = typeof(ICollection<>).MakeGenericType(elemType).GetMethod("Contains");
            // ReSharper disable PossiblyMistakenUseOfParamsMethod
            return Expression.Call(setExpr, setContainsMethod, coreExpr);
            // ReSharper restore PossiblyMistakenUseOfParamsMethod
        }

        Expression StaticGroupReferenceMembershipExpression(Func<int, Func<int, bool>> getStaticGroupContainmentVerifier, Expression coreExpr)
        {
            if (coreExpr.Type.GetNonNullableUnderlyingType() != typeof(int)) {
                throw new InvalidOperationException(
                    "Cannot evaluate a static group for a non-integer column " + KolomNaam + " of type " + ObjectToCode.GetCSharpFriendlyTypeName(coreExpr.Type));
            }

            var membFunc = getStaticGroupContainmentVerifier(((GroupReference)Waarde).GroupId);

            if (coreExpr.Type.CanBeNull()) {
                return Expression.AndAlso(
                    Expression.NotEqual(Expression.Default(coreExpr.Type), coreExpr),
                    Expression.Invoke(Expression.Constant(membFunc), Expression.Convert(Expression.Convert(coreExpr, typeof(int?)), typeof(int))));
            }

            return Expression.Invoke(Expression.Constant(membFunc), Expression.Convert(coreExpr, typeof(int)));
        }

        public override string ToString()
        {
            if (Comparer == BooleanComparer.In || Comparer == BooleanComparer.NotIn) {
                var optionalNot = Comparer == BooleanComparer.NotIn ? " not" : "";
                if (Waarde is GroupReference) {
                    return KolomNaam + optionalNot + " in " + (Waarde as GroupReference).Name;
                } else if (Waarde is Array) {
                    return KolomNaam + optionalNot + " in (" + (Waarde as IEnumerable).Cast<object>().Select(o => o == null ? "NULL" : o.ToString()).JoinStrings(", ") + ")";
                } else {
                    return base.ToString();
                }
            } else {
                return base.ToString();
            }
        }
    }
}
