// ReSharper disable PossiblyMistakenUseOfParamsMethod

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Collections;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
	public static class AutoLoadFromDb
	{
		public static T ReadScalar<T>(this QueryBuilder builder, QueryBuilder.ToSqlArgs args)
		{
			return builder.CreateSqlCommand(args).Using(command => DBNullRemover.Cast<T>(command.ExecuteScalar()));
		}

		public static int ExecuteNonQuery(this QueryBuilder builder, QueryBuilder.ToSqlArgs args)
		{
			return builder.CreateSqlCommand(args).Using(
				command => {
					try
					{
						return command.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						throw new NietZoErgeException("Non-query failed " + command.CommandText, ex);
					}
				});
		}


		/// <summary>
		/// Reads all records of the given query from the database, unpacking into a C# array using each item's constructor.
		/// Supports structs and classes.
		/// Type T must have a constructor whose parameters match the columns of the query.  Matching is case insensitive.  The order of the columns must be the same.
		/// </summary>
		/// <typeparam name="T">The type to unpack each record into</typeparam>
		/// <param name="q">The query to execute</param>
		/// <param name="conn">The database connection</param>
		/// <returns>An array of strongly-typed objects; never null</returns>
		public static T[] ReadByConstructor<T>(this QueryBuilder q, QueryBuilder.ToSqlArgs qArgs) where T : IReadByConstructor
		{
			using (var cmd = q.CreateSqlCommand(qArgs))
				return ReadByConstructorUnpacker<T>(cmd);
		}

		public static T[] ReadByConstructorUnpacker<T>(SqlCommand cmd) where T : IReadByConstructor
		{
			using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				DataReaderSpecialization<SqlDataReader>.Impl<T>.VerifyDataReaderShape(reader);
				return DataReaderSpecialization<SqlDataReader>.Impl<T>.LoadRows(reader);
			}
		}

		/// <summary>
		/// Reads all records of the given query from the database, unpacking into a C# array using each item's publicly writable fields and properties.
		/// Type T must have a public parameterless constructor; both structs and classes are supported
		/// The type T must match the queries columns by name (the order is not relevant).  Matching columns to properties/fields is case insensitive.
		/// The number of fields+properties must be the same as the number of columns
		/// </summary>
		/// <typeparam name="T">The type to unpack each record into</typeparam>
		/// <param name="q">The query to execute</param>
		/// <param name="conn">The database connection</param>
		/// <returns>An array of strongly-typed objects; never null</returns>
		public static T[] ReadByFields<T>(this QueryBuilder q, QueryBuilder.ToSqlArgs qArgs) where T : IReadByFields, new()
		{
			using (var cmd = q.CreateSqlCommand(qArgs))
				return ReadByFieldsUnpacker<T>(cmd);
		}

		public static T[] ReadByFieldsUnpacker<T>(SqlCommand cmd) where T : IReadByFields, new()
		{
			using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				var unpacker = DataReaderSpecialization<SqlDataReader>.ByFieldImpl<T>.GetDataReaderUnpacker(reader);
				return unpacker(reader);
			}
		}

		/// <summary>
		/// Reads all records of the given query from the database, unpacking into a C# array using basic types (i.e. scalars)
		/// Type T must be int, long, string, decimal, double, bool, DateTime or byte[].
		/// </summary>
		/// <typeparam name="T">The type to unpack each record into</typeparam>
		/// <param name="q">The query to execute</param>
		/// <param name="conn">The database connection</param>
		/// <returns>An array of strongly-typed objects; never null</returns>
		public static T[] ReadPlain<T>(this QueryBuilder q, QueryBuilder.ToSqlArgs qArgs)
		{
			using (var cmd = q.CreateSqlCommand(qArgs))
				return ReadPlainUnpacker<T>(cmd);
		}

		public static T[] ReadPlainUnpacker<T>(SqlCommand cmd)
		{
			using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.VerifyDataReaderShape(reader);
				return DataReaderSpecialization<SqlDataReader>.PlainImpl<T>.LoadRows(reader);
			}
		}

		/// <summary>
		/// Overloaded; see primary overload for details.  This overload unpacks two recordsets; i.e. two subsequent SELECT statements.
		/// It's equivalent to but faster than Tuple.Create(queryA.ReadByConstructor&lt;T1&gt;(conn), queryB.ReadByConstructor&lt;T2&gt;(conn))
		/// </summary>
		public static Tuple<T1[], T2[]> ReadByConstructor<T1, T2>(this QueryBuilder q, QueryBuilder.ToSqlArgs qArgs)
			where T1 : IReadByConstructor
			where T2 : IReadByConstructor
		{
			using (var cmd = q.CreateSqlCommand(qArgs))
				return ReadByConstructor<T1, T2>(cmd);
		}

		public static Tuple<T1[], T2[]> ReadByConstructor<T1, T2>(SqlCommand cmd)
			where T1 : IReadByConstructor
			where T2 : IReadByConstructor
		{
			using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				DataReaderSpecialization<SqlDataReader>.Impl<T1>.VerifyDataReaderShape(reader);
				var arr1 = DataReaderSpecialization<SqlDataReader>.Impl<T1>.LoadRows(reader);
				if (!reader.NextResult())
					throw new QueryException("Cannot load second result set (type " + ObjectToCode.GetCSharpFriendlyTypeName(typeof(T2)) + ")\nQuery:\n\n" + cmd.CommandText);
				DataReaderSpecialization<SqlDataReader>.Impl<T2>.VerifyDataReaderShape(reader);
				var arr2 = DataReaderSpecialization<SqlDataReader>.Impl<T2>.LoadRows(reader);
				return Tuple.Create(arr1, arr2);
			}
		}

		const BindingFlags binding = BindingFlags.Public | BindingFlags.Instance;

		static readonly Dictionary<Type, MethodInfo> GetterMethodsByType =
			new Dictionary<Type, MethodInfo> {
					{ typeof(int), typeof(IDataRecord).GetMethod("GetInt32", binding) },
					{ typeof(long), typeof(IDataRecord).GetMethod("GetInt64", binding) },
					{ typeof(string), typeof(IDataRecord).GetMethod("GetString", binding) },
					{ typeof(decimal), typeof(IDataRecord).GetMethod("GetDecimal", binding) },
					{ typeof(double), typeof(IDataRecord).GetMethod("GetDouble", binding) },
					{ typeof(bool), typeof(IDataRecord).GetMethod("GetBoolean", binding) },
					{ typeof(DateTime), typeof(IDataRecord).GetMethod("GetDateTime", binding) },
					{ typeof(byte[]), typeof(DbLoadingHelperImpl).GetMethod("GetBytes", BindingFlags.Public | BindingFlags.Static) },
				};

		//static bool SupportsType(Type type) { return GetterMethodsByType.ContainsKey(type); }
		//static MethodInfo GetterForType(Type type) { return GetterMethodsByType[type]; }

		//static readonly MethodInfo IsDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", binding);
		//static readonly MethodInfo ReadMethod = typeof(IDataReader).GetMethod("Read", binding);

		static Dictionary<MethodInfo, MethodInfo> MakeMap(params InterfaceMapping[] mappings)
		{
			return mappings.SelectMany(
				map => map.InterfaceMethods.Zip(map.TargetMethods, Tuple.Create))
				.ToDictionary(methodPair => methodPair.Item1, methodPair => methodPair.Item2);
		}
		static readonly AssemblyBuilder assemblyBuilder;
		static readonly ModuleBuilder moduleBuilder;
		static int counter;

		static AutoLoadFromDb()
		{
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("AutoLoadFromDb_Helper"), AssemblyBuilderAccess.Run);
			moduleBuilder = assemblyBuilder.DefineDynamicModule("AutoLoadFromDb_HelperModule");
		}


		static class DataReaderSpecialization<TReader> where TReader : IDataReader
		{
			static readonly Dictionary<MethodInfo, MethodInfo> InterfaceMap = MakeMap(typeof(TReader).GetInterfaceMap(typeof(IDataRecord)), typeof(TReader).GetInterfaceMap(typeof(IDataReader)));
			static readonly MethodInfo IsDBNullMethod = InterfaceMap[typeof(IDataRecord).GetMethod("IsDBNull", binding)];
			static readonly MethodInfo ReadMethod = InterfaceMap[typeof(IDataReader).GetMethod("Read", binding)];
			static bool SupportsType(Type type) { return GetterMethodsByType.ContainsKey(type.GetNonNullableUnderlyingType()); }
			static MethodInfo GetterForType(Type underlyingType) { return InterfaceMap[GetterMethodsByType[underlyingType]]; }

			static Expression GetColValueExpr(ParameterExpression readerParamExpr, int i, Type type)
			{
				bool canBeNull = type.CanBeNull();
				Type underlyingType = type.GetNonNullableUnderlyingType();
				bool needsCast = underlyingType != type.GetNonNullableType();
				var iConstant = Expression.Constant(i);
				var callExpr = underlyingType == typeof(byte[]) ? Expression.Call(GetterMethodsByType[underlyingType], readerParamExpr, iConstant) : Expression.Call(readerParamExpr, GetterForType(underlyingType), iConstant);
				var castExpr = !needsCast ? (Expression)callExpr : Expression.Convert(callExpr, type.GetNonNullableType());
				var colValueExpr = !canBeNull ? castExpr :
					Expression.Condition(
						Expression.Call(readerParamExpr, IsDBNullMethod, iConstant), Expression.Default(type),
						Expression.Convert(castExpr, type));
				return colValueExpr;
			}

			static Func<TReader, T[]> CreateLoadRowsMethod<T>(Func<ParameterExpression, Expression> createRowObjectExpression)
			{
				var dataReaderParamExpr = Expression.Parameter(typeof(TReader), "dataReader");
				var listType = typeof(FastArrayBuilder<T>);
				var listVarExpr = Expression.Variable(listType, "rowList");

				var listAssignment = Expression.Assign(listVarExpr, Expression.Call(listType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)));

				var constructRowExpr = createRowObjectExpression(dataReaderParamExpr);
				var addRowExpr = Expression.Call(listVarExpr, listType.GetMethod("Add", new[] { typeof(T) }), constructRowExpr);

				var loopExitLabel = Expression.Label("loopExit");
				var rowLoopExpr =
					Expression.Loop(
						Expression.IfThenElse(
							Expression.Call(dataReaderParamExpr, ReadMethod),
							addRowExpr,
							Expression.Break(loopExitLabel)
							),
						loopExitLabel
						);

				var listToArrayExpr = Expression.Call(listVarExpr, listType.GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance));

				//LoadRows = 
				var loadRowFunc = Expression.Lambda<Func<TReader, T[]>>(
					Expression.Block(
						typeof(T[]),
						new[] { listVarExpr },
						listAssignment,
					//listInit,
						rowLoopExpr,
						listToArrayExpr
						),
					"LoadRows",
					new[] { dataReaderParamExpr }
					);

				TypeBuilder typeBuilder = moduleBuilder.DefineType("AutoLoadFromDb_For_" + typeof(T).Name + "_" + typeof(TReader).Name + Interlocked.Increment(ref counter), TypeAttributes.Public);
				var methodBuilder = typeBuilder.DefineMethod("LoadRows", MethodAttributes.Public | MethodAttributes.Static);
				try
				{
					if (typeof(T).IsPublic)
						loadRowFunc.CompileToMethod(methodBuilder); //faster
					else
						return loadRowFunc.Compile();
				}
				catch (Exception e)
				{
					throw new ProgressNetException("Cannot dynamically compile unpacker method for type " + typeof(T) + ", where type.IsPublic: " + typeof(T).IsPublic, e);
				}
				var newType = typeBuilder.CreateType();

				var loadRows = (Func<TReader, T[]>)Delegate.CreateDelegate(typeof(Func<TReader, T[]>), newType.GetMethod("LoadRows"));
				return loadRows;
			}

			public static class ByFieldImpl<T>
	where T : IReadByFields, new()
			{

				sealed class ColumnOrdering : IEquatable<ColumnOrdering>
				{
					public readonly string[] Cols;
					readonly ulong cachedHash;
					public ColumnOrdering(TReader reader)
					{
						var primeArr = ColHashPrimes;
						Cols = new string[primeArr.Length];
						cachedHash = 0;
						for (int i = 0; i < Cols.Length; i++)
						{
							var name = reader.GetName(i);
							Cols[i] = name;
							cachedHash += (ulong)primeArr[i] * (uint)StringComparer.OrdinalIgnoreCase.GetHashCode(name);
						}
					}

					public bool Equals(ColumnOrdering other) { return cachedHash == other.cachedHash && Cols.SequenceEqual(other.Cols); }
					public override int GetHashCode() { return (int)(uint)((cachedHash >> 32) + cachedHash); }
					public override bool Equals(object obj) { return obj is ColumnOrdering && Equals((ColumnOrdering)obj); }
				}

				static readonly ConcurrentDictionary<ColumnOrdering, Func<TReader, T[]>> LoadRows;

				static Type type { get { return typeof(T); } }
				static string FriendlyName { get { return ObjectToCode.GetCSharpFriendlyTypeName(type); } }
				static readonly Dictionary<string, MemberInfo> GetMember;
				static readonly uint[] ColHashPrimes;
				static Type MemberType(MemberInfo mi)
				{
					return mi is FieldInfo ? ((FieldInfo)mi).FieldType : ((PropertyInfo)mi).PropertyType;
				}

				static ByFieldImpl()
				{
					GetMember = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);
					try
					{
						foreach (var fi in PublicFields())
							GetMember.Add(fi.Name, fi);
						foreach (var pi in PublicProperties())
							GetMember.Add(pi.Name, pi);
					}
					catch (ArgumentException argE)
					{
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByFields's writable fields & properties must have a case insensitively unique name", argE);
					}

					ColHashPrimes = Utils.Primes().Take(GetMember.Count).Select(i => (uint)i).ToArray();

					VerifyTypeValidity();
					LoadRows = new ConcurrentDictionary<ColumnOrdering, Func<TReader, T[]>>();
					//LoadRows = CreateLoadRowsMethod<T>(readerParamExpr =>
					//	Expression.New(constructor, ConstructorParameters.Select((ci, i) => GetColValueExpr(readerParamExpr, i, ci.ParameterType)))
					//	);
				}

				static IEnumerable<PropertyInfo> PublicProperties() { return type.GetProperties().Where(pi => pi.CanWrite && pi.GetSetMethod() != null); }
				static IEnumerable<FieldInfo> PublicFields() { return type.GetFields().Where(fi => !fi.Attributes.HasFlag(FieldAttributes.InitOnly)); }

				static void VerifyTypeValidity()
				{

					if (!type.IsSealed)
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByFields must be a public, sealed type!");

#if false
					if (!type.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(mi => mi.Name == "DbQuery"))
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByFields must have a public static method DbQuery that returns a QueryBuilder");
					if (type.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(mi => mi.Name == "DbQuery" && mi.ReturnType != typeof(QueryBuilder)))
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByFields's DbQuery does not return QueryBuilder");
#endif
					if (type.GetConstructors().All(ci => ci.GetParameters().Any()) && !type.IsValueType)
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByFields must have a parameterless public constructor.");


					if (!GetMember.Values.All(mi => SupportsType(MemberType(mi))))
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByFields's writable fields & properties must have only simple types: cannot support "
							+ GetMember.Where(miKV => !SupportsType(MemberType(miKV.Value))).Select(miKV => ObjectToCode.GetCSharpFriendlyTypeName(MemberType(miKV.Value)) + " " + miKV.Key).JoinStrings(", "));

				}


				public static Func<TReader, T[]> GetDataReaderUnpacker(TReader reader)
				{
					if (reader.FieldCount != ColHashPrimes.Length)
						throw new InvalidOperationException("Cannot unpack DbDataReader into type " + FriendlyName + "; column count = " + reader.FieldCount + "; field count = " + ColHashPrimes.Length);
					var ordering = new ColumnOrdering(reader);

					return LoadRows.GetOrAdd(ordering, orderingP =>
						CreateLoadRowsMethod<T>(readerParamExpr =>
							Expression.MemberInit(
								Expression.New(type),
								orderingP.Cols.Select((colName, i) => {
									MemberInfo member;
									if (!GetMember.TryGetValue(colName, out member))
										throw new ArgumentOutOfRangeException("Cannot resolve IDataReader column " + colName + " in type " + FriendlyName);
									return Expression.Bind(member, GetColValueExpr(readerParamExpr, i, MemberType(member)));
								}))));
				}
			}


			public static class Impl<T>
				where T : IReadByConstructor
			{
				public static readonly Func<TReader, T[]> LoadRows;

				static Type type { get { return typeof(T); } }
				static string FriendlyName { get { return ObjectToCode.GetCSharpFriendlyTypeName(type); } }
				static readonly ConstructorInfo constructor;
				static ParameterInfo[] ConstructorParameters { get { return constructor.GetParameters(); } }

				static Impl()
				{
					constructor = VerifyTypeValidityAndGetConstructor();
					LoadRows = CreateLoadRowsMethod<T>(readerParamExpr => Expression.New(constructor, ConstructorParameters.Select((ci, i) => GetColValueExpr(readerParamExpr, i, ci.ParameterType))));
				}

				static ConstructorInfo VerifyTypeValidityAndGetConstructor()
				{
					if (!type.IsSealed || !type.IsPublic)
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor must be a public, sealed type.");

#if false
					if (!type.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(mi => mi.Name == "DbQuery"))
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor must have a public static method DbQuery that returns a QueryBuilder");
					if (type.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(mi => mi.Name == "DbQuery" && mi.ReturnType != typeof(QueryBuilder)))
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor's DbQuery does not return QueryBuilder");
#endif

					//if (type.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(mi => mi.Name == "DbQuery" && !mi.GetParameters().All(SupportsParameter)))
					//	throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor's DbQuery may only have simple types as parameters (" + GetterMethodsByType.Keys.Select(t => ObjectToCode.GetCSharpFriendlyTypeName(t)).JoinStrings(", ") + ")");

					var constructors = type.GetConstructors().Where(ci => ci.GetParameters().Any()).ToArray();
					if (constructors.Length != 1)
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor must have a single public constructor (not counting a structs implicit constructor), not " + constructors.Length);
					var retval = constructors.Single();

					if (!retval.GetParameters().All(pi => SupportsType(pi.ParameterType)))
						throw new ArgumentException(FriendlyName + " : ILoadFromDbByConstructor's constructor must have only simple types: cannot support " + retval.GetParameters().Where(pi => !SupportsType(pi.ParameterType)).Select(pi => ObjectToCode.GetCSharpFriendlyTypeName(pi.ParameterType) + " " + pi.Name).JoinStrings(", "));
					return retval;
				}

				public static void VerifyDataReaderShape(TReader reader)
				{
					if (reader.FieldCount != ConstructorParameters.Length)
						throw new InvalidOperationException("Cannot unpack DbDataReader into type " + FriendlyName + "; column count = " + reader.FieldCount + "; constructr parameter count = " + ConstructorParameters.Length);
					if (!Enumerable.Range(0, reader.FieldCount).Select(reader.GetName)
						.SequenceEqual(ConstructorParameters.Select(ci => ci.Name), StringComparer.OrdinalIgnoreCase)
						||
						!Enumerable.Range(0, reader.FieldCount).Select(reader.GetFieldType)
							.SequenceEqual(ConstructorParameters.Select(ci => ci.ParameterType.GetNonNullableUnderlyingType())))
						throw new InvalidOperationException("Cannot unpack DbDataReader:\n"
							+ Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i) + " : " + ObjectToCode.GetCSharpFriendlyTypeName(reader.GetFieldType(i))).JoinStrings(", ") + "\n\t into type " + FriendlyName + ":\n"
							+ ConstructorParameters.Select(ci => ci.Name + " : " + ObjectToCode.GetCSharpFriendlyTypeName(ci.ParameterType)).JoinStrings(", "));
				}
			}


			public static class PlainImpl<T>
			{
				static string FriendlyName { get { return ObjectToCode.GetCSharpFriendlyTypeName(type); } }
				public static readonly Func<TReader, T[]> LoadRows;
				static Type type { get { return typeof(T); } }

				static PlainImpl()
				{
					VerifyTypeValidity();
					LoadRows = CreateLoadRowsMethod<T>(readerParamExpr => GetColValueExpr(readerParamExpr, 0, type));
				}

				static void VerifyTypeValidity()
				{
					if (!SupportsType(type))
						throw new ArgumentException(FriendlyName + " cannot be auto loaded as plain data since it isn't a basic type (" + GetterMethodsByType.Keys.Select(ObjectToCode.GetCSharpFriendlyTypeName).JoinStrings(", ") + ")!");
				}

				public static void VerifyDataReaderShape(TReader reader)
				{
					if (reader.FieldCount != 1)
						throw new InvalidOperationException("Cannot unpack DbDataReader into type " + FriendlyName + "; column count = " + reader.FieldCount + " != 1");
					if (!Enumerable.Range(0, reader.FieldCount).Select(reader.GetFieldType)
						.SequenceEqual(new[] { typeof(T).GetNonNullableUnderlyingType() }))
						throw new InvalidOperationException("Cannot unpack DbDataReader into type " + FriendlyName + ":\n"
							+ Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i) + " : " + ObjectToCode.GetCSharpFriendlyTypeName(reader.GetFieldType(i))).JoinStrings(", "));
				}
			}
		}
	}


	public static class DbLoadingHelperImpl
	{
		[UsedImplicitly]
		public static byte[] GetBytes(this IDataRecord row, int colIndex)
		{
			long byteCount = row.GetBytes(colIndex, 0L, null, 0, 0);
			if (byteCount > int.MaxValue) throw new NotSupportedException("Array too large!");
			var arr = new byte[byteCount];
			long offset = 0;
			while (offset < byteCount)
				offset += row.GetBytes(colIndex, offset, arr, (int)offset, (int)byteCount);
			return arr;
		}
	}

