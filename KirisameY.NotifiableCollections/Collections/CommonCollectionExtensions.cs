using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     用于从 <see cref="IEnumerable{T}"/> 中创建 Notifiable 集合的拓展方法。
///     <br/>
///     Extensions that creates notifiable collections from an <see cref="IEnumerable{T}"/>
/// </summary>
public static class CommonCollectionExtensions
{
    extension<T>([InstantHandle] IEnumerable<T> source)
    {
        /// <summary>
        ///     从 <see cref="IEnumerable{T}"/> 创建一个 <see cref="NotifiableList{T}"/>
        ///     <br/>
        ///     Creates a <see cref="NotifiableList{T}"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <seealso cref="Enumerable.ToList"/>
        public NotifiableList<T> ToNotifiableList()
        {
            var result = new NotifiableList<T>();
            result.AddRange(source);
            return result;
        }

        /// <summary>
        ///     从 <see cref="IEnumerable{T}"/> 创建一个 <see cref="NotifiableDictionary{TKey, TValue}"/>
        ///     <br/>
        ///     Creates a <see cref="NotifiableDictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <seealso cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource,TKey}, Func{TSource,TElement})"/>
        public NotifiableDictionary<TKey, TValue> ToNotifiableDictionary<TKey, TValue>(
            Func<T, TKey> keySelector, Func<T, TValue> valueSelector
        ) where TKey : notnull
        {
            var result = new NotifiableDictionary<TKey, TValue>();
            result.AddRange(source.Select(e => new KeyValuePair<TKey, TValue>(keySelector.Invoke(e), valueSelector.Invoke(e))));
            return result;
        }
    }

    extension<TKey, TValue>([InstantHandle] IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
    {
        /// <summary>
        ///     从 <see cref="IEnumerable{T}"/> 创建一个 <see cref="NotifiableDictionary{TKey, TValue}"/>
        ///     <br/>
        ///     Creates a <see cref="NotifiableDictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <seealso cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})"/>
        public NotifiableDictionary<TKey, TValue> ToNotifiableDictionary()
        {
            var result = new NotifiableDictionary<TKey, TValue>();
            result.AddRange(source);
            source.ToDictionary();
            return result;
        }
    }
}