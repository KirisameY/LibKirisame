using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableDictionaryExtensions
{
    extension<TKey, TValue>(IReadOnlyNotifiableDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableDictionary{TKey, TValue}"/> 包装为只读视图
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableDictionary{TKey, TValue}"/> into a read-only view.
        /// </summary>
        /// <returns>
        ///     与 <paramref name="dictionary"/> 同步的只读视图。
        ///     <br/>
        ///     A read-only view kept in sync with <paramref name="dictionary"/>.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnlyNotifiableDictionary() =>
            new NotifiableDictionaryReadonlyView<TKey, TValue>(dictionary);

        [PublicAPI]
        public IReadOnlyObservableDictionary<TKey, TValue> AsReadOnlyObservableDictionary() => new ObservableDictionaryWrapper<TKey, TValue>(dictionary);
    }

    extension<TKey, TSourceValue, TValue>(IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary)
        where TKey : notnull
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableDictionary{TKey, TValue}"/> 包装为只读视图，并通过传入的委托把值投影成 <typeparamref name="TValue"/>。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableDictionary{TKey, TValue}"/> into a read-only view,
        ///     projecting its values into <typeparamref name="TValue"/> through the supplied delegate.
        /// </summary>
        /// <param name="valueSelector">
        ///     值的投影函数。每次读取时才调用，不会被缓存。
        ///     <br/>
        ///     The value projection function. It is invoked on each read and is not cached.
        /// </param>
        /// <returns>
        ///     键不变、值类型为 <typeparamref name="TValue"/> 的同步只读视图。
        ///     <br/>
        ///     A read-only view with unchanged keys and <typeparamref name="TValue"/> values, kept in sync with the source.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <see cref="IDictionaryUpdateNotifier{TKey, TValue}.DictionaryUpdated"/> 通知时，其中携带的值会经过 <paramref name="valueSelector"/> 映射，
        ///         故该委托需承担保证事件中传递的值与从字典中读到/被其他事件发送的值一致性的责任。<br/>
        ///         具体来说，进行类型转换或属性读取等能保证同一输入多次调用返回值皆相同的操作是安全的；但创建新实例的操作则需要进行缓存（如使用
        ///         <see cref="ConditionalWeakTable{TKey,TValue}"/> 进行弱引用缓存）才能保证安全性。<br/>
        ///         另外，键不参与投影，因此通知中携带的键始终与底层字典一致。
        ///     </para>
        ///     <para>
        ///         When <see cref="IDictionaryUpdateNotifier{TKey, TValue}.DictionaryUpdated"/> fires, the values it
        ///         carries are mapped through <paramref name="valueSelector"/>. The delegate is therefore responsible for
        ///         keeping the values delivered by events consistent with those read from the dictionary or sent by other
        ///         events.<br/>
        ///         Concretely, operations that guarantee the same result for the same input across calls — such as casts
        ///         or property reads — are safe; operations that create new instances are only safe if they are cached
        ///         (for example, a weak cache backed by <see cref="ConditionalWeakTable{TKey,TValue}"/>).<br/>
        ///         Note also that keys are never projected, so the keys carried by the notifications always match those
        ///         of the underlying dictionary.
        ///     </para>
        /// </remarks>
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnlyNotifiableDictionary(Func<TSourceValue, TValue> valueSelector) =>
            new NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(dictionary, valueSelector);
    }

    extension<TKey, TSourceValue, TValue>(IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary)
        where TKey : notnull
        where TSourceValue : TValue
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableDictionary{TKey, TValue}"/> 包装为只读视图，将值向上转型为 <typeparamref name="TValue"/>。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableDictionary{TKey, TValue}"/> into a read-only view,
        ///     upcasting its values to <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="type">
        ///     用于声明 <typeparamref name="TValue"/> 类型的trick，填写干参数用于避免显式完整填写类型参数，填写了类型参数的情况下可省略。
        ///     <br/>
        ///     A trick for declaring the <typeparamref name="TValue"/> type: the dummy argument lets callers avoid
        ///     spelling out the whole type argument list, and it can be omitted when no type argument is given.
        /// </param>
        /// <returns>
        ///     键不变、值类型为 <typeparamref name="TValue"/> 的只读视图。
        ///     <br/>
        ///     A read-only view with unchanged keys and <typeparamref name="TValue"/> values.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnlyNotifiableDictionary(TypeA<TValue> type = default) =>
            new NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(dictionary, static s => s);
    }
}