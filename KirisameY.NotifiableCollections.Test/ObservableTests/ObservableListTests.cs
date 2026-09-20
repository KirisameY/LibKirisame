using System.Collections.Specialized;
using System.ComponentModel;

using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ObservableTests;

/// <summary>
///     列表版的 <c>AsReadOnlyObservableCollection</c> 产出的视图：
///     除集合版的通知外，还要求携带索引、且排序一律走 Reset。
/// </summary>
/// <remarks>
///     接收者声明成 <see cref="IReadOnlyNotifiableList{T}"/>，以确保命中列表版那个扩展方法。
/// </remarks>
public class ObservableListTests
{
    [Fact]
    public void ViewReflectsTheSourceContentsAndIndexer()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableList<int> source = list;

        var view = source.AsReadOnlyObservableCollection();

        Assert.Equal([1, 2, 3], view);
        Assert.Equal(2, view[1]);
        // Count 是独立于枚举与索引器的一条路径，单独确认一次（取到局部变量是为了绕开 xUnit2013 分析器）
        var count = view.Count;
        Assert.Equal(3, count);
    }

    [Fact]
    public void ViewIsLive()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;

        var view = source.AsReadOnlyObservableCollection();

        list.Add(1);

        Assert.Equal([1], view);
    }

    [Fact]
    public void AddCarriesTheIndex()
    {
        NotifiableList<int> list = [0];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Add(1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
        Assert.Equal(1, args.NewStartingIndex);
        Assert.Equal([1], ObservableTestUtils.Elements<int>(args.NewItems));
    }

    [Fact]
    public void InsertCarriesTheIndex()
    {
        NotifiableList<int> list = [0, 2];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Insert(1, 1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
        Assert.Equal(1, args.NewStartingIndex);
        Assert.Equal([1], ObservableTestUtils.Elements<int>(args.NewItems));
    }

    [Fact]
    public void RemoveAtCarriesTheIndex()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.RemoveAt(1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
        Assert.Equal(1, args.OldStartingIndex);
        Assert.Equal([2], ObservableTestUtils.Elements<int>(args.OldItems));
    }

    [Fact]
    public void ReplaceCarriesTheIndex()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list[1] = 9;

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
        Assert.Equal(1, args.NewStartingIndex);
        Assert.Equal(1, args.OldStartingIndex);
        Assert.Equal([2], ObservableTestUtils.Elements<int>(args.OldItems));
        Assert.Equal([9], ObservableTestUtils.Elements<int>(args.NewItems));
    }

    [Fact]
    public void AddRangeRaisesOneAddPerItemWithAscendingIndexes()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.AddRange(new List<int> { 1, 2, 3 });

        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Add, e.Action));
        Assert.Equal([1, 2, 3], ObservableTestUtils.NewItemsOf<int>(events));
        Assert.Equal([0, 1, 2], events.Select(e => e.NewStartingIndex));
    }

    [Fact]
    public void RemoveRangeCarriesEachOriginalIndex()
    {
        NotifiableList<int> list = [1, 2, 3, 4];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.RemoveRange(1, 2);

        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action));
        Assert.Equal([2, 3], ObservableTestUtils.OldItemsOf<int>(events));
        Assert.Equal([1, 2], events.Select(e => e.OldStartingIndex));
    }

    [Fact]
    public void SortRaisesReset()
    {
        NotifiableList<int> list = [3, 1, 2];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Sort();

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
    }

    [Fact]
    public void ReverseRaisesReset()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Reverse();

        Assert.Equal(NotifyCollectionChangedAction.Reset, Assert.Single(events).Action);
    }

    [Fact]
    public void AddAndRemoveRaiseCountAndIndexerPropertyChanged()
    {
        NotifiableList<int> list = [1];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        list.Add(2);
        list.RemoveAt(0);

        Assert.Equal(["Count", "Item[]", "Count", "Item[]"], names);
    }

    [Fact]
    public void ReplaceRaisesIndexerPropertyChangedOnly()
    {
        NotifiableList<int> list = [1];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        list[0] = 2;

        Assert.Equal(["Item[]"], names);
    }

    [Fact]
    public void SortRaisesIndexerPropertyChangedOnly()
    {
        NotifiableList<int> list = [2, 1];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        list.Sort();

        Assert.Equal(["Item[]"], names);
    }

    [Fact]
    public void SenderIsTheViewItself()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        object? sender = null;
        view.CollectionChanged += (s, _) => sender = s;

        list.Add(1);

        Assert.Same(view, sender);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = new List<NotifyCollectionChangedEventArgs>();
        NotifyCollectionChangedEventHandler handler = (_, args) => events.Add(args);
        view.CollectionChanged += handler;
        list.Add(1);

        view.CollectionChanged -= handler;
        list.Add(2);

        Assert.Single(events);
    }

    [Fact]
    public void ViewStopsForwardingPropertyChangedAfterUnsubscribing()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = new List<string?>();
        PropertyChangedEventHandler handler = (_, args) => names.Add(args.PropertyName);
        view.PropertyChanged += handler;
        list.Add(1);

        view.PropertyChanged -= handler;
        list.Add(2);

        Assert.Equal(["Count", "Item[]"], names);
    }

    [Fact]
    public void ViewEnumeratesNonGenerically()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableList<int> source = list;
        var view = source.AsReadOnlyObservableCollection();

        Assert.Equal([1, 2, 3], ObservableTestUtils.Enumerate(view));
    }
}