#if LINQPAD_BENCHMARK
	public struct VolgOnderwijsOnderwijsVal : ILoadFromDbByConstructor
	{
		public readonly int? Ouder;
		public readonly int Kind;
		public readonly int? Student;
		public readonly bool Verplicht;
		public readonly bool Verwijderd;
		public readonly int? Afgeleidvan;
		public readonly int? Volgorde;
		public readonly int Onderwijsonderwijsid;
		public readonly int? Periodevan;
		public readonly int? Periodetot;
		public VolgOnderwijsOnderwijsVal(int? ouder, int kind, int? student, bool verplicht, bool verwijderd, int? afgeleidvan, int? volgorde, int onderwijsonderwijsid, int? periodevan, int? periodetot)
		{
			Ouder = ouder;
			Kind = kind;
			Student = student;
			Verplicht = verplicht;
			Verwijderd = verwijderd;
			Afgeleidvan = afgeleidvan;
			Volgorde = volgorde;
			Onderwijsonderwijsid = onderwijsonderwijsid;
			Periodevan = periodevan;
			Periodetot = periodetot;
		}

		public static QueryBuilder DbQuery()
		{
			return QueryBuilder.Create(@"select * from volgonderwijsonderwijs");
		}
	}


	public sealed class VolgOnderwijsOnderwijsMO : IMetaObject
	{
		public int? Ouder { get; set; }
		public int Kind { get; set; }
		public int? Student { get; set; }
		public bool Verplicht { get; set; }
		public bool Verwijderd { get; set; }
		public int? Afgeleidvan { get; set; }
		public int? Volgorde { get; set; }
		public int Onderwijsonderwijsid { get; set; }
		public int? Periodevan { get; set; }
		public int? Periodetot { get; set; }
	}



	public sealed class StudentInschrijvingVal : ILoadFromDbByConstructor
	{
		public readonly int Inschrijvingid;
		public readonly int Student;
		public readonly int Opleiding;
		public readonly int Fase;
		public readonly int Soort;
		public readonly int Periodestudiejaar;
		public readonly DateTime Datumvan;
		public readonly DateTime Datumtot;
		public readonly int Bekostiging;
		public readonly bool? Nietherinschrijven;
		public readonly int? Aanmeldingstatus;
		public readonly DateTime? Aanmeldingdatum;
		public readonly int? Aanmeldingvooropleiding;
		public readonly int? Aanmeldingdeficientiestatus;
		public readonly bool? Aanmeldingeerstejaar;
		public readonly bool? Aanmeldingverzoekintrekken;
		public readonly int? Aanmeldingibgstatus;
		public readonly int? Aanmeldingibgredenafkeur;
		public readonly int? Beeindigingreden;
		public readonly DateTime? Beeindigingdatumingediend;
		public readonly DateTime? Beeindigingdatum;
		public readonly string Beeindigingopmerkingstudent;
		public readonly string Beeindigingopmerkingadministratie;
		public readonly bool? Beeindigingverzoekrestitutie;
		public readonly bool? Beeindigingaccoord;
		public readonly DateTime? Examendatum;
		public readonly int? Examenjudicium;
		public readonly int? Examencode;
		public readonly DateTime? Examenaanvraagdatum;
		public readonly DateTime? Examenjudiciumdatum;
		public readonly bool? Flex;
		public readonly bool? Kop;
		public readonly bool? Offline;
		public readonly int? Bekostigingtoegekend;
		public readonly int? Bekostigingexamentoegekend;
		public readonly DateTime? Datumdefinitief;
		public readonly DateTime? Beeindigingdatumgewenst;
		public readonly decimal? Collegegeld;
		public readonly bool? Collegegeldhandmatig;
		public readonly bool? Bijvak;
		public readonly string Opmerking;
		public readonly DateTime? Voorlopigetoelatingtot;
		public readonly DateTime? Voorlopigetoelatingvan;
		public readonly bool? Bsaselect;
		public readonly string Aanmeldingstudielinkstartoccasion;
		public readonly int? Duoindicatiecollegegeld;
		public readonly string Diplomanummer;
		public readonly int? Bsacluster;
		public StudentInschrijvingVal(int inschrijvingid, int student, int opleiding, int fase, int soort, int periodestudiejaar, DateTime datumvan, DateTime datumtot, int bekostiging, bool? nietherinschrijven, int? aanmeldingstatus, DateTime? aanmeldingdatum, int? aanmeldingvooropleiding, int? aanmeldingdeficientiestatus, bool? aanmeldingeerstejaar, bool? aanmeldingverzoekintrekken, int? aanmeldingibgstatus, int? aanmeldingibgredenafkeur, int? beeindigingreden, DateTime? beeindigingdatumingediend, DateTime? beeindigingdatum, string beeindigingopmerkingstudent, string beeindigingopmerkingadministratie, bool? beeindigingverzoekrestitutie, bool? beeindigingaccoord, DateTime? examendatum, int? examenjudicium, int? examencode, DateTime? examenaanvraagdatum, DateTime? examenjudiciumdatum, bool? flex, bool? kop, bool? offline, int? bekostigingtoegekend, int? bekostigingexamentoegekend, DateTime? datumdefinitief, DateTime? beeindigingdatumgewenst, decimal? collegegeld, bool? collegegeldhandmatig, bool? bijvak, string opmerking, DateTime? voorlopigetoelatingtot, DateTime? voorlopigetoelatingvan, bool? bsaselect, string aanmeldingstudielinkstartoccasion, int? duoindicatiecollegegeld, string diplomanummer, int? bsacluster)
		{
			Inschrijvingid = inschrijvingid;
			Student = student;
			Opleiding = opleiding;
			Fase = fase;
			Soort = soort;
			Periodestudiejaar = periodestudiejaar;
			Datumvan = datumvan;
			Datumtot = datumtot;
			Bekostiging = bekostiging;
			Nietherinschrijven = nietherinschrijven;
			Aanmeldingstatus = aanmeldingstatus;
			Aanmeldingdatum = aanmeldingdatum;
			Aanmeldingvooropleiding = aanmeldingvooropleiding;
			Aanmeldingdeficientiestatus = aanmeldingdeficientiestatus;
			Aanmeldingeerstejaar = aanmeldingeerstejaar;
			Aanmeldingverzoekintrekken = aanmeldingverzoekintrekken;
			Aanmeldingibgstatus = aanmeldingibgstatus;
			Aanmeldingibgredenafkeur = aanmeldingibgredenafkeur;
			Beeindigingreden = beeindigingreden;
			Beeindigingdatumingediend = beeindigingdatumingediend;
			Beeindigingdatum = beeindigingdatum;
			Beeindigingopmerkingstudent = beeindigingopmerkingstudent;
			Beeindigingopmerkingadministratie = beeindigingopmerkingadministratie;
			Beeindigingverzoekrestitutie = beeindigingverzoekrestitutie;
			Beeindigingaccoord = beeindigingaccoord;
			Examendatum = examendatum;
			Examenjudicium = examenjudicium;
			Examencode = examencode;
			Examenaanvraagdatum = examenaanvraagdatum;
			Examenjudiciumdatum = examenjudiciumdatum;
			Flex = flex;
			Kop = kop;
			Offline = offline;
			Bekostigingtoegekend = bekostigingtoegekend;
			Bekostigingexamentoegekend = bekostigingexamentoegekend;
			Datumdefinitief = datumdefinitief;
			Beeindigingdatumgewenst = beeindigingdatumgewenst;
			Collegegeld = collegegeld;
			Collegegeldhandmatig = collegegeldhandmatig;
			Bijvak = bijvak;
			Opmerking = opmerking;
			Voorlopigetoelatingtot = voorlopigetoelatingtot;
			Voorlopigetoelatingvan = voorlopigetoelatingvan;
			Bsaselect = bsaselect;
			Aanmeldingstudielinkstartoccasion = aanmeldingstudielinkstartoccasion;
			Duoindicatiecollegegeld = duoindicatiecollegegeld;
			Diplomanummer = diplomanummer;
			Bsacluster = bsacluster;
		}

		public static QueryBuilder DbQuery()
		{
			return QueryBuilder.Create(@"select * from studentinschrijving");
		}
	}


	public sealed class StudentInschrijvingMetaObject : IMetaObject
	{
		public int Inschrijvingid { get; set; }
		public int Student { get; set; }
		public int Opleiding { get; set; }
		public int Fase { get; set; }
		public int Soort { get; set; }
		public int Periodestudiejaar { get; set; }
		public DateTime Datumvan { get; set; }
		public DateTime Datumtot { get; set; }
		public int Bekostiging { get; set; }
		public bool? Nietherinschrijven { get; set; }
		public int? Aanmeldingstatus { get; set; }
		public DateTime? Aanmeldingdatum { get; set; }
		public int? Aanmeldingvooropleiding { get; set; }
		public int? Aanmeldingdeficientiestatus { get; set; }
		public bool? Aanmeldingeerstejaar { get; set; }
		public bool? Aanmeldingverzoekintrekken { get; set; }
		public int? Aanmeldingibgstatus { get; set; }
		public int? Aanmeldingibgredenafkeur { get; set; }
		public int? Beeindigingreden { get; set; }
		public DateTime? Beeindigingdatumingediend { get; set; }
		public DateTime? Beeindigingdatum { get; set; }
		public string Beeindigingopmerkingstudent { get; set; }
		public string Beeindigingopmerkingadministratie { get; set; }
		public bool? Beeindigingverzoekrestitutie { get; set; }
		public bool? Beeindigingaccoord { get; set; }
		public DateTime? Examendatum { get; set; }
		public int? Examenjudicium { get; set; }
		public int? Examencode { get; set; }
		public DateTime? Examenaanvraagdatum { get; set; }
		public DateTime? Examenjudiciumdatum { get; set; }
		public bool? Flex { get; set; }
		public bool? Kop { get; set; }
		public bool? Offline { get; set; }
		public int? Bekostigingtoegekend { get; set; }
		public int? Bekostigingexamentoegekend { get; set; }
		public DateTime? Datumdefinitief { get; set; }
		public DateTime? Beeindigingdatumgewenst { get; set; }
		public decimal? Collegegeld { get; set; }
		public bool? Collegegeldhandmatig { get; set; }
		public bool? Bijvak { get; set; }
		public string Opmerking { get; set; }
		public DateTime? Voorlopigetoelatingtot { get; set; }
		public DateTime? Voorlopigetoelatingvan { get; set; }
		public bool? Bsaselect { get; set; }
		public string Aanmeldingstudielinkstartoccasion { get; set; }
		public int? Duoindicatiecollegegeld { get; set; }
		public string Diplomanummer { get; set; }
		public int? Bsacluster { get; set; }
	}



	public struct StudentVal : ILoadFromDbByConstructor
	{
		public readonly int Studentid;
		public readonly int Organisatie;
		public readonly int Studentnummer;
		public readonly int? Account;
		public readonly string Naam;
		public readonly string Naampartner;
		public readonly string Voorvoegsels;
		public readonly string Voorletters;
		public readonly string Voornamen;
		public readonly string Roepnaam;
		public readonly int? Geslacht;
		public readonly DateTime? Geboortedatum;
		public readonly string Geboorteplaats;
		public readonly int? Geboorteland;
		public readonly string Oudervoogd;
		public readonly string Emailprive;
		public readonly string Emailinstelling;
		public readonly string Persoonlijknummer;
		public readonly string Externecode;
		public readonly string Ocenwnummer;
		public readonly string Bsn;
		public readonly string Onderwijsnummer;
		public readonly string Studielinknummer;
		public readonly int? Telefoonnummermobielland;
		public readonly string Telefoonnummermobiel;
		public readonly int? Relatievorm;
		public readonly int? Taalstudielink;
		public readonly int? Toestemmingsverklaring;
		public readonly bool? Geheimhoudinggba;
		public readonly bool? Geboortedatumonbekend;
		public readonly int? Nationaliteit;
		public readonly int? Nationaliteit2;
		public readonly int? Prwinnummer;
		public StudentVal(int studentid, int organisatie, int studentnummer, int? account, string naam, string naampartner, string voorvoegsels, string voorletters, string voornamen, string roepnaam, int? geslacht, DateTime? geboortedatum, string geboorteplaats, int? geboorteland, string oudervoogd, string emailprive, string emailinstelling, string persoonlijknummer, string externecode, string ocenwnummer, string bsn, string onderwijsnummer, string studielinknummer, int? telefoonnummermobielland, string telefoonnummermobiel, int? relatievorm, int? taalstudielink, int? toestemmingsverklaring, bool? geheimhoudinggba, bool? geboortedatumonbekend, int? nationaliteit, int? nationaliteit2, int? prwinnummer)
		{
			Studentid = studentid;
			Organisatie = organisatie;
			Studentnummer = studentnummer;
			Account = account;
			Naam = naam;
			Naampartner = naampartner;
			Voorvoegsels = voorvoegsels;
			Voorletters = voorletters;
			Voornamen = voornamen;
			Roepnaam = roepnaam;
			Geslacht = geslacht;
			Geboortedatum = geboortedatum;
			Geboorteplaats = geboorteplaats;
			Geboorteland = geboorteland;
			Oudervoogd = oudervoogd;
			Emailprive = emailprive;
			Emailinstelling = emailinstelling;
			Persoonlijknummer = persoonlijknummer;
			Externecode = externecode;
			Ocenwnummer = ocenwnummer;
			Bsn = bsn;
			Onderwijsnummer = onderwijsnummer;
			Studielinknummer = studielinknummer;
			Telefoonnummermobielland = telefoonnummermobielland;
			Telefoonnummermobiel = telefoonnummermobiel;
			Relatievorm = relatievorm;
			Taalstudielink = taalstudielink;
			Toestemmingsverklaring = toestemmingsverklaring;
			Geheimhoudinggba = geheimhoudinggba;
			Geboortedatumonbekend = geboortedatumonbekend;
			Nationaliteit = nationaliteit;
			Nationaliteit2 = nationaliteit2;
			Prwinnummer = prwinnummer;
		}

		public static QueryBuilder DbQuery()
		{
			return QueryBuilder.Create(@"select * from student");
		}
	}

	public sealed class StudentMetaObject : IMetaObject
	{
		public int Studentid { get; set; }
		public int Organisatie { get; set; }
		public int Studentnummer { get; set; }
		public int? Account { get; set; }
		public string Naam { get; set; }
		public string Naampartner { get; set; }
		public string Voorvoegsels { get; set; }
		public string Voorletters { get; set; }
		public string Voornamen { get; set; }
		public string Roepnaam { get; set; }
		public int? Geslacht { get; set; }
		public DateTime? Geboortedatum { get; set; }
		public string Geboorteplaats { get; set; }
		public int? Geboorteland { get; set; }
		public string Oudervoogd { get; set; }
		public string Emailprive { get; set; }
		public string Emailinstelling { get; set; }
		public string Persoonlijknummer { get; set; }
		public string Externecode { get; set; }
		public string Ocenwnummer { get; set; }
		public string Bsn { get; set; }
		public string Onderwijsnummer { get; set; }
		public string Studielinknummer { get; set; }
		public int? Telefoonnummermobielland { get; set; }
		public string Telefoonnummermobiel { get; set; }
		public int? Relatievorm { get; set; }
		public int? Taalstudielink { get; set; }
		public int? Toestemmingsverklaring { get; set; }
		public bool? Geheimhoudinggba { get; set; }
		public bool? Geboortedatumonbekend { get; set; }
		public int? Nationaliteit { get; set; }
		public int? Nationaliteit2 { get; set; }
		public int? Prwinnummer { get; set; }
	}
#endif
}
