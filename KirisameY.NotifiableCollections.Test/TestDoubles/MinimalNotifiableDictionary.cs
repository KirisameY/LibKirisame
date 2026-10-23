using System.Diagnostics.CodeAnalysis;

using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.TestDoubles;

/// <summary>
///     只实现 <see cref="NotifiableDictionarySimpleBase{TKey,TValue}"/> 的三个抽象成员，
///     其余操作（含 <c>TryAdd</c> / <c>AddRange</c> / <c>PutRange</c> / <c>Clear</c> 等）全部交给基类的默认实现。
///     <br/>
///     A dictionary that implements only the three abstract members of
///     <see cref="NotifiableDictionarySimpleBase{TKey,TValue}"/>, leaving every other operation to the base defaults.
/// </summary>
/// <remarks>
///     <see cref="NotifiableDictionary{TKey,TValue}"/> 重写了所有 protected virtual 钩子，所以这些钩子的默认实现
///     只能通过这样一个最小子类来单独验证。
/// </remarks>
public class MinimalNotifiableDictionary<TKey, TValue> : NotifiableDictionarySimpleBase<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _inner = [];

    protected override IReadOnlyDictionary<TKey, TValue> View => _inner;

    protected override void SetValue(TKey key, TValue value) => _inner[key] = value;

    protected override bool RemoveValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        _inner.Remove(key, out value);
}
