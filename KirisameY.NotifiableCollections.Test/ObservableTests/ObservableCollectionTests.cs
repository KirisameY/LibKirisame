using System.Collections.Specialized;
using System.ComponentModel;

using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ObservableTests;

/// <summary>
///     集合版的 <c>AsReadOnlyObservableCollection</c> 产出的视图：
///     读取是否与源同步、通知是否被翻译成标准库模型。
/// </summary>
/// <remarks>
///     接收者刻意声明成 <see cref="IReadOnlyNotifiableCollection{T}"/> 而不是具体的
///     <see cref="NotifiableList{T}"/>：列表同时满足集合版与列表版两个扩展方法的重载，
///     声明成集合接口才能保证这里测的确实是集合版那个。
/// </remarks>
public class ObservableCollectionTests
{
    [Fact]
    public void ViewReflectsTheSourceContents()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableCollection<int> source = list;

        var view = source.AsReadOnlyObservableCollection();

        Assert.Equal([1, 2, 3], view);
        // Count 是独立于枚举的一条路径，单独确认一次（取到局部变量是为了绕开 xUnit2013 分析器）
        var count = view.Count;
        Assert.Equal(3, count);
    }

    [Fact]
    public void ViewIsLive()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;

        var view = source.AsReadOnlyObservableCollection();

        list.Add(1);

        Assert.Equal([1], view);
    }

    [Fact]
    public void AddRaisesASingleAddNotification()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Add(1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
        Assert.Equal([1], ObservableTestUtils.Elements<int>(args.NewItems));
    }

    [Fact]
    public void RemoveRaisesASingleRemoveNotification()
    {
        NotifiableList<int> list = [1, 2];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Remove(1);

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
        Assert.Equal([1], ObservableTestUtils.Elements<int>(args.OldItems));
    }

    [Fact]
    public void ReplaceRaisesASingleReplaceNotification()
    {
        NotifiableList<int> list = [1, 2];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list[0] = 9;

        var args = Assert.Single(events);
        Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
        Assert.Equal([1], ObservableTestUtils.Elements<int>(args.OldItems));
        Assert.Equal([9], ObservableTestUtils.Elements<int>(args.NewItems));
    }

    [Fact]
    public void ClearRaisesOneRemovePerItem()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var events = ObservableTestUtils.RecordCollectionChanged(view);

        list.Clear();

        Assert.All(events, e => Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action));
        Assert.Equal([1, 2, 3], ObservableTestUtils.OldItemsOf<int>(events));
    }

    [Fact]
    public void AddAndRemoveRaiseCountPropertyChanged()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        list.Add(1);
        list.Remove(1);

        Assert.Equal(["Count", "Count"], names);
    }

    [Fact]
    public void ReplaceRaisesNoPropertyChanged()
    {
        NotifiableList<int> list = [1];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = ObservableTestUtils.RecordPropertyChanged(view);

        list[0] = 2;

        // 集合版的 PropertyChanged 只关心 Count，替换不改数量所以不发
        Assert.Empty(names);
    }

    [Fact]
    public void SenderIsTheViewItself()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
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
        IReadOnlyNotifiableCollection<int> source = list;
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
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();
        var names = new List<string?>();
        PropertyChangedEventHandler handler = (_, args) => names.Add(args.PropertyName);
        view.PropertyChanged += handler;
        list.Add(1);

        view.PropertyChanged -= handler;
        list.Add(2);

        Assert.Single(names);
    }

    [Fact]
    public void ViewEnumeratesNonGenerically()
    {
        NotifiableList<int> list = [1, 2, 3];
        IReadOnlyNotifiableCollection<int> source = list;
        var view = source.AsReadOnlyObservableCollection();

        Assert.Equal([1, 2, 3], ObservableTestUtils.Enumerate(view));
    }
}
