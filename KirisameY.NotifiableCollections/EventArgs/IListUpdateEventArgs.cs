using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     列表变更事件参数的基接口，只保证能拿到列表视图。
///     <br/>
///     Base interface for list change event args; it only guarantees access to the list view.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     相比无序的集合版，列表版额外提供索引信息，具体见
///     <see cref="IListItemRemovedEventArgs{T}"/> 等子接口。
///     <br/>
///     Unlike the unordered collection version, the list version additionally provides index information;
///     see the sub-interfaces such as <see cref="IListItemRemovedEventArgs{T}"/>.
/// </remarks>
[PublicAPI]
public interface IListUpdateEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    /// <summary>
    ///     触发本次变更的列表视图，反映变更<b>之后</b>的状态。
    ///     <br/>
    ///     The list view that raised this change, reflecting the state <b>after</b> the change.
    /// </summary>
    /// <remarks>
    ///     是<b>活视图</b>而非快照：列表之后再变，这里读到的也会跟着变，需要留档请自行拷贝。
    ///     另外事件是在内部列表改完之后才触发的，所以这里是新状态，
    ///     而各种 <c>Indexes</c> 记的是变更<b>前</b>的原始索引，两者口径不同。
    ///     <br/>
    ///     This is a <b>live view</b>, not a snapshot: later changes to the list are visible here too, so copy it
    ///     yourself if you need to keep a record. Note also that the event is raised after the underlying list has
    ///     been modified, so this view shows the new state, whereas the various <c>Indexes</c> record the original
    ///     indices <b>before</b> the change — the two are not on the same footing.
    /// </remarks>
    [PublicAPI] public IReadOnlyList<T> ListView { get; }
}

/// <summary>
///     一个或多个元素被添加至列表。
///     <br/>
///     One or more elements has added to the list.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
[PublicAPI]
public interface IListItemAddedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemAddedEventArgs<T>
{
    /// <summary>
    ///     被添加的元素，按它们在列表中出现的先后顺序排列。
    ///     <br/>
    ///     The added elements, in the order they appear in the list.
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> AddedItems { get; }

    /// <summary>
    ///     被加入的各个项的索引，按<b>升序</b>排列。
    ///     <br/>
    ///     The start index of the inserted elements (measured before the insertion).
    ///     The index of each added item, in <b>ascending</b> order.
    /// </summary>
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }

    /// <summary>
    ///     相应的索引值与 <see cref="AddedItems"/> 按位配对（Zip）的结果。
    ///     <br/>
    ///     The result of pairing (zipping) indexes with corresponding <see cref="AddedItems"/> position by position.
    /// </summary>
    [PublicAPI] public IReadOnlyList<IItemWithIndex<T>> AddedItemsWithIndex { get; }

    IReadOnlyCollection<T> ICollectionItemAddedEventArgs<T>.AddedItems => AddedItems;
}

/// <summary>
///     一个或多个元素被从列表中移除。
///     <br/>
///     One or more elements has removed from the list.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
[PublicAPI]
public interface IListItemRemovedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemRemovedEventArgs<T>
{
    /// <summary>
    ///     被移除的元素，与 <see cref="Indexes"/> <b>逐位对应且同序</b>。
    ///     <br/>
    ///     The removed elements, <b>positionally aligned and in the same order</b> as <see cref="Indexes"/>
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> RemovedItems { get; }

    /// <summary>
    ///     被移除的各个元素在<b>移除前</b>的原始索引，按<b>升序</b>排列。
    ///     <br/>
    ///     The original index of each removed element <b>before</b> removal, in <b>ascending</b> order.
    /// </summary>
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }

    /// <summary>
    ///     <see cref="Indexes"/> 与 <see cref="RemovedItems"/> 按位配对（Zip）的结果。
    ///     <br/>
    ///     The result of pairing (zipping) <see cref="Indexes"/> with <see cref="RemovedItems"/> position by position.
    /// </summary>
    /// <remarks>
    ///     依赖上面两者逐位对齐的约定，取用索引与元素请优先用这个属性。
    ///     <br/>
    ///     Relies on the positional alignment of the two properties above; prefer this property when you need both
    ///     an index and its element.
    /// </remarks>
    [PublicAPI] public IReadOnlyList<IItemWithIndex<T>> RemovedItemsWithIndex { get; }

    IReadOnlyCollection<T> ICollectionItemRemovedEventArgs<T>.RemovedItems => RemovedItems;
}

/// <summary>
///     整个列表被清空。
///     <br/>
///     The whole list was cleared.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     是移除事件的特例：<see cref="IListItemRemovedEventArgs{T}.Indexes"/> 为 <c>0..旧长度-1</c> 的连续升序，
///     <see cref="IListItemRemovedEventArgs{T}.RemovedItems"/> 即清空前的整个列表。
///     <br/>
///     A special case of the removal event: <see cref="IListItemRemovedEventArgs{T}.Indexes"/> is the contiguous
///     ascending range <c>0..oldLength-1</c>, and <see cref="IListItemRemovedEventArgs{T}.RemovedItems"/> holds the
///     entire list as it was before being cleared.
/// </remarks>
[PublicAPI]
public interface IListItemClearedEventArgs<out T> : IListItemRemovedEventArgs<T>, ICollectionItemClearedEventArgs<T>;

