using System.Collections.Immutable;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Data;
using KirisameY.NotifiableCollections.Utils;

namespace KirisameY.NotifiableCollections.EventArgs;

/// <summary>
///     列表变更事件参数的公共基类。
///     <br/>
///     Common base class for list change event args.
/// </summary>
/// <typeparam name="T">
///     元素类型。
///     <br/>
///     The type of the elements.
/// </typeparam>
/// <param name="listView">
///     触发本次变更的列表视图。
///     <br/>
///     The list view that raised this change.
/// </param>
public abstract class ListUpdateEventArgs<T>(IReadOnlyList<T> listView) : CollectionUpdateEventArgs<T>(listView), IListUpdateEventArgs<T>
{
    /// <inheritdoc cref="IListUpdateEventArgs{T}.ListView"/>
    public IReadOnlyList<T> ListView => listView;
}

internal class ListItemAddedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> addedItems, IReadOnlyList<int> indexes)
    : ListUpdateEventArgs<T>(listView), IListItemAddedEventArgs<T>
{
    public IReadOnlyList<T> AddedItems => addedItems;
    public IReadOnlyList<int> Indexes => indexes;

    public IReadOnlyList<IItemWithIndex<T>> AddedItemsWithIndex => field ??=
        [..Indexes.Zip(AddedItems, ItemWithIndex.From)];
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

internal class ListResetEventArgs<T>(IReadOnlyList<T> listView) : ListUpdateEventArgs<T>(listView), IListResetEventArgs<T>;

internal class ListItemMovedEventArgs<T>(IReadOnlyList<T> listView, IReadOnlyList<T> items, IReadOnlyList<int> oldIndexes, IReadOnlyList<int> newIndexes)
    : ListUpdateEventArgs<T>(listView), IListItemMovedEventArgs<T>
{
    public IReadOnlyList<T> Items => items;
    public IReadOnlyList<int> OldIndexes => oldIndexes;
    public IReadOnlyList<int> NewIndexes => newIndexes;
    public IReadOnlyList<IListItemMovedInfo<T>> ItemMoves => field ??=
    [
        .. Items.Zip(OldIndexes, NewIndexes)
                .Select(t => ListItemMoveInfo.From(t.First, t.Second, t.Third))
    ];
}

internal class ListSortedEventArgs<T>(
    IReadOnlyList<T> listView, IReadOnlyList<T> items, IReadOnlyList<int> oldIndexes, IReadOnlyList<int> newIndexes
) : ListItemMovedEventArgs<T>(listView, items, oldIndexes, newIndexes), IListSortedEventArgs<T>
{
    public static ListSortedEventArgs<T> From(IReadOnlyList<T> listView, IReadOnlyList<T> snapshotBeforeSort)
    {
        var dict = snapshotBeforeSort.Index()
                                     .Reverse()
                                     .GroupBy(
                                          t => t.Item,
                                          (k, c) => KeyValuePair.Create(
                                              Optional.FromNullable(k), c.Select(t => t.Index).ToList()))
                                     .ToDictionary();
        var order = listView.Select(t =>
        {
            var l = dict[Optional.FromNullable(t)];
            var result = l.Last();
            l.RemoveAt(l.Count - 1);
            return result;
        }).ToArray();
        var lis = MathUtils.ResolveLis(order);


        var moveTimes = order.Length - lis.Length;
        List<(T Item, int From, int To)> moves = new(capacity: moveTimes);

        {
            // 同时逆序扫描源和目标序列，找到每一步移动的顺序
            HashSet<int> lisSet = [..lis];
            List<int> movedFront = new(capacity: moveTimes),
                      movedBack = new(capacity: moveTimes);
            int p, pTarget,
                prevOffset = 0; // 插入目标点向前的反向偏移
            p = pTarget = order.Length - 1;
            while (pTarget >= 0)
            {
                if (p >= 0 && p == order[pTarget])
                {
                    p--;
                    pTarget--;
                    continue;
                }

                if (p >= 0 && !lisSet.Contains(p)) p--;
                else
                {
                    var target = order[pTarget];
                    int from, to;
                    {
                        from = target
                          - MathUtils.BinaryFindFirst(movedFront, target, static (i, t) => i >= t)
                          + MathUtils.BinaryFindFirst(movedBack, target, static (i, t) => i >= t);

                        to = p - prevOffset - MathUtils.BinaryFindFirst(movedFront, p, static (i, t) => i >= t);
                        if (from > to) to += 1; // 从前方移动时当前项会自动前移，但从后方时不会，需要移至后面一位

                        MathUtils.BinarySortedAdd(movedFront, target, (i, t) => i >= t);
                        MathUtils.BinarySortedAdd(movedBack, p, (i, t) => i >= t);
                    }
                    moves.Add((listView[pTarget], from, to));
                    pTarget--;
                }
            }
        }

        return new(listView,
                   [..moves.Select(t => t.Item)],
                   [..moves.Select(t => t.From)],
                   [..moves.Select(t => t.To)]
        );
    }
}