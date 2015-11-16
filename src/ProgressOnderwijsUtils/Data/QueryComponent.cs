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

        public static IQueryComponent ToTableParameter<T>(string tableTypeName, IEnumerable<T> set) where T : IMetaObject, new()
        {
            return new QueryTableValuedParameterComponent<T>(tableTypeName, set);
        }

        static IQueryComponent TryToTableParameter<T>(IEnumerable set)
        {
            if (set is IEnumerable<T>) {
                var typedSet = (IEnumerable<T>)set;
                var projectedSet = typedSet.Select(i => new DbTableValuedParameterWrapper<T> { val = i });
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
                var bindingParExpr = Expression.Bind(targetType.GetProperty(nameof(DbTableValuedParameterWrapper<TOutput>.val)), convertExpr);
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
		CREATE TYPE TVar_Int AS TABLE (val int NOT NULL)
		CREATE TYPE TVar_NVarcharMax AS TABLE (val nvarchar(max) NOT NULL)
		CREATE TYPE TVar_DateTime2 AS TABLE (val datetime2 NOT NULL)
		CREATE TYPE TVar_Time AS TABLE (val time NOT NULL)
		CREATE TYPE TVar_Decimal AS TABLE (val decimal NOT NULL)
		CREATE TYPE TVar_NChar1 AS TABLE (val nchar(1) NOT NULL)
		CREATE TYPE TVar_Bit AS TABLE (val bit NOT NULL)
		CREATE TYPE TVar_Tinyint AS TABLE (val tinyint NOT NULL)
		CREATE TYPE TVar_Smallint AS TABLE (val smallint NOT NULL)
		CREATE TYPE TVar_Bigint AS TABLE (val bigint NOT NULL)
		CREATE TYPE TVar_Float AS TABLE (val float NOT NULL)

		GRANT EXECUTE ON TYPE::dbo.TVar_Int TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_NVarcharMax TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_DateTime2 TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Time TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Decimal TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_NChar1 TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Bit TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Tinyint TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Smallint TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Bigint TO public;
		GRANT EXECUTE ON TYPE::dbo.TVar_Float TO public;
		 

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
            public T val { get; set; }

            public override string ToString() => val == null ? "NULL" : val.ToString();
        }
    }
}
