using System.Collections.Specialized;
using System.ComponentModel;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     只读且可观测的集合视图，以标准库的 <see cref="INotifyCollectionChanged"/> 与 <see cref="INotifyPropertyChanged"/>
///     对外报告变更。
///     <br/>
///     A read-only, observable collection view that reports changes through the standard
///     <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/> events.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     <para>
///         本接口是库自身的 <see cref="IReadOnlyNotifiableCollection{T}"/> 与标准库通知模型之间的桥接。
///         视图本身是实时的只读投影，不提供任何修改成员，更新只可能来自被包装的集合；
///         而通知则被翻译成标准格式，供 WPF / Avalonia / MAUI 等按标准模型订阅的场合使用。
///         若需要携带项、索引等富信息的强类型事件参数，请改用 <see cref="IReadOnlyNotifiableCollection{T}"/>。
///     </para>
///     <para>
///         <see cref="NotifyCollectionChangedAction.Reset"/> 在变更无法逐项表达时发出；
///         此外，创建视图的方法还可以通过阈值控制逐项通知与 Reset 之间的取舍。
///         收到 Reset 时应当视为整个集合已失效，并重新读取其全部内容。
///     </para>
///     <para>
///         This interface bridges the library's own <see cref="IReadOnlyNotifiableCollection{T}"/> to the standard
///         notification model. The view is a live read-only projection with no mutating members, so updates can only
///         originate from the wrapped collection, while notifications are translated into the standard shape for
///         consumers that subscribe through that model (WPF / Avalonia / MAUI and the like). Use
///         <see cref="IReadOnlyNotifiableCollection{T}"/> instead when the richer, strongly typed event arguments
///         (carrying items, indices and so on) are needed.
///     </para>
///     <para>
///         <see cref="NotifyCollectionChangedAction.Reset"/> is raised when a change cannot be expressed item by
///         item; in addition, the methods that create a view accept a threshold that controls the trade-off between
///         per-item notifications and Reset. Treat a Reset as invalidating the whole collection and re-read its
///         contents.
///     </para>
/// </remarks>
public interface IReadOnlyObservableCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged;

/// <summary>
///     只读且可观测的列表视图，在 <see cref="IReadOnlyObservableCollection{T}"/> 的基础上额外提供按索引访问。
///     <br/>
///     A read-only, observable list view that adds indexed access on top of
///     <see cref="IReadOnlyObservableCollection{T}"/>.
/// </summary>
/// <typeparam name="T">
///     元素的类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     <para>
///         除 <see cref="IReadOnlyObservableCollection{T}"/> 所述的通知外，元素加入、移除或替换时，
///         本接口的视图还会通过 <see cref="INotifyPropertyChanged.PropertyChanged"/> 通知索引器（属性名
///         <c>Item[]</c>）发生变化，订阅方据此重新读取受影响索引处的值。
///     </para>
///     <para>
///         排序等无法逐项表达的变更会发出 <see cref="NotifyCollectionChangedAction.Reset"/>；
///         此外，创建视图的方法还可以通过阈值控制逐项通知与 Reset 之间的取舍。
///     </para>
///     <para>
///         Beyond the notifications described by <see cref="IReadOnlyObservableCollection{T}"/>, a view of this
///         interface also reports indexer changes through <see cref="INotifyPropertyChanged.PropertyChanged"/> when
///         items are added, removed or replaced, so subscribers can re-read the values at the affected indices. The
///         indexer is reported under the name <c>Item[]</c>.
///     </para>
///     <para>
///         Changes that cannot be expressed item by item — sorting, for instance — raise
///         <see cref="NotifyCollectionChangedAction.Reset"/>; in addition, the methods that create a view accept a
///         threshold that controls the trade-off between per-item notifications and Reset.
///     </para>
/// </remarks>
public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>;

/// <summary>
///     只读且可观测的字典视图，在 <see cref="IReadOnlyObservableCollection{T}"/> 的基础上额外提供按键访问，
///     并以 <see cref="KeyValuePair{TKey, TValue}"/> 为单位报告变更。
///     <br/>
///     A read-only, observable dictionary view that adds keyed access on top of
///     <see cref="IReadOnlyObservableCollection{T}"/> and reports changes in terms of
///     <see cref="KeyValuePair{TKey, TValue}"/>.
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
/// <remarks>
///     <para>
///         <see cref="Keys"/> 与 <see cref="Values"/> 自身也是可观测的视图，订阅它们即可只关心键或值的变更。
///         通过 <see cref="IReadOnlyDictionary{TKey, TValue}"/> 访问时得到的是同一个视图，
///         只是静态类型为 <see cref="IEnumerable{T}"/>、看不到通知成员；需要通知时请使用本接口上的属性。
///     </para>
///     <para>
///         <see cref="Keys"/> and <see cref="Values"/> are observable views themselves, so subscribing to them
///         narrows the interest to key-only or value-only changes. Accessing them through
///         <see cref="IReadOnlyDictionary{TKey, TValue}"/> yields the very same views, statically typed as
///         <see cref="IEnumerable{T}"/> and therefore without the notifying members; use the properties on this
///         interface when notifications are wanted.
///     </para>
/// </remarks>
public interface IReadOnlyObservableDictionary<TKey, TValue> : IReadOnlyObservableCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
{
    /// <summary>
    ///     键的只读可观测视图，与字典保持同步。
    ///     <br/>
    ///     A read-only observable view of the keys, kept in sync with the dictionary.
    /// </summary>
    new IReadOnlyObservableCollection<TKey> Keys { get; }

    /// <summary>
    ///     值的只读可观测视图，与字典保持同步。
    ///     <br/>
    ///     A read-only observable view of the values, kept in sync with the dictionary.
    /// </summary>
    new IReadOnlyObservableCollection<TValue> Values { get; }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
}