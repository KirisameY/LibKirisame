using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableCollectionExtensions
{
    extension<T>(IReadOnlyNotifiableCollection<T> source)
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableCollection{T}"/> 包装为只读视图
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableCollection{T}"/> into a read-only view.
        /// </summary>
        /// <returns>
        ///     与 <paramref name="source"/> 同步的只读视图。
        ///     <br/>
        ///     A read-only view kept in sync with <paramref name="source"/>.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableCollection<T> AsReadOnlyNotifiableCollection() => new NotifiableCollectionReadonlyView<T>(source);

        [PublicAPI]
        public IReadOnlyObservableCollection<T> AsReadOnlyObservableCollection() => new ObservableCollectionWrapper<T>(source);
    }

    extension<TSource, TValue>(IReadOnlyNotifiableCollection<TSource> source)
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableCollection{T}"/> 包装为只读视图，并通过传入的委托把元素投影成 <typeparamref name="TValue"/>。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableCollection{T}"/> into a read-only view, projecting its
        ///     elements into <typeparamref name="TValue"/> through the supplied delegate.
        /// </summary>
        /// <param name="valueSelector">
        ///     元素投影函数。每次读取时才调用，不会被缓存。
        ///     <br/>
        ///     The element projection function. It is invoked on each read and is not cached.
        /// </param>
        /// <returns>
        ///     元素类型为 <typeparamref name="TValue"/> 的同步只读视图。
        ///     <br/>
        ///     A read-only view of <typeparamref name="TValue"/> elements, kept in sync with the source.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <see cref="ICollectionUpdateNotifier{T}.CollectionUpdated"/> 通知时，其中携带的元素会经过 <paramref name="valueSelector"/> 映射，
        ///         故该委托需承担保证事件中传递的元素与从集合中读到/被其他事件发送的元素一致性的责任。<br/>
        ///         具体来说，进行类型转换或属性读取等能保证同一输入多次调用返回值皆相同的操作是安全的；但创建新实例的操作则需要进行缓存（如使用
        ///         <see cref="ConditionalWeakTable{TKey,TValue}"/> 进行弱引用缓存）才能保证安全性。
        ///     </para>
        ///     <para>
        ///         When <see cref="ICollectionUpdateNotifier{T}.CollectionUpdated"/> fires, the elements it carries are
        ///         mapped through <paramref name="valueSelector"/>. The delegate is therefore responsible for keeping
        ///         the elements delivered by events consistent with those read from the collection or sent by other
        ///         events.<br/>
        ///         Concretely, operations that guarantee the same result for the same input across calls — such as casts
        ///         or property reads — are safe; operations that create new instances are only safe if they are cached
        ///         (for example, a weak cache backed by <see cref="ConditionalWeakTable{TKey,TValue}"/>).
        ///     </para>
        /// </remarks>
        [PublicAPI]
        public IReadOnlyNotifiableCollection<TValue> AsReadOnlyNotifiableCollection(Func<TSource, TValue> valueSelector) =>
            new NotifiableCollectionReadonlyView<TSource, TValue>(source, valueSelector);
    }

    extension<TSource, TValue>(IReadOnlyNotifiableCollection<TSource> source) where TSource : TValue
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableCollection{T}"/> 包装为只读视图，将元素向上转型为 <typeparamref name="TValue"/>。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableCollection{T}"/> into a read-only view, upcasting its
        ///     elements to <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="type">
        ///     用于声明 <typeparamref name="TValue"/> 类型的trick，填写干参数用于避免显式完整填写类型参数，填写了类型参数的情况下可省略。
        ///     <br/>
        ///     A trick for declaring the <typeparamref name="TValue"/> type: the dummy argument lets callers avoid
        ///     spelling out the whole type argument list, and it can be omitted when no type argument is given.
        /// </param>
        /// <returns>
        ///     元素类型为 <typeparamref name="TValue"/> 的只读视图。
        ///     <br/>
        ///     A read-only view of <typeparamref name="TValue"/> elements.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableCollection<TValue> AsReadOnlyNotifiableCollection(TypeA<TValue> type = default) =>
            new NotifiableCollectionReadonlyView<TSource, TValue>(source, static v => v);
    }
}