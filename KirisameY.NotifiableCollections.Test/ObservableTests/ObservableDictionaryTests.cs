using System.Collections.Specialized;
using System.ComponentModel;

using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ObservableTests;

/// <summary>
///     字典版的 <c>AsReadOnlyObservableDictionary</c> 产出的视图：
///     变更以键值对为单位报告，且 <c>Keys</c> / <c>Values</c> 自身也是可观测视图。
/// </summary>
public class ObservableDictionaryTests
{
    [Fact]
    public void ViewReflectsTheSourceContents()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        IReadOnlyNotifiableDictionary<string, int> source = dict;

        var view = source.AsReadOnlyObservableDictionary();

        Assert.Equal(
            [new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2)],
            view.OrderBy(p => p.Key)
        );
        // Count 是独立于枚举的一条路径，单独确认一次（取到局部变量是为了绕开 xUnit2013 分析器）
        var count = view.Count;
        Assert.Equal(2, count);
    }

    [Fact]
    public void ViewIsLive()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;

        var view = source.AsReadOnlyObservableDictionary();

        dict.Add("a", 1);

        Assert.Equal(1, view["a"]);
    }

    [Fact]
    public void ContainsKeyAndTryGetValueDelegateToTheSource()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();

        Assert.True(view.ContainsKey("a"));
        Assert.False(view.ContainsKey("missing"));
        Assert.True(view.TryGetValue("a", out var value));
        Assert.Equal(1, value);
        Assert.False(view.TryGetValue("missing", out _));
    }

    [Fact]
    public void IndexerThrowsForAMissingKey()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();

        Assert.Throws<KeyNotFoundException>(() => view["missing"]);
    }

    [Fact]
    public void KeysAndValuesViewsReflectTheSource()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        IReadOnlyNotifiableDictionary<string, int> source = dict;

        var view = source.AsReadOnlyObservableDictionary();

        Assert.Equal(["a", "b"], view.Keys.OrderBy(k => k));
        Assert.Equal([1, 2], view.Values.OrderBy(v => v));
    }

    [Fact]
    public void AddRaisesAddWithTheKeyValuePair()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        dict.Add("a", 1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
        Assert.Equal([new KeyValuePair<string, int>("a", 1)], ObservableTestUtils.Elements<KeyValuePair<string, int>>(args.NewItems));
    }

    [Fact]
    public void RemoveRaisesRemoveWithTheKeyValuePair()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        dict.Remove("a");

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
        Assert.Equal([new KeyValuePair<string, int>("a", 1)], ObservableTestUtils.Elements<KeyValuePair<string, int>>(args.OldItems));
    }

    [Fact]
    public void ReplaceRaisesReplaceWithOldAndNewPairs()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        dict["a"] = 2;

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
        Assert.Equal([new KeyValuePair<string, int>("a", 1)], ObservableTestUtils.Elements<KeyValuePair<string, int>>(args.OldItems));
        Assert.Equal([new KeyValuePair<string, int>("a", 2)], ObservableTestUtils.Elements<KeyValuePair<string, int>>(args.NewItems));
    }

    [Fact]
    public void AddAndRemoveRaiseCountAndIndexerPropertyChanged()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        dict.Add("a", 1);
        dict.Remove("a");

        Assert.Equal(["Count", "Item[]", "Count", "Item[]"], names);
    }

    [Fact]
    public void ReplaceRaisesIndexerPropertyChangedOnly()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        dict["a"] = 2;

        Assert.Equal(["Item[]"], names);
    }

    [Fact]
    public void KeysViewRaisesAddForANewKey()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view.Keys);

        dict.Add("a", 1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
        Assert.Equal(["a"], ObservableTestUtils.Elements<string>(args.NewItems));
    }

    [Fact]
    public void KeysViewStaysSilentWhenOnlyAValueChanges()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view.Keys);

        dict["a"] = 2;

        // 键没变，键视图不该有任何动静
        Assert.Empty(events);
    }

    [Fact]
    public void ValuesViewRaisesReplaceWhenAValueChanges()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = ObservableTestUtils.RecordCollectionChanged(view.Values);

        dict["a"] = 2;

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
        Assert.Equal([1], ObservableTestUtils.Elements<int>(args.OldItems));
        Assert.Equal([2], ObservableTestUtils.Elements<int>(args.NewItems));
    }

    [Fact]
    public void SenderIsTheViewItself()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        object? sender = null;
        view.CollectionChanged += (s, _) => sender = s;

        dict.Add("a", 1);

        Assert.Same(view, sender);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var events = new List<NotifyCollectionChangedEventArgs>();
        NotifyCollectionChangedEventHandler handler = (_, args) => events.Add(args);
        view.CollectionChanged += handler;
        dict.Add("a", 1);

        view.CollectionChanged -= handler;
        dict.Add("b", 2);

        Assert.Single(events);
    }

    [Fact]
    public void ViewStopsForwardingPropertyChangedAfterUnsubscribing()
    {
        var dict = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();
        var names = new List<string?>();
        PropertyChangedEventHandler handler = (_, args) => names.Add(args.PropertyName);
        view.PropertyChanged += handler;
        dict.Add("a", 1);

        view.PropertyChanged -= handler;
        dict.Add("b", 2);

        Assert.Equal(["Count", "Item[]"], names);
    }

    [Fact]
    public void ViewEnumeratesNonGenerically()
    {
        var dict = new NotifiableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        IReadOnlyNotifiableDictionary<string, int> source = dict;
        var view = source.AsReadOnlyObservableDictionary();

        Assert.Equal(
            [new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2)],
            ObservableTestUtils.Enumerate(view)
        );
    }
}
