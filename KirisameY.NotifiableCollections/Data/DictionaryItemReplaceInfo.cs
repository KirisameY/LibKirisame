using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Data;

[PublicAPI]
public interface IDictionaryItemReplaceInfo<TKey, TValue> : IItemReplaceInfo<KeyValuePair<TKey, TValue>>
{
    [PublicAPI] public TKey Key { get; }
    [PublicAPI] public TValue OldValue { get; }
    [PublicAPI] public TValue NewValue { get; }
}

public static class DictionaryItemReplaceInfo
{
    [PublicAPI]
    public static IDictionaryItemReplaceInfo<TKey, TValue> From<TKey, TValue>(TKey key, TValue old, TValue @new) =>
        new DictionaryItemReplaceInfo<TKey, TValue>(key, old, @new);
}

public class DictionaryItemReplaceInfo<TKey, TValue>(TKey key, TValue old, TValue @new)
    : ItemReplaceInfo<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, old), new KeyValuePair<TKey, TValue>(key, @new)),
      IDictionaryItemReplaceInfo<TKey, TValue>
{
    public TKey Key { get; } = key;
    public TValue OldValue { get; } = old;
    public TValue NewValue { get; } = @new;
}
