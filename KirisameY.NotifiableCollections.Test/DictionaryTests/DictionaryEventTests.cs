using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

public class DictionaryEventTests
{
    [Fact]
    public void SenderIsTheDictionaryItself()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        object? sender = null;
        dictionary.DictionaryUpdated += (s, _) => sender = s;

        dictionary.Add("a", 1);

        Assert.Same(dictionary, sender);
    }

    [Fact]
    public void DictionaryViewIsLiveNotSnapshot()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        IReadOnlyDictionary<string, int>? view = null;
        dictionary.DictionaryUpdated += (_, args) => view = args.DictionaryView;

        dictionary.Add("a", 1);
        Assert.Equal(1, view!["a"]);

        // 事件里拿到的是活视图，之后字典再变它也跟着变
        dictionary.Add("b", 2);
        Assert.Equal(2, view!.Count);
    }

    [Fact]
    public void DictionaryViewReflectsTheStateAfterTheChange()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        IReadOnlyDictionary<string, int>? view = null;
        dictionary.DictionaryUpdated += (_, args) => view = args.DictionaryView;

        dictionary.Remove("a");

        Assert.Equal(["b"], view!.Keys);
    }

    [Fact]
    public void UnsubscribedHandlerStopsReceiving()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var count = 0;
        EventHandler<DictionaryUpdateEventArgs<string, int>> handler = (_, _) => count++;
        dictionary.DictionaryUpdated += handler;
        dictionary.Add("a", 1);

        dictionary.DictionaryUpdated -= handler;
        dictionary.Add("b", 2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void CollectionUpdatedAndDictionaryUpdatedAreTheSameEvent()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        DictionaryUpdateEventArgs<string, int>? fromDictionary = null;
        CollectionUpdateEventArgs<KeyValuePair<string, int>>? fromCollection = null;
        dictionary.DictionaryUpdated += (_, args) => fromDictionary = args;
        ((ICollectionUpdateNotifier<KeyValuePair<string, int>>)dictionary).CollectionUpdated +=
            (_, args) => fromCollection = args;

        dictionary.Add("a", 1);

        // 集合级事件是显式实现转发到 DictionaryUpdated 的，两边收到的是同一个 args 实例
        Assert.Same(fromDictionary, fromCollection);
    }

    [Fact]
    public void KeysViewNotifiesWhenTheDictionaryChanges()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary.Add("a", 1);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<string>>(Assert.Single(updates));
        Assert.Equal(["a"], added.AddedItems);
    }

    [Fact]
    public void KeysViewReportsRemovals()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary.Remove("a");

        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<string>>(Assert.Single(updates));
        Assert.Equal(["a"], removed.RemovedItems);
    }

    [Fact]
    public void KeysViewStaysSilentOnAValueOnlyReplacement()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary["a"] = 2;

        // 替换不影响键集合，所以这里不发生通知
        Assert.Empty(updates);
    }

    [Fact]
    public void KeysViewIsTheSameInstanceEachTime()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.Same(dictionary.Keys, dictionary.Keys);
        Assert.Same(dictionary.Values, dictionary.Values);
    }

    [Fact]
    public void ValuesViewNotifiesWhenTheDictionaryChanges()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Values);

        dictionary.Add("a", 1);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], added.AddedItems);
    }

    [Fact]
    public void ValuesViewReportsTheReplacedValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Values);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], replaced.OldItems);
        Assert.Equal([2], replaced.NewItems);
    }

    [Fact]
    public void KeysViewReportsClearedKeys()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);

        dictionary.Clear();

        var cleared = Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<string>>(Assert.Single(updates));

        // 快照用的是 ImmutableDictionary，枚举顺序不保证，比较前先排一下
        Assert.Equal(["a", "b"], cleared.RemovedItems.OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void ValuesViewReportsClearedValues()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Values);

        dictionary.Clear();

        var cleared = Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2], cleared.RemovedItems.OrderBy(v => v));
    }
}
