using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Internal;

namespace ProgressOnderwijsUtils
{
    static class QueryComponent
    {
        public static IQueryComponent CreateString(string val)
        {
            if (val == "") {
                return null;
            } else {
                return new QueryStringComponent(val);
            }
        }

        public static IQueryComponent CreateParam(object o)
        {
            if (o is QueryBuilder) {
                throw new ArgumentException("Cannot pass a querybuilder as a parameter");
            } else if (o is IQueryParameter) {
                return (IQueryComponent)o;
            } else if (o is LiteralSqlInt) {
                return new QueryStringComponent(((LiteralSqlInt)o).Value.ToStringInvariant());
            } else if (o is IEnumerable && !(o is string) && !(o is byte[])) {
                return ToTableParameter((IEnumerable)o);
            } else {
                return new QueryScalarParameterComponent(o);
            }
        }

        public static void AppendParamTo(ref CommandFactory factory, object o)
        {
            if (o is LiteralSqlInt) {
                var literalIntSql = ((LiteralSqlInt)o).Value.ToStringInvariant();
                factory.AppendSql(literalIntSql, 0, literalIntSql.Length);
            } else if (o is IEnumerable && !(o is string) && !(o is byte[])) {
                ToTableParameter((IEnumerable)o).AppendTo(ref factory);
            } else {
                new QueryScalarParameterComponent(o).AppendTo(ref factory);
            }
        }


        public static IQueryComponent ToTableParameter<T>(string tableTypeName, IEnumerable<T> set) where T : IMetaObject, new()
        {
            return new QueryTableValuedParameterComponent<T>(tableTypeName, set);
        }

        static IQueryComponent TryToTableParameter<T>(IEnumerable set)
        {
            if (set is IEnumerable<T>) {
                var typedSet = (IEnumerable<T>)set;
                var projectedSet = typedSet.Select(i => new Internal.DbTableValuedParameterWrapper<T> { querytablevalue = i });
                return ToTableParameter(TableValueTypeName<T>.TypeName, projectedSet);
            } else {
                return null;
            }
        }

        static IQueryComponent TryToEnumTableParameter(IEnumerable set)
        {
            var interfaceType = set
                .GetType()
                .GetInterfaces()
                .SingleOrDefault(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (interfaceType == null) {
                return null;
            }

            var enumType = interfaceType.GetGenericArguments().Single();
            if (!enumType.IsEnum) {
                return null;
            }

            var func = TvpFactoryCache.GetOrAdd(enumType, TvpFactoryFactory);

            return func(set);
        }

        static readonly ConcurrentDictionary<Type, Func<IEnumerable, IQueryComponent>> TvpFactoryCache = new ConcurrentDictionary<Type, Func<IEnumerable, IQueryComponent>>();

        static Func<IEnumerable, IQueryComponent> TvpFactoryFactory(Type enumType)
        {
            var underlyingType = enumType.GetEnumUnderlyingType();
            var specializedMethod = ToEnumTableParameter_Method.MakeGenericMethod(enumType, underlyingType);
            var func = (Func<IEnumerable, IQueryComponent>)Delegate.CreateDelegate(typeof(Func<IEnumerable, IQueryComponent>), specializedMethod);
            return func;
        }

        static readonly MethodInfo ToEnumTableParameter_Method =
            ((Func<IEnumerable, IQueryComponent>)ToEnumTableParameter<int, int>)
                .Method
                .GetGenericMethodDefinition();

        static IQueryComponent ToEnumTableParameter<TEnum, TOut>(IEnumerable set)
        {
            var typedSet = (IEnumerable<TEnum>)set;
            var projectedSet = typedSet.Select(Converter<TEnum, TOut>.Func);

            return ToTableParameter(TableValueTypeName<TOut>.TypeName, projectedSet);
        }

        static class TableValueTypeName<T>
        {
            public static readonly string TypeName = Compute();

            static string Compute()
            {
                if (typeof(int) == typeof(T)) {
                    return "TVar_Int";
                } else if (typeof(string) == typeof(T)) {
                    return "TVar_NVarcharMax";
                } else if (typeof(DateTime) == typeof(T)) {
                    return "TVar_DateTime2";
                } else if (typeof(TimeSpan) == typeof(T)) {
                    return "TVar_Time";
                } else if (typeof(decimal) == typeof(T)) {
                    return "TVar_Decimal";
                } else if (typeof(char) == typeof(T)) {
                    return "TVar_NChar1";
                } else if (typeof(bool) == typeof(T)) {
                    return "TVar_Bit";
                } else if (typeof(byte) == typeof(T)) {
                    return "TVar_Tinyint";
                } else if (typeof(short) == typeof(T)) {
                    return "TVar_Smallint";
                } else if (typeof(long) == typeof(T)) {
                    return "TVar_Bigint";
                } else if (typeof(double) == typeof(T)) {
                    return "TVar_Float";
                }

                throw new InvalidOperationException("Cannot interpret " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(T)) + " as a table valued parameter");
            }
        }

        static class Converter<TInput, TOutput>
        {
            public static readonly Func<TInput, DbTableValuedParameterWrapper<TOutput>> Func = MakeWrappedConverterExpression().Compile();

