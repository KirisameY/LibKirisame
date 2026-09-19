using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;

namespace KirisameY.NotifiableCollections.EventArgs;

#region Interfaces

[PublicAPI]
public interface IListUpdateEventArgs<out T> : ICollectionUpdateEventArgs<T>
{
    /// <summary>
    /// 变更<b>之后</b>的列表快照。
    /// </summary>
    /// <remarks>
    /// 事件是在内部列表改完之后才触发的，所以这里看到的是新状态；
    /// 而下面各种 <c>Indexes</c> 记的是变更<b>前</b>的原始索引，两者口径不同。
    /// </remarks>
    [PublicAPI] public IReadOnlyList<T> ListView { get; }
}

[PublicAPI]
public interface IListItemAddedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemAddedEventArgs<T>
{
    /// <summary>
    /// 被添加的元素，按它们在列表中出现的先后顺序排列。
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> AddedItems { get; }

    /// <summary>
    /// 这批元素插入的起始索引（插入前的口径）。
    /// </summary>
    /// <remarks>
    /// 批量添加插入的是连续一段，所以一个起点就足以定位，
    /// <see cref="AddedItems"/> 不必像移除那样额外配一份索引。
    /// </remarks>
    [PublicAPI] public int StartIndex { get; }

    IReadOnlyCollection<T> ICollectionItemAddedEventArgs<T>.AddedItems => AddedItems;
}

[PublicAPI]
public interface IListItemRemovedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemRemovedEventArgs<T>
{
    /// <summary>
    /// 被移除的元素，与 <see cref="Indexes"/> <b>逐位对应且同序</b>（即同样按索引升序）。
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> RemovedItems { get; }

    /// <summary>
    /// 各被移除元素在<b>移除前</b>的原始索引，<b>严格升序</b>（不要求连续）。
    /// </summary>
    /// <remarks>
    /// 统一约定为升序，批量移除的实现必须维持这一点。
    /// 存的是移除前的口径：移除后列表会收缩，用新索引无法还原这些元素本来的位置。
    /// </remarks>
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }

    /// <summary>
    /// <see cref="Indexes"/> 与 <see cref="RemovedItems"/> 按位配对（Zip）的结果。
    /// </summary>
    /// <remarks>依赖上面两者逐位对齐的约定，取用索引与元素请优先用这个属性。</remarks>
    [PublicAPI] public IReadOnlyList<IItemWithIndex<T>> RemovedItemsWithIndex { get; }

    IReadOnlyCollection<T> ICollectionItemRemovedEventArgs<T>.RemovedItems => RemovedItems;
}

/// <summary>
/// 清空整个列表。
/// </summary>
/// <remarks>
/// 是移除事件的特例：<see cref="IListItemRemovedEventArgs{T}.Indexes"/> 为 <c>0..旧长度-1</c> 的连续升序，
/// <see cref="IListItemRemovedEventArgs{T}.RemovedItems"/> 即清空前的整个列表。
/// </remarks>
[PublicAPI]
public interface IListItemClearedEventArgs<out T> : IListItemRemovedEventArgs<T>;

[PublicAPI]
public interface IListItemReplacedEventArgs<out T> : IListUpdateEventArgs<T>, ICollectionItemReplacedEventArgs<T>
{
    /// <summary>
    /// 被替换掉的旧元素，与 <see cref="Indexes"/>、<see cref="NewItems"/> 逐位对应且同序。
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> OldItems { get; }

    /// <summary>
    /// 替换后的新元素，与 <see cref="Indexes"/>、<see cref="OldItems"/> 逐位对应且同序。
    /// </summary>
    [PublicAPI] public new IReadOnlyList<T> NewItems { get; }

    /// <summary>
    /// 各次替换发生的索引，按<b>升序</b>排列。
    /// </summary>
    [PublicAPI] public IReadOnlyList<int> Indexes { get; }

    /// <summary>
    /// <see cref="Indexes"/>、<see cref="OldItems"/>、<see cref="NewItems"/> 按位配对（Zip）的结果。
    /// </summary>
    /// <remarks>依赖上面三者逐位对齐的约定。</remarks>
    [PublicAPI] public new IReadOnlyList<IListItemReplaceInfo<T>> ItemChanges { get; }

    IReadOnlyCollection<T> ICollectionItemReplacedEventArgs<T>.OldItems => OldItems;
    IReadOnlyCollection<T> ICollectionItemReplacedEventArgs<T>.NewItems => NewItems;
    IReadOnlyCollection<IItemReplaceInfo<T>> ICollectionItemReplacedEventArgs<T>.ItemChanges => ItemChanges;
}

/// <summary>
/// 排序 / 反转事件：整表重排，不携带任何索引信息。
/// </summary>
/// <remarks>需要新顺序请直接读 <see cref="IListUpdateEventArgs{T}.ListView"/>。</remarks>
[PublicAPI]
public interface IListSortedEventArgs<out T> : IListUpdateEventArgs<T> { }

#endregion

public abstract class ListUpdateEventArgs<T>(IReadOnlyList<T> listView) : CollectionUpdateEventArgs<T>(listView), IListUpdateEventArgs<T>
{
    public IReadOnlyList<T> ListView => listView;
}

internal class ListItemAddedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> addedItems, int startIndex)
    : ListUpdateEventArgs<T>(listView), IListItemAddedEventArgs<T>
{
    public IReadOnlyList<T> AddedItems => addedItems;
    public int StartIndex => startIndex;
}

internal class ListItemRemovedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> removedItems, IReadOnlyList<int> indexes)
    : ListUpdateEventArgs<T>(listView), IListItemRemovedEventArgs<T>
{
    public IReadOnlyList<T> RemovedItems => removedItems;
    public IReadOnlyList<int> Indexes => indexes;
    public IReadOnlyList<IItemWithIndex<T>> RemovedItemsWithIndex => field ??= [..Indexes.Zip(RemovedItems, ItemWithIndex.From)];
}

internal class ListItemClearedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> removedItems, IReadOnlyList<int> indexes)
    : ListItemRemovedEventArgs<T>(listView, removedItems, indexes), IListItemClearedEventArgs<T>;

internal class ListItemReplacedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> oldItems, IReadOnlyList<T> newItems, IReadOnlyList<int> indexes)
    : ListUpdateEventArgs<T>(listView), IListItemReplacedEventArgs<T>
{
    public IReadOnlyList<T> OldItems => oldItems;
    public IReadOnlyList<T> NewItems => newItems;
    public IReadOnlyList<int> Indexes => indexes;

    public IReadOnlyList<IListItemReplaceInfo<T>> ItemChanges => field ??=
    [
        ..Indexes.Zip(OldItems, NewItems)
                 .Select(t => ItemReplaceInfo.From(t.First, t.Second, t.Third))
    ];
}

internal class ListSortedEventArgs<T>(IReadOnlyList<T> listView) : ListUpdateEventArgs<T>(listView), IListSortedEventArgs<T>;