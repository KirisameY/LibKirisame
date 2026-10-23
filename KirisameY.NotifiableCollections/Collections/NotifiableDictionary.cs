using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="INotifiableDictionary{TKey,TValue}"/> 的默认实现。
///     <br/>
///     Default implement of <see cref="INotifiableDictionary{TKey,TValue}"/>.
/// </summary>
/// <typeparam name="TKey">
///     键的类型。
///     <br/>
///     The type of the keys.
/// </typeparam>
/// <typeparam name="TValue">
///     值的类型。
///     <br/>
///     The type of the values.
/// </typeparam>
public class NotifiableDictionary<TKey, TValue> : NotifiableDictionarySimpleBase<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _innerDict = [];

    /// <inheritdoc />
    protected override IReadOnlyDictionary<TKey, TValue> View => field ??= _innerDict.AsReadOnly();

    /// <inheritdoc />
    protected override void SetValue(TKey key, TValue value) => _innerDict[key] = value;

    /// <inheritdoc />
    protected override bool TryAddValue(TKey key, TValue value) => _innerDict.TryAdd(key, value);

    /// <inheritdoc />
    protected override void AddValue(TKey key, TValue value) => _innerDict.Add(key, value);

    /// <inheritdoc />
    protected override bool RemoveValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _innerDict.Remove(key, out value);

    /// <inheritdoc />
    protected override bool RemoveValue(TKey key) => _innerDict.Remove(key);

    /// <inheritdoc />
    protected override void ClearValues() => _innerDict.Clear();
}