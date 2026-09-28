using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.VanillaTests;

/// <summary>
///     <c>AsNotifiableCollection</c>：把标准库风格的 <see cref="INotifyCollectionChanged"/> 集合
///     包装成本库通知模型的只读视图。
/// </summary>
/// <remarks>
///     标准库的通知形状与本库的无序集合模型对不上的地方，这里都单独钉一条：
///     Reset 不带被清掉的元素、Move 无处安放、多项变更尽量并成一次通知。
/// </remarks>
public class VanillaCollectionTests
{
    [Fact]
    public void ViewReflectsTheSourceContents()
    {
        ObservableCollection<int> source = [1, 2, 3];

        var view = source.AsNotifiableCollection(TypeA.Of<int>());

        Assert.Equal([1, 2, 3], view);
        // Count 是独立于枚举的一条路径，单独确认一次（取到局部变量是为了绕开 xUnit2013 分析器）
        var count = view.Count;
        Assert.Equal(3, count);
    }

    [Fact]
    public void ViewIsLive()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());

        source.Add(1);
        Assert.Equal([1], view);

        source.Clear();
        Assert.Empty(view);
    }

    [Fact]
    public void AddIsReportedAsAnAddition()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.Add(7);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([7], added.AddedItems);
    }

    [Fact]
    public void ABatchAddIsReportedAsOneEvent()
    {
        var source = new RaisableObservableCollection<int>();
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.AddSilently(1);
        source.AddSilently(2);
        source.Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int> { 1, 2 }, 0));

        // 标准库允许一次通知携带多项，本库照单全收，不拆成多条
        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2], added.AddedItems.OrderBy(v => v));
        Assert.Equal([1, 2], view);
    }

    [Fact]
    public void ABatchRemoveIsReportedAsOneEvent()
    {
        var source = new RaisableObservableCollection<int> { 1, 2, 3 };
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.RemoveSilentlyAt(2);
        source.RemoveSilentlyAt(0);
        source.Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int> { 1, 3 }, 0));

        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3], removed.RemovedItems.OrderBy(v => v));
        Assert.Equal([2], view);
    }

    [Fact]
    public void ReplaceIsReportedWithTheOldAndNewItemsPairedUp()
    {
        ObservableCollection<int> source = [1, 2];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source[1] = 9;

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], replaced.OldItems);
        Assert.Equal([9], replaced.NewItems);

        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal(2, change.Old);
        Assert.Equal(9, change.New);
    }

    [Fact]
    public void ABatchReplaceIsReportedAsOneEventWithThePairsKeptTogether()
    {
        var source = new RaisableObservableCollection<int> { 1, 2 };
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.SetSilently(0, 9);
        source.SetSilently(1, 8);
        source.Raise(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace, new List<int> { 9, 8 }, new List<int> { 1, 2 })
        );

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        // 两批元素逐位配对，不会把新旧搞混
        Assert.Equal([(1, 9), (2, 8)], replaced.ItemChanges.Select(c => (c.Old, c.New)));
    }

    [Fact]
    public void ResetIsReportedAsResetWithoutTheClearedItems()
    {
        ObservableCollection<int> source = [1, 2];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.Clear();

        // 标准库的清空只发 Reset、不告诉你是谁被清掉了，所以这里拿不到本库自己清空时那种
        // 携带全部旧元素的 Cleared，只能是 Reset。
        var reset = Assert.IsAssignableFrom<ICollectionResetEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(reset.CollectionView);
        Assert.IsNotAssignableFrom<ICollectionItemRemovedEventArgs<int>>(reset);
    }

    [Fact]
    public void MoveIsNotRepresentableSoNothingIsRaised()
    {
        ObservableCollection<int> source = [1, 2, 3];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.Move(0, 2);

        // 无序模型连顺序都不保证，自然没有"移动"这一说，标准库的 Move 到这里就丢掉了
        Assert.Empty(updates);

        // 但视图是活视图，内容照样跟着变
        Assert.Equal([2, 3, 1], view);
    }

    [Fact]
    public void EventArgsPointAtTheViewItself()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        CollectionUpdateEventArgs<int>? captured = null;
        view.CollectionUpdated += (_, args) => captured = args;

        source.Add(1);

        Assert.Same(view, captured!.CollectionView);
    }

    [Fact]
    public void SenderIsTheViewItself()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        object? sender = null;
        view.CollectionUpdated += (s, _) => sender = s;

        source.Add(1);

        Assert.Same(view, sender);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        view.CollectionUpdated += handler;
        source.Add(1);

        view.CollectionUpdated -= handler;
        source.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void UnsubscribingAnUnknownHandlerIsIgnored()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);
        EventHandler<CollectionUpdateEventArgs<int>> unknown = (_, _) => { };
        source.Add(1);

        var exception = Record.Exception(() => view.CollectionUpdated -= unknown);
        source.Add(2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void SameHandlerSubscribedTwiceIsCountedSeparately()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        view.CollectionUpdated += handler;
        view.CollectionUpdated += handler;

        source.Add(1);
        Assert.Equal(2, count);

        // 退订一次只摘掉一份，另一份还在
        view.CollectionUpdated -= handler;
        source.Add(2);
        Assert.Equal(3, count);
    }

    [Fact]
    public void SourcesThatAreOnlyCollectionsAreSupported()
    {
        // 集合版包装只要求「能数、能枚举」，不需要索引，所以非列表的源也该能用
        var source = new FakeNotifyCollection<string>();
        var view = source.AsNotifiableCollection(TypeA.Of<string>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.Add("a");

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<string>>(Assert.Single(updates));
        Assert.Equal(["a"], added.AddedItems);
        Assert.Equal(["a"], view);
    }

    [Fact]
    public void NonGenericEnumerationAlsoYieldsTheElements()
    {
        ObservableCollection<int> source = [1, 2, 3];
        var view = source.AsNotifiableCollection(TypeA.Of<int>());

        Assert.Equal([1, 2, 3], ((IEnumerable)view).Cast<int>());
    }
}
