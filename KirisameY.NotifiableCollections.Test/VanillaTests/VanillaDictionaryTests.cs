using System.Collections;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.VanillaTests;

/// <summary>
///     <c>AsNotifiableDictionary</c>：把标准库风格的 <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
///     字典包装成本库通知模型的只读字典视图，连带 <c>Keys</c> / <c>Values</c> 两个子视图。
/// </summary>
/// <remarks>
///     标准库没有可观测字典，用的是 <see cref="FakeNotifyDictionary{TKey,TValue}"/>：
///     它按标准库的形状发通知，项一律是键值对。
/// </remarks>
public class VanillaDictionaryTests
{
    /// <summary>两对键值对的源。</summary>
    private static FakeNotifyDictionary<string, int> PairSource() =>
        new FakeNotifyDictionary<string, int>().With("a", 1).With("b", 2);

    [Fact]
    public void ViewReflectsTheSourceContents()
    {
        var source = PairSource();

        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        Assert.Equal(1, view["a"]);
        Assert.True(view.ContainsKey("b"));
        Assert.True(view.TryGetValue("b", out var value));
        Assert.Equal(2, value);
        // Count 是独立于索引器与查找的一条路径，单独确认一次（取到局部变量是为了绕开 xUnit2013 分析器）
        var count = view.Count;
        Assert.Equal(2, count);
        Assert.Equal(
            [new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2)],
            view.OrderBy(p => p.Key)
        );
    }

    [Fact]
    public void ViewReportsMissingKeysLikeAPlainDictionary()
    {
        var source = PairSource();

        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        Assert.False(view.ContainsKey("z"));
        Assert.False(view.TryGetValue("z", out _));
        Assert.Throws<KeyNotFoundException>(() => view["z"]);
    }

    [Fact]
    public void ViewIsLive()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        source.Add("a", 1);

        Assert.Equal(1, view["a"]);
        Assert.Single(view);
    }

    [Fact]
    public void AddIsReportedWithTheAddedPairs()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        source.Add("a", 1);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, added.AddedItems["a"]);
    }

    [Fact]
    public void RemoveIsReportedWithTheRemovedPairs()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        source.Remove("a");

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
    }

    [Fact]
    public void ReplaceIsReportedWithTheOldAndNewValuesOfEachKey()
    {
        var source = new FakeNotifyDictionary<string, int>().With("a", 1);
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        source.Set("a", 2);

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(2, replaced.NewItems["a"]);

        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal("a", change.Key);
        Assert.Equal(1, change.OldValue);
        Assert.Equal(2, change.NewValue);
    }

    [Fact]
    public void ResetIsReportedAsResetWithoutTheClearedPairs()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        source.Clear();

        // 标准库的清空只发 Reset，所以这里拿不到携带全部旧键值对的 Cleared
        var reset = Assert.IsAssignableFrom<IDictionaryResetEventArgs<string, int>>(Assert.Single(updates));
        Assert.Empty(reset.DictionaryView);
    }

    [Fact]
    public void EventArgsPointAtTheViewItself()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        DictionaryUpdateEventArgs<string, int>? captured = null;
        view.DictionaryUpdated += (_, args) => captured = args;

        source.Add("a", 1);

        Assert.Same(view, captured!.DictionaryView);
        Assert.Same(view, captured.CollectionView);
    }

    [Fact]
    public void SenderIsTheViewItself()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        object? sender = null;
        view.DictionaryUpdated += (s, _) => sender = s;

        source.Add("a", 1);

        Assert.Same(view, sender);
    }

    [Fact]
    public void KeysViewReflectsTheSourceKeys()
    {
        var source = PairSource();

        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        Assert.Equal(["a", "b"], view.Keys);
    }

    [Fact]
    public void KeysViewIsLive()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        source.Add("a", 1);
        Assert.Equal(["a"], view.Keys);

        source.Remove("a");
        Assert.Empty(view.Keys);
    }

    [Fact]
    public void KeysViewReportsAddsAndRemovals()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view.Keys);

        source.Add("c", 3);
        source.Remove("a");

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<string>>(updates[0]);
        Assert.Equal(["c"], added.AddedItems);
        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<string>>(updates[1]);
        Assert.Equal(["a"], removed.RemovedItems);
    }

    [Fact]
    public void KeysViewIgnoresValueReplacement()
    {
        var source = new FakeNotifyDictionary<string, int>().With("a", 1);
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view.Keys);

        source.Set("a", 2);

        // 只换了值、没动键，键视图无话可说
        Assert.Empty(updates);
        Assert.Equal(["a"], view.Keys);
    }

    [Fact]
    public void KeysViewRaisesResetOnClear()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view.Keys);

        source.Clear();

        Assert.IsAssignableFrom<ICollectionResetEventArgs<string>>(Assert.Single(updates));
        Assert.Empty(view.Keys);
    }

    [Fact]
    public void ValuesViewReflectsTheSourceValues()
    {
        var source = PairSource();

        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        Assert.Equal([1, 2], view.Values);
    }

    [Fact]
    public void ValuesViewIsLive()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        source.Add("a", 1);
        Assert.Equal([1], view.Values);

        source.Remove("a");
        Assert.Empty(view.Values);
    }

    [Fact]
    public void ValuesViewReportsAddsRemovalsAndReplacements()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view.Values);

        source.Add("c", 3);
        source.Remove("a");
        source.Set("b", 9);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(updates[0]);
        Assert.Equal([3], added.AddedItems);
        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<int>>(updates[1]);
        Assert.Equal([1], removed.RemovedItems);
        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(updates[2]);
        Assert.Equal([2], replaced.OldItems);
        Assert.Equal([9], replaced.NewItems);
    }

    [Fact]
    public void ValuesViewRaisesResetOnClear()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view.Values);

        source.Clear();

        // Values 是在字典的集合视图上做的投影视图，清空走的是 Reset 那条路，理应一并转发出去
        Assert.IsAssignableFrom<ICollectionResetEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(view.Values);
    }

    [Fact]
    public void KeysAndValuesViewsAreCachedPerDictionaryView()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        Assert.Same(view.Keys, view.Keys);
        Assert.Same(view.Values, view.Values);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var count = 0;
        EventHandler<DictionaryUpdateEventArgs<string, int>> handler = (_, _) => count++;
        view.DictionaryUpdated += handler;
        source.Add("a", 1);

        view.DictionaryUpdated -= handler;
        source.Add("b", 2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void UnsubscribingAnUnknownHandlerIsIgnored()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);
        EventHandler<DictionaryUpdateEventArgs<string, int>> unknown = (_, _) => { };
        source.Add("a", 1);

        var exception = Record.Exception(() => view.DictionaryUpdated -= unknown);
        source.Add("b", 2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void ViewIsUsableAsAPlainNotifiableCollection()
    {
        var source = new FakeNotifyDictionary<string, int>();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();
        // 字典视图本身也是集合视图，按集合订阅也照样收到
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.Add("a", 1);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<KeyValuePair<string, int>>>(Assert.Single(updates));
        // 走集合这条路时项就是键值对本身，按集合的接口读，不再是按键组织的字典
        Assert.Equal(new KeyValuePair<string, int>("a", 1), Assert.Single(added.AddedItems));
    }

    [Fact]
    public void NonGenericEnumerationAlsoYieldsThePairs()
    {
        var source = PairSource();
        var view = source.AsNotifiableDictionary<string, int, FakeNotifyDictionary<string, int>>();

        Assert.Equal(
            [new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2)],
            ((IEnumerable)view).Cast<KeyValuePair<string, int>>().OrderBy(p => p.Key)
        );
    }
}
