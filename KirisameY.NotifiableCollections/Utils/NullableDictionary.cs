using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using KirisameY.GenericUtils;

namespace KirisameY.NotifiableCollections.Utils;

internal static class NullableFrozenDictionary
{
    public static NullableFrozenDictionary<TK, TV> Empty<TK, TV>() => new(ReadOnlyDictionary<Optional<TK>, TV>.Empty);

    public static NullableFrozenDictionary<TK, TV> From<TK, TV>(params IEnumerable<KeyValuePair<TK, TV>> pairs) => new(
        pairs.ToFrozenDictionary(pair => Optional.FromNullable(pair.Key), pair => pair.Value)
    );

    public static NullableFrozenDictionary<TK, TV> From<TK, TV>(params IEnumerable<(TK Key, TV Value)> pairs) => new(
        pairs.ToFrozenDictionary(t => Optional.FromNullable(t.Key), pair => pair.Value)
    );
}

internal class NullableFrozenDictionary<TK, TV>(IReadOnlyDictionary<Optional<TK>, TV> dict) : IReadOnlyDictionary<TK, TV>
{
    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() =>
        dict.Select(p => KeyValuePair.Create(p.Key.HasValue ? p.Key.Value : default!, p.Value))
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => dict.Count;

    public bool ContainsKey(TK key) => dict.ContainsKey(Optional.FromNullable(key));

    public bool TryGetValue(TK key, [MaybeNullWhen(false)] out TV value) => dict.TryGetValue(Optional.FromNullable(key), out value);

    public TV this[TK key] => dict[Optional.FromNullable(key)];
    public IEnumerable<TK> Keys => dict.Keys.Select(k => k.HasValue ? k.Value : default!);
    public IEnumerable<TV> Values => dict.Values;
}