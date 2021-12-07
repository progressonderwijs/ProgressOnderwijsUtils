using System;
using ExpressionToCodeLib;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils;

public interface IPropertyMapper
{
    Func<TRow, TRow> CreateRowMapper<TRow>()
        where TRow : IWrittenImplicitly;

    public (Func<TId, TId> mapper, bool idNonIdentityMap) CreateIdMapper<TId>();
    Type MappedPropertyType();
}

public static class PropertyMapper
{
    public static PropertyMappers CreateForValue<TProperty>(TProperty value)
        where TProperty : struct, Enum
        => CreateForFunc<TProperty>(_ => value);

    [CodeThatsOnlyUsedForTests]
    public static PropertyMappers CreateForValue<TProperty>(TProperty? value)
        where TProperty : struct, Enum
        => CreateForFunc<TProperty>(_ => value);

    public static PropertyMappers CreateForAutoColumnMapping<TProperty>(params SingleIdMapping<TProperty>[] mappings)
        where TProperty : struct, Enum
        => CreateForDictionary(mappings.CodeToDatabaseIds());

    public static PropertyMappers CreateForDictionary<TProperty>(Dictionary<TProperty, TProperty> lookup)
        where TProperty : struct, Enum
        => CreateForFunc<TProperty>(
            property => {
                if (lookup.TryGetValue(property, out var mapped)) {
                    return mapped;
                } else {
                    throw new InvalidOperationException($"Cannot find key {property} : {typeof(TProperty).ToCSharpFriendlyTypeName()} in dictionary.");
                }
            }
        );

    public static PropertyMappers CreateForIdentityMap<TProperty>()
        where TProperty : struct, Enum
        => CreateForFunc(NoopLambda<TProperty>.Instance);

    public static PropertyMappers CreateForFunc<TProperty>(Func<TProperty, TProperty> func)
        where TProperty : struct, Enum
        => new(
            new PropertyMapper<TProperty>(func),
            new PropertyMapper<TProperty?>(property => property.HasValue ? func(property.Value) : default(TProperty?))
        );

    public static PropertyMappers CreateForFunc<TProperty>(Func<TProperty?, TProperty?> func)
        where TProperty : struct, Enum
        => new(new PropertyMapper<TProperty?>(func));
}

public sealed class PropertyMapper<TProperty> : IPropertyMapper
{
    readonly Func<TProperty, TProperty> mapper;

    internal PropertyMapper(Func<TProperty, TProperty> mapper)
        => this.mapper = mapper;

    public Type MappedPropertyType()
        => typeof(TProperty);

    public (Func<TId, TId> mapper, bool idNonIdentityMap) CreateIdMapper<TId>()
        => mapper is Func<TId, TId> reTyped ? (reTyped, true) : (NoopLambda<TId>.Instance, false);

    Func<T, T> IPropertyMapper.CreateRowMapper<T>()
        => Mappers<T>.Instance.Invoke(mapper);

    static class Mappers<T>
        where T : IWrittenImplicitly
    {
        public static readonly Func<Func<TProperty, TProperty>, Func<T, T>> Instance = CreateMapper();

        static Func<Func<TProperty, TProperty>, Func<T, T>> CreateMapper()
        {
            var relevantProperties = PocoUtils.GetProperties<T>()
                .Where(property => property.DataType == typeof(TProperty) && property.CanRead && property.CanWrite)
                .ToArray();
            if (relevantProperties.None()) {
                return _ => NoopLambda<T>.Instance;
            }

            var rowObjParam = Expression.Parameter(typeof(T));
            var doMap = Expression.Parameter(typeof(Func<TProperty, TProperty>));
            var setters = Expression.Block(
                relevantProperties.Select(
                    property =>
                        Expression.Assign(property.PropertyAccessExpression(rowObjParam), Expression.Invoke(doMap, property.PropertyAccessExpression(rowObjParam)))
                )
            );
            var inner = Expression.Lambda<Func<T, T>>(Expression.Block(setters, rowObjParam), rowObjParam);
            return Expression.Lambda<Func<Func<TProperty, TProperty>, Func<T, T>>>(inner, doMap).Compile();
        }
    }
}

static class NoopLambda<T>
{
    public static readonly Func<T, T> Instance = x => x;
}

public sealed class PropertyMappers
{
    readonly Dictionary<Type, IPropertyMapper> mapperByPropertyType;

    public PropertyMappers()
        : this(Array.Empty<IPropertyMapper>()) { }

    public PropertyMappers(params IPropertyMapper[] mappers)
        => mapperByPropertyType = mappers.ToDictionary(o => o.MappedPropertyType());

    [UsefulToKeep("Library function")]
    public Type[] MappedPropertyTypes()
        => mapperByPropertyType.Keys.ToArray();

    public PropertyMappers CloneWithExtraMappers(PropertyMappers extraMappers)
        => new(mapperByPropertyType.Values.Concat(extraMappers.mapperByPropertyType.Values).ToArray());

    public T[] Map<T>(T[] objects)
        where T : class, ICopyable<T>, IWrittenImplicitly, IEquatable<T>
    {
        var mappingFunction = MappingFunctionFor<T>();
        return objects.ArraySelect(mappingFunction);
    }

    public Func<T, T> MappingFunctionFor<T>()
        where T : class, ICopyable<T>, IWrittenImplicitly, IEquatable<T>
    {
        var funcs = mapperByPropertyType.Values
            .Select(mapper => mapper.CreateRowMapper<T>())
            .Where(mapper => mapper != NoopLambda<T>.Instance)
            .ToArray();
        return obj => {
            var copy = obj.Copy();
            foreach (var func1 in funcs) {
                copy = func1(copy);
            }
            return copy;
        };
    }

    public (Func<TId, TId> mapper, bool idNonIdentityMap) GetIdMapper<TId>()
        where TId : struct, Enum
        => mapperByPropertyType.TryGetValue(typeof(TId), out var pMapper) ? pMapper.CreateIdMapper<TId>() : (NoopLambda<TId>.Instance, false);

    public TId MapId<TId>(TId id)
        where TId : struct, Enum
        => mapperByPropertyType.TryGetValue(typeof(TId), out var pMapper) ? pMapper.CreateIdMapper<TId>().mapper(id) : id;
}

public static class PropertyMappersExtensions
{
    public static SingleIdMapping<TId>[] AddToPropertyMappers<TId>(this SingleIdMapping<TId>[] mappings, ref PropertyMappers mappers)
        where TId : struct, Enum
    {
        mappers = mappers.CloneWithExtraMappers(PropertyMapper.CreateForAutoColumnMapping(mappings));
        return mappings;
    }
}