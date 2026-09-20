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
}
