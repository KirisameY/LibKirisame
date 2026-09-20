using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Data;

/// <summary>
///     字典版的新旧值对照：以键为准，分别给出该键的新旧值。
///     <br/>
///     The dictionary-level old/new pair: it is keyed, exposing the old and new value of that key separately.
/// </summary>
/// <typeparam name="TKey">
///     键的类型。
///     <br/>
///     The type of the key.
/// </typeparam>
/// <typeparam name="TValue">
///     值的类型。
///     <br/>
///     The type of the values.
/// </typeparam>
/// <remarks>
///     继承来的 <see cref="IItemReplaceInfo{T}.Old"/> 与 <see cref="IItemReplaceInfo{T}.New"/> 是打包好的键值对，
///     键取同一个键、值分别为新旧值；想直接拿值请用下面三个属性。
///     <br/>
///     The inherited <see cref="IItemReplaceInfo{T}.Old"/> and <see cref="IItemReplaceInfo{T}.New"/> are packaged
///     key-value pairs sharing the same key, differing only in the value; use the three properties below when the
///     values are what is wanted.
/// </remarks>
[PublicAPI]
public interface IDictionaryItemReplaceInfo<TKey, TValue> : IItemReplaceInfo<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    ///     发生替换的键。
    ///     <br/>
    ///     The key whose value was replaced.
    /// </summary>
    [PublicAPI] public TKey Key { get; }

    /// <summary>
    ///     该键被替换掉的旧值。
    ///     <br/>
    ///     The old value that was replaced.
    /// </summary>
    [PublicAPI] public TValue OldValue { get; }

    /// <summary>
    ///     该键替换后的新值。
    ///     <br/>
    ///     The new value it was replaced with.
    /// </summary>
    [PublicAPI] public TValue NewValue { get; }
}

/// <summary>
///     构造 <see cref="IDictionaryItemReplaceInfo{TKey, TValue}"/> 的工厂。
///     <br/>
///     Factory for <see cref="IDictionaryItemReplaceInfo{TKey, TValue}"/>.
/// </summary>
public static class DictionaryItemReplaceInfo
{
    /// <summary>
    ///     按键配一对新旧值。
    ///     <br/>
    ///     Pairs an old value with a new one under the given key.
    /// </summary>
    [PublicAPI]
    public static IDictionaryItemReplaceInfo<TKey, TValue> From<TKey, TValue>(TKey key, TValue old, TValue @new) =>
        new DictionaryItemReplaceInfo<TKey, TValue>(key, old, @new);
}

/// <summary>
///     <see cref="IDictionaryItemReplaceInfo{TKey, TValue}"/> 的默认实现。
///     <br/>
///     The default implementation of <see cref="IDictionaryItemReplaceInfo{TKey, TValue}"/>.
/// </summary>
public class DictionaryItemReplaceInfo<TKey, TValue>(TKey key, TValue old, TValue @new)
    : ItemReplaceInfo<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, old), new KeyValuePair<TKey, TValue>(key, @new)),
      IDictionaryItemReplaceInfo<TKey, TValue>
{
    /// <inheritdoc/>
    public TKey Key { get; } = key;

    /// <inheritdoc/>
    public TValue OldValue { get; } = old;

    /// <inheritdoc/>
    public TValue NewValue { get; } = @new;
}