            static Expression<Func<TInput, DbTableValuedParameterWrapper<TOutput>>> MakeWrappedConverterExpression()
            {
                var parExpr = Expression.Parameter(typeof(TInput), "input");
                var convertExpr = Expression.Convert(parExpr, typeof(TOutput));

                var targetType = typeof(DbTableValuedParameterWrapper<TOutput>);
                var newExpr = Expression.New(targetType);
                var bindingParExpr = Expression.Bind(targetType.GetProperty(nameof(DbTableValuedParameterWrapper<TOutput>.querytablevalue)), convertExpr);
                var initExpr = Expression.MemberInit(newExpr, bindingParExpr);
                return Expression.Lambda<Func<TInput, DbTableValuedParameterWrapper<TOutput>>>(initExpr, parExpr);
            }
        }

        public static IQueryComponent ToTableParameter(IEnumerable set)
        {
            var retval =
                TryToEnumTableParameter(set)
                    ?? TryToTableParameter<int>(set)
                        ?? TryToTableParameter<string>(set)
                            ?? TryToTableParameter<DateTime>(set)
                                ?? TryToTableParameter<TimeSpan>(set)
                                    ?? TryToTableParameter<decimal>(set)
                                        ?? TryToTableParameter<char>(set)
                                            ?? TryToTableParameter<bool>(set)
                                                ?? TryToTableParameter<byte>(set)
                                                    ?? TryToTableParameter<short>(set)
                                                        ?? TryToTableParameter<long>(set)
                                                            ?? TryToTableParameter<double>(set)
                ;
            if (retval == null) {
                throw new ArgumentException("Cannot interpret " + ObjectToCode.GetCSharpFriendlyTypeName(set.GetType()) + " as a table valued parameter", nameof(set));
            }
            return retval;
        }

        /*
        create type TVar_Bigint as table(querytablevalue bigint not null)
        create type TVar_Bit as table(querytablevalue bit not null)
        create type TVar_DateTime2 as table(querytablevalue datetime2(7) not null)
        create type TVar_Decimal as table(querytablevalue decimal(18, 0) not null)
        create type TVar_Float as table(querytablevalue float not null)
        create type TVar_Int as table(querytablevalue int not null, primary key clustered (querytablevalue asc) with (ignore_dup_key = off))
        create type TVar_NChar1 as table(querytablevalue nchar(1) not null)
        create type TVar_NVarcharMax as table(querytablevalue nvarchar(max) not null)
        create type TVar_Smallint as table(querytablevalue smallint not null)
        create type TVar_StudentStudielast as table(studentid int not null, studielast int not null)
        create type TVar_Time as table(querytablevalue time(7) not null)
        create type TVar_Tinyint as table(querytablevalue tinyint not null)

        grant exec on TYPE::TVar_Bigint to [webprogress-readonly]
        grant exec on TYPE::TVar_Bit to [webprogress-readonly]
        grant exec on TYPE::TVar_DateTime2 to [webprogress-readonly]
        grant exec on TYPE::TVar_Decimal to [webprogress-readonly]
        grant exec on TYPE::TVar_Float to [webprogress-readonly]
        grant exec on TYPE::TVar_Int to [webprogress-readonly]
        grant exec on TYPE::TVar_NChar1 to [webprogress-readonly]
        grant exec on TYPE::TVar_NVarcharMax to [webprogress-readonly]
        grant exec on TYPE::TVar_Smallint to [webprogress-readonly]
        grant exec on TYPE::TVar_StudentStudielast to [webprogress-readonly]
        grant exec on TYPE::TVar_Time to [webprogress-readonly]
        grant exec on TYPE::TVar_Tinyint to [webprogress-readonly]

        grant exec on TYPE::TVar_Bigint to [webprogress]
        grant exec on TYPE::TVar_Bit to [webprogress]
        grant exec on TYPE::TVar_DateTime2 to [webprogress]
        grant exec on TYPE::TVar_Decimal to [webprogress]
        grant exec on TYPE::TVar_Float to [webprogress]
        grant exec on TYPE::TVar_Int to [webprogress]
        grant exec on TYPE::TVar_NChar1 to [webprogress]
        grant exec on TYPE::TVar_NVarcharMax to [webprogress]
        grant exec on TYPE::TVar_Smallint to [webprogress]
        grant exec on TYPE::TVar_StudentStudielast to [webprogress]
        grant exec on TYPE::TVar_Time to [webprogress]
        grant exec on TYPE::TVar_Tinyint to [webprogress]
		 

        TODO: once we're using Sql2014, the following types appear to be considerably faster:
         CREATE TYPE TVar_Int AS TABLE ( 
             val int NOT NULL, 
             PRIMARY KEY NONCLUSTERED (val ASC)
         )
         WITH ( MEMORY_OPTIMIZED = ON )
         
         Memory optimized types also need a new MEMORY_OPTIMIZED_DATA filegroup, even if we're not going to store anything in that filegroup.
         
		 */
    }

    namespace Internal
    {
        //public needed for auto-mapping
        public struct DbTableValuedParameterWrapper<T> : IMetaObject
        {
            [Key]
            public T querytablevalue { get; set; }

            public override string ToString() => querytablevalue == null ? "NULL" : querytablevalue.ToString();
        }
    }
}
