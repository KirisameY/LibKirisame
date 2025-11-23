using Godot;
using Godot.Collections;

using KirisameLib.Extensions;

using GdArray = Godot.Collections.Array;
using GdDictionary = Godot.Collections.Dictionary;

namespace KirisameLib.Godot.Linq;

public static class ToGdCollections
{
    // Array
    public static GdArray ToGdCommonArray<[MustBeVariant] TSource>(this IEnumerable<TSource> source) =>
        new(source.Select(s => Variant.From(s)));

    public static Array<TSource> ToGdArray<[MustBeVariant] TSource>(this IEnumerable<TSource> source) => new(source);


    // Dictionary
    public static GdDictionary ToGdCommonDictionary<[MustBeVariant] TKey, [MustBeVariant] TValue>
        (this IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        var result = new GdDictionary();
        source.ForEach(pair => result.Add(Variant.From(pair.Key), Variant.From(pair.Value)));
        return result;
    }

    public static GdDictionary ToGdCommonDictionary<TSource>
        (this IEnumerable<TSource> source, Func<TSource, Variant> keySelector, Func<TSource, Variant> valueSelector)
    {
        var result = new GdDictionary();
        source.ForEach(s => result.Add(keySelector(s), valueSelector(s)));
        return result;
    }

    public static GdDictionary ToGdCommonDictionary<[MustBeVariant] TSource>
        (this IEnumerable<TSource> source, Func<TSource, Variant> keySelector)
    {
        var result = new GdDictionary();
        source.ForEach(s => result.Add(keySelector(s), Variant.From(s)));
        return result;
    }


    // Generic Dictionary
    public static global::Godot.Collections.Dictionary<TKey, TValue> ToGdDictionary<[MustBeVariant] TKey, [MustBeVariant] TValue>
        (this IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        var result = new global::Godot.Collections.Dictionary<TKey, TValue>();
        source.ForEach(pair => result.Add(pair.Key, pair.Value));
        return result;
    }

    public static global::Godot.Collections.Dictionary<TKey, TValue> ToGdDictionary<TSource, [MustBeVariant] TKey, [MustBeVariant] TValue>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
    {
        var result = new global::Godot.Collections.Dictionary<TKey, TValue>();
        source.ForEach(s => result.Add(keySelector(s), valueSelector(s)));
        return result;
    }

    public static global::Godot.Collections.Dictionary<TKey, TSource> ToGdDictionary<[MustBeVariant] TSource, [MustBeVariant] TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var result = new global::Godot.Collections.Dictionary<TKey, TSource>();
        source.ForEach(s => result.Add(keySelector(s), s));
        return result;
    }
}