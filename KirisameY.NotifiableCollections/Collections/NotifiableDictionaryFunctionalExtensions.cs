namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="INotifiableDictionary{TKey,TValue}"/> 的功能性拓展方法：
///     <br/>
///     Functional extension methods for <see cref="INotifiableDictionary{TKey,TValue}"/>.
/// </summary>
public static class NotifiableDictionaryFunctionalExtensions
{
    extension<TKey, TValue>(INotifiableDictionary<TKey, TValue> dict)
    {
        /// <summary>
        ///     尝试添加一个键值对，结果与 <see cref="INotifiableDictionary{TKey,TValue}.TryAdd"/> 完全相同。
        ///     <br/>
        ///     Tries to add a key-value pair; identical in effect to <see cref="INotifiableDictionary{TKey,TValue}.TryAdd"/>.
        /// </summary>
        /// <param name="item">
        ///     要添加的键值对。
        ///     <br/>
        ///     The key-value pair to add.
        /// </param>
        /// <returns>
        ///     添加成功返回 <see langword="true"/>；键已存在则返回 <see langword="false"/>。
        ///     <br/>
        ///     <see langword="true"/> when the pair was added; <see langword="false"/> when the key already exists.
        /// </returns>
        public bool TryAdd(KeyValuePair<TKey, TValue> item) => dict.TryAdd(item.Key, item.Value);


        /// <inheritdoc cref="INotifiableDictionary{TKey,TValue}.Remove(TKey, out TValue)"/>
        public bool Remove(TKey key) => dict.Remove(key, out _);
    }
}