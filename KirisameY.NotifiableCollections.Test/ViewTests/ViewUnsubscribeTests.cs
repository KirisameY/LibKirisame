using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

/// <summary>
///     包装器退订路径的覆盖。
///     每个包装器都自带一份「把源事件参数改写成自己这份」的逻辑，
///     退订必须能摘掉对应的那一份；摘不掉的话转换用的委托会留在源集合上，
///     继续把通知转成已经没人要的事件发出来。
/// </summary>
public class ViewUnsubscribeTests
{
    // ---- 投影包装器 ----

    [Fact]
    public void ProjectingCollectionViewStopsForwardingAfterUnsubscribing()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        view.CollectionUpdated += handler;
        list.Add("a");

        view.CollectionUpdated -= handler;
        list.Add("bb");

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProjectingListViewStopsForwardingAfterUnsubscribing()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var count = 0;
        EventHandler<ListUpdateEventArgs<int>> handler = (_, _) => count++;
        view.ListUpdated += handler;
        list.Add("a");

        view.ListUpdated -= handler;
        list.Add("bb");

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProjectingDictionaryViewStopsForwardingAfterUnsubscribing()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());
        var count = 0;
        EventHandler<DictionaryUpdateEventArgs<string, string>> handler = (_, _) => count++;
        view.DictionaryUpdated += handler;
        dictionary.Add("a", 1);

        view.DictionaryUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }

    // ---- 字典的 Keys / Values 视图 ----

    [Fact]
    public void DictionaryKeysViewStopsForwardingAfterUnsubscribing()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<string>> handler = (_, _) => count++;
        dictionary.Keys.CollectionUpdated += handler;
        dictionary.Add("a", 1);

        dictionary.Keys.CollectionUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void DictionaryValuesViewStopsForwardingAfterUnsubscribing()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        dictionary.Values.CollectionUpdated += handler;
        dictionary.Add("a", 1);

        dictionary.Values.CollectionUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }

    // ---- 集合级那个别名事件的退订 ----
    // 它是显式实现转发到各自的 ListUpdated / DictionaryUpdated 上的，退订也要跟着转发到同一处

    [Fact]
    public void ListCollectionLevelSubscriptionCanBeRemoved()
    {
        var list = new NotifiableList<int>();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        var notifier = (ICollectionUpdateNotifier<int>)list;
        notifier.CollectionUpdated += handler;
        list.Add(1);

        notifier.CollectionUpdated -= handler;
        list.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void DictionaryCollectionLevelSubscriptionCanBeRemoved()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<KeyValuePair<string, int>>> handler = (_, _) => count++;
        var notifier = (ICollectionUpdateNotifier<KeyValuePair<string, int>>)dictionary;
        notifier.CollectionUpdated += handler;
        dictionary.Add("a", 1);

        notifier.CollectionUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ListViewCollectionLevelSubscriptionCanBeRemoved()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        var notifier = (ICollectionUpdateNotifier<int>)view;
        notifier.CollectionUpdated += handler;
        list.Add(1);

        notifier.CollectionUpdated -= handler;
        list.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProjectingListViewCollectionLevelSubscriptionCanBeRemoved()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        var notifier = (ICollectionUpdateNotifier<int>)view;
        notifier.CollectionUpdated += handler;
        list.Add("a");

        notifier.CollectionUpdated -= handler;
        list.Add("bb");

        Assert.Equal(1, count);
    }

    [Fact]
    public void DictionaryViewCollectionLevelSubscriptionCanBeRemoved()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = dictionary.AsReadOnlyNotifiableDictionary();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<KeyValuePair<string, int>>> handler = (_, _) => count++;
        var notifier = (ICollectionUpdateNotifier<KeyValuePair<string, int>>)view;
        notifier.CollectionUpdated += handler;
        dictionary.Add("a", 1);

        notifier.CollectionUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }
}