/// <summary>
///     一个或多个列表中的元素被替换。
///     <br/>
///     One or more elements in the list has been replaced.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
[PublicAPI]
public interface IListItemReplacedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemReplacedEventArgs<T>
{
    /// <summary>
    ///     被替换掉的旧元素，与 <see cref="Indexes"/>、<see cref="NewItems"/> 逐位对应且同序。
    ///     <br/>
    ///     The old elements that were replaced, positionally aligned and in the same order as
    ///     <see cref="Indexes"/> and <see cref="NewItems"/>.
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> OldItems { get; }

    /// <summary>
    ///     替换后的新元素，与 <see cref="Indexes"/>、<see cref="OldItems"/> 逐位对应且同序。
    ///     <br/>
    ///     The new elements, positionally aligned and in the same order as <see cref="Indexes"/> and
    ///     <see cref="OldItems"/>.
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> NewItems { get; }

    /// <summary>
    ///     各次替换发生的索引，按<b>升序</b>排列。
    ///     <br/>
    ///     The indices at which the replacements happened, in <b>ascending</b> order.
    /// </summary>
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }

    /// <summary>
    ///     <see cref="Indexes"/>、<see cref="OldItems"/>、<see cref="NewItems"/> 按位配对（Zip）的结果。
    ///     <br/>
    ///     The result of pairing (zipping) <see cref="Indexes"/>, <see cref="OldItems"/> and <see cref="NewItems"/>
    ///     position by position.
    /// </summary>
    /// <remarks>
    ///     依赖上面三者逐位对齐的约定。
    ///     <br/>
    ///     Relies on the positional alignment of the three properties above.
    /// </remarks>
    [PublicAPI] public new IReadOnlyList<IListItemReplaceInfo<T>> ItemChanges { get; }

    IReadOnlyCollection<T> ICollectionItemReplacedEventArgs<T>.OldItems => OldItems;
    IReadOnlyCollection<T> ICollectionItemReplacedEventArgs<T>.NewItems => NewItems;
    IReadOnlyCollection<IItemReplaceInfo<T>> ICollectionItemReplacedEventArgs<T>.ItemChanges => ItemChanges;
}

/// <summary>
///     列表被重置为新状态，需要重新加载。
///     <br/>
///     The list has been reset to a new state and need to be reloaded.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
[PublicAPI]
public interface IListResetEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionResetEventArgs<T>;

/// <summary>
///     列表中的元素被移动位置。
///     <br/>
///     Elements in the list has been moved to another index.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     存在多条记录时，将依照操作顺序排序，列表项和前后的索引依序一一对应。
///     <br/>
///     When there are multiple records, they will be sorted according to the order of movement,
///     and the items and there old and new indexes will correspond one-to-one.
/// </remarks>
[PublicAPI]
public interface IListItemMovedEventArgs<out T> : IListUpdateEventArgs<T>
{
    /// <summary>
    ///     被移动的列表项，按<b>移动顺序</b>排列。
    ///     <br/>
    ///     The item which has been moved, in the order in <b>which they were moved</b>.
    /// </summary>
    IReadOnlyList<T> Items { get; }

    /// <summary>
    ///     列表项在被移动前的位置，与 <see cref="Items"/>、<see cref="Items"/> 逐位对应且同序。
    ///     <br/>
    ///     The index of items before moved, positionally aligned and in the same order as
    ///     <see cref="Items"/> and <see cref="Items"/>.
    /// </summary>
    IReadOnlyList<int> OldIndexes { get; }

    /// <summary>
    ///     列表项在被移动后的位置，与 <see cref="Items"/>、<see cref="Items"/> 逐位对应且同序。
    ///     <br/>
    ///     The index of items after moved, positionally aligned and in the same order as
    ///     <see cref="Items"/> and <see cref="Items"/>.
    /// </summary>
    IReadOnlyList<int> NewIndexes { get; }

    /// <summary>
    ///     <see cref="Items"/>、<see cref="OldIndexes"/>、<see cref="NewIndexes"/> 按位配对（Zip）的结果。
    ///     <br/>
    ///     The result of pairing (zipping) <see cref="Items"/>, <see cref="OldIndexes"/> and <see cref="NewIndexes"/>
    ///     position by position.
    /// </summary>
    /// <remarks>
    ///     依赖上面三者逐位对齐的约定。
    ///     <br/>
    ///     Relies on the positional alignment of the three properties above.
    /// </remarks>
    [PublicAPI] public IReadOnlyList<IListItemMovedInfo<T>> ItemMoves { get; }
}

/// <summary>
///     列表中的元素被重新排序。
///     <br/>
///     Elements in the list has been resorted.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <remarks>
///     元素本身没有增减，只是整表重排，因此不携带任何索引信息；
///     需要新顺序请直接读 <see cref="IListUpdateEventArgs{T}.ListView"/>。
///     <br/>
///     No elements were added or removed — the whole list was merely reordered — so no index information is
///     carried. Read <see cref="IListUpdateEventArgs{T}.ListView"/> directly for the new order.
/// </remarks>
[PublicAPI]
public interface IListSortedEventArgs<out T> : IListUpdateEventArgs<T> { }