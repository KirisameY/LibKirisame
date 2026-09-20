using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews;
using KirisameY.NotifiableCollections.Collections.WrappedViews.VanillaNotifyWrappers;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     把任意 <see cref="IReadOnlyNotifiableList{T}"/> 包装成只读视图的扩展方法：
///     库自身的通知模型为 <c>AsReadOnlyNotifiableList</c>，
///     桥接到标准库通知模型的观测版本为 <c>AsReadOnlyObservableCollection</c>。
///     <br/>
///     Extension methods that wrap an arbitrary <see cref="IReadOnlyNotifiableList{T}"/> into a read-only view:
///     the library's own notification model is <c>AsReadOnlyNotifiableList</c>, and the observable variant
///     bridging to the standard one is <c>AsReadOnlyObservableCollection</c>.
/// </summary>
public static class NotifiableListExtensions
{
    extension<T>(IReadOnlyNotifiableList<T> list)
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableList{T}"/> 包装为只读视图
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableList{T}"/> into a read-only view.
        /// </summary>
        /// <returns>
        ///     与源列表同步的只读视图。
        ///     <br/>
        ///     A read-only view kept in sync with source list.
        /// </returns>
        [PublicAPI]
        public IReadOnlyNotifiableList<T> AsReadOnlyNotifiableList() => new NotifiableListReadonlyView<T>(list);

        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableList{T}"/> 包装为 <see cref="IReadOnlyObservableList{T}"/>，
        ///     即以标准库通知模型对外报告变更的只读视图。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableList{T}"/> into an <see cref="IReadOnlyObservableList{T}"/>:
        ///     a read-only view that reports changes through the standard notification model.
        /// </summary>
        /// <param name="notifyThreshold">
        ///     <para>
        ///         决定一次变更应分多次发出单项通知，还是一次性发出 Reset 通知。默认为 <c>-1</c>，
        ///         即不设上限、始终逐项发出通知：
        ///         <list type="bullet">
        ///             <item>
        ///                 <b>负值</b>：不设上限，始终逐项发出通知
        ///             </item>
        ///             <item>
        ///                 <b>0</b>：始终发出单次 <see cref="NotifyCollectionChangedAction.Reset"/>
        ///             </item>
        ///             <item>
        ///                 <b>正值</b>：发生变化的项数超过该值时，发出单次
        ///                 <see cref="NotifyCollectionChangedAction.Reset"/>；否则逐项发出
        ///             </item>
        ///         </list>
        ///         本参数只影响 <see cref="INotifyCollectionChanged.CollectionChanged"/>，对
        ///         <see cref="INotifyPropertyChanged.PropertyChanged"/> 无影响；排序本身就不受其影响，见备注。
        ///     </para>
        ///     <para>
        ///         Decides whether a single change is reported as several per-item notifications or as one Reset.
        ///         Defaults to <c>-1</c>, i.e. no limit — notifications are always raised per item:
        ///         <list type="bullet">
        ///             <item>
        ///                 <b>Negative</b>: no limit; notifications are always raised per item
        ///             </item>
        ///             <item>
        ///                 <b>0</b>: a single <see cref="NotifyCollectionChangedAction.Reset"/> is always raised
        ///             </item>
        ///             <item>
        ///                 <b>Positive</b>: a single <see cref="NotifyCollectionChangedAction.Reset"/> is raised once
        ///                 the number of changed items exceeds the value; otherwise notifications are raised per item
        ///             </item>
        ///         </list>
        ///         This parameter only affects <see cref="INotifyCollectionChanged.CollectionChanged"/>;
        ///         <see cref="INotifyPropertyChanged.PropertyChanged"/> is unaffected, and sorting is never affected —
        ///         see the remarks.
        ///     </para>
        /// </param>
        /// <returns>
        ///     与源列表同步的只读可观测视图。
        ///     <br/>
        ///     A read-only observable view kept in sync with source list.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         与库自身的只读通知视图相比，二者的读取行为完全一致，区别只在通知的对外形式：
        ///         本方法的通知为 <see cref="INotifyCollectionChanged.CollectionChanged"/> 与
        ///         <see cref="INotifyPropertyChanged.PropertyChanged"/>，适合 WPF / Avalonia / MAUI 等按标准模型订阅的场合；
        ///         若需要携带项与索引等富信息的强类型事件参数，请改用库自身的通知视图。
        ///     </para>
        ///     <para>
        ///         通知的翻译规则为：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="INotifyCollectionChanged.CollectionChanged"/>：元素加入、移除、替换时，逐项发出对应的
        ///                 <see cref="NotifyCollectionChangedAction.Add"/>、
        ///                 <see cref="NotifyCollectionChangedAction.Remove"/> 与
        ///                 <see cref="NotifyCollectionChangedAction.Replace"/>，并携带发生变化的索引
        ///             </item>
        ///             <item>
        ///                 排序无法逐项表达，故其 <see cref="INotifyCollectionChanged.CollectionChanged"/> 一律为单次
        ///                 <see cref="NotifyCollectionChangedAction.Reset"/>，不受 <paramref name="notifyThreshold"/> 影响
        ///             </item>
        ///             <item>
        ///                 <see cref="INotifyPropertyChanged.PropertyChanged"/>：元素加入或移除时发出
        ///                 <see cref="IReadOnlyCollection{T}.Count"/> 与索引器（属性名 <c>Item[]</c>）；
        ///                 元素替换或排序则只发出索引器
        ///             </item>
        ///         </list>
        ///         <paramref name="notifyThreshold"/> 只作用于加入、移除、替换：单次变更的项数超过阈值时，
        ///         上述逐项通知会被替换为单次 <see cref="NotifyCollectionChangedAction.Reset"/>。
        ///     </para>
        ///     <para>
        ///         The read behaviour is identical to that of the library's own notifying read-only view; only the
        ///         shape of the notifications differs. This overload reports through
        ///         <see cref="INotifyCollectionChanged.CollectionChanged"/> and
        ///         <see cref="INotifyPropertyChanged.PropertyChanged"/>, which suits consumers subscribing through the
        ///         standard model (WPF / Avalonia / MAUI and the like). Use the library's own notifying view instead
        ///         when the richer, strongly typed event arguments are needed.
        ///     </para>
        ///     <para>
        ///         The notification mapping is as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="INotifyCollectionChanged.CollectionChanged"/>: additions, removals and
        ///                 replacements raise the matching <see cref="NotifyCollectionChangedAction.Add"/>,
        ///                 <see cref="NotifyCollectionChangedAction.Remove"/> and
        ///                 <see cref="NotifyCollectionChangedAction.Replace"/>, one per item, carrying the affected
        ///                 index
        ///             </item>
        ///             <item>
        ///                 sorting cannot be expressed item by item, so its
        ///                 <see cref="INotifyCollectionChanged.CollectionChanged"/> is always a single
        ///                 <see cref="NotifyCollectionChangedAction.Reset"/>, unaffected by
        ///                 <paramref name="notifyThreshold"/>
        ///             </item>
        ///             <item>
        ///                 <see cref="INotifyPropertyChanged.PropertyChanged"/>: additions and removals raise
        ///                 <see cref="IReadOnlyCollection{T}.Count"/> and the indexer (reported under the name
        ///                 <c>Item[]</c>); replacements and sorting raise the indexer only
        ///             </item>
        ///         </list>
        ///         <paramref name="notifyThreshold"/> applies to additions, removals and replacements only: once the
        ///         number of changed items exceeds the threshold, those per-item notifications are replaced by a
        ///         single <see cref="NotifyCollectionChangedAction.Reset"/>.
        ///     </para>
        /// </remarks>
        [PublicAPI]
        public IReadOnlyObservableList<T> AsReadOnlyObservableCollection(int notifyThreshold = -1) =>
            new ObservableListWrapper<T>(list, notifyThreshold);
    }

    extension<TSource, TValue>(IReadOnlyNotifiableList<TSource> list)
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableList{T}"/> 包装为只读视图，并通过传入的委托把元素投影成 <typeparamref name="TValue"/>。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableList{T}"/> into a read-only view, projecting its
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
        ///         <see cref="IListUpdateNotifier{T}.ListUpdated"/> 通知时，其中携带的元素会经过 <paramref name="valueSelector"/> 映射，
        ///         故该委托需承担保证事件中传递的元素与从列表中读到/被其他事件发送的元素一致性的责任。<br/>
        ///         具体来说，进行类型转换或属性读取等能保证同一输入多次调用返回值皆相同的操作是安全的；但创建新实例的操作则需要进行缓存（如使用
        ///         <see cref="ConditionalWeakTable{TKey,TValue}"/> 进行弱引用缓存）才能保证安全性。<br/>
        ///         另外，通知中携带的起始索引与索引列表是位置信息、与元素类型无关，会原样透传，
        ///         因此投影前后指向的仍是同一批位置。
        ///     </para>
        ///     <para>
        ///         When <see cref="IListUpdateNotifier{T}.ListUpdated"/> fires, the elements it carries are mapped
        ///         through <paramref name="valueSelector"/>. The delegate is therefore responsible for keeping the
        ///         elements delivered by events consistent with those read from the list or sent by other events.<br/>
        ///         Concretely, operations that guarantee the same result for the same input across calls — such as casts
        ///         or property reads — are safe; operations that create new instances are only safe if they are cached
        ///         (for example, a weak cache backed by <see cref="ConditionalWeakTable{TKey,TValue}"/>).<br/>
        ///         Note also that the start index and the index list carried by the notifications describe positions and
        ///         are independent of the element type, so they are passed through unchanged and keep referring to the
        ///         same positions before and after projection.
        ///     </para>
        /// </remarks>
        [PublicAPI]
        public IReadOnlyNotifiableList<TValue> AsReadOnlyNotifiableList(Func<TSource, TValue> valueSelector) =>
            new NotifiableListReadonlyView<TSource, TValue>(list, valueSelector);
    }

    extension<TSource, TValue>(IReadOnlyNotifiableList<TSource> list) where TSource : TValue
    {
        /// <summary>
        ///     将给定的任意 <see cref="IReadOnlyNotifiableList{T}"/> 包装为只读视图，将元素向上转型为 <typeparamref name="TValue"/>。
        ///     <br/>
        ///     Wraps the given <see cref="IReadOnlyNotifiableList{T}"/> into a read-only view, upcasting its
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
        public IReadOnlyNotifiableList<TValue> AsReadOnlyNotifiableList(TypeA<TValue> type = default) =>
            new NotifiableListReadonlyView<TSource, TValue>(list, static v => v);
    }
}