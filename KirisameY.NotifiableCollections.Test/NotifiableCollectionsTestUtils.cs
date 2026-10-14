using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test;

public static class NotifiableCollectionsTestUtils
{
    /// <summary>
    ///     订阅列表通知，并按触发顺序收集事件参数。
    /// </summary>
    public static List<ListUpdateEventArgs<T>> RecordListUpdates<T>(IListUpdateNotifier<T> notifier)
    {
        List<ListUpdateEventArgs<T>> updates = [];
        notifier.ListUpdated += (_, args) => updates.Add(args);
        return updates;
    }

    /// <summary>
    ///     订阅集合通知，并按触发顺序收集事件参数。
    /// </summary>
    public static List<CollectionUpdateEventArgs<T>> RecordCollectionUpdates<T>(ICollectionUpdateNotifier<T> notifier)
    {
        List<CollectionUpdateEventArgs<T>> updates = [];
        notifier.CollectionUpdated += (_, args) => updates.Add(args);
        return updates;
    }

    /// <summary>
    ///     订阅字典通知，并按触发顺序收集事件参数。
    /// </summary>
    public static List<DictionaryUpdateEventArgs<TKey, TValue>> RecordDictionaryUpdates<TKey, TValue>(
        IDictionaryUpdateNotifier<TKey, TValue> notifier
    )
    {
        List<DictionaryUpdateEventArgs<TKey, TValue>> updates = [];
        notifier.DictionaryUpdated += (_, args) => updates.Add(args);
        return updates;
    }

    /// <summary>
    ///     把移动事件里报的那串移动按顺序重放到一份副本上，返回照做一遍之后的样子。
    /// </summary>
    /// <remarks>
    ///     移动事件报的是一串「从哪儿挪到哪儿」的操作（<c>ItemMoves</c>），
    ///     每一步的索引都是<b>那一刻</b>的口径，所以要一步一步重放才复现得出来；
    ///     重放的结果应当正好等于事件所属的那次操作之后列表的样子。
    /// </remarks>
    public static List<T> ReplayMoves<T>(IEnumerable<T> original, IListItemMovedEventArgs<T> moved)
    {
        var list = original.ToList();
        foreach (var move in moved.ItemMoves)
        {
            var item = list[move.OldIndex];
            Assert.Equal(move.Item, item);
            list.RemoveAt(move.OldIndex);
            list.Insert(move.NewIndex, item);
        }
        return list;
    }
}
