using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.VanillaTests;

/// <summary>
///     <c>AsNotifiableList</c>：把标准库风格的 <see cref="INotifyCollectionChanged"/> 列表
///     包装成本库通知模型的只读列表视图。
/// </summary>
/// <remarks>
///     比集合版多一层工作：把标准库那一对索引还原成本库要的索引信息——
///     添加给一个起始索引、移除与替换给一串索引、移动给一对新旧索引。
/// </remarks>
public class VanillaListTests
{
    [Fact]
    public void ViewReflectsTheSourceContentsAndIndexer()
    {
        ObservableCollection<int> source = [1, 2, 3];

        var view = source.AsNotifiableList(TypeA.Of<int>());

        Assert.Equal([1, 2, 3], view);
        Assert.Equal(2, view[1]);
        // Count 是独立于枚举与索引器的一条路径，单独确认一次（取到局部变量是为了绕开 xUnit2013 分析器）
        var count = view.Count;
        Assert.Equal(3, count);
    }

    [Fact]
    public void ViewIsLive()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableList(TypeA.Of<int>());

        source.Add(1);
        Assert.Equal([1], view);

        source.Clear();
        Assert.Empty(view);
    }

    [Fact]
    public void InsertCarriesTheStartIndex()
    {
        ObservableCollection<int> source = [1, 3];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source.Insert(1, 2);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], added.AddedItems);
        Assert.Equal(1, added.StartIndex);
        Assert.Equal([(1, 2)], added.AddedItemsWithIndex.Select(i => (i.Index, i.Item)));
    }

    [Fact]
    public void ABatchAddSharesOneStartIndex()
    {
        var source = new RaisableObservableCollection<int> { 1 };
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source.AddSilently(2);
        source.AddSilently(3);
        source.Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<int> { 2, 3 }, 1));

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        // 批量添加插的是连续一段，一个起点就够，配对用的索引由库自己推出来
        Assert.Equal(1, added.StartIndex);
        Assert.Equal([(1, 2), (2, 3)], added.AddedItemsWithIndex.Select(i => (i.Index, i.Item)));
    }

    [Fact]
    public void RemoveAtCarriesTheIndex()
    {
        ObservableCollection<int> source = [1, 2, 3];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source.RemoveAt(1);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], removed.RemovedItems);
        Assert.Equal([1], removed.Indexes);
    }

    [Fact]
    public void ABatchRemoveCarriesEachOriginalIndex()
    {
        var source = new RaisableObservableCollection<int> { 1, 2, 3, 4 };
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source.RemoveSilentlyAt(2);
        source.RemoveSilentlyAt(1);
        source.Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<int> { 2, 3 }, 1));

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], removed.RemovedItems);
        // 标准库只给一个起点，索引是按「起点 + 项数」推出来的连续升序
        Assert.Equal([1, 2], removed.Indexes);
        Assert.Equal([(1, 2), (2, 3)], removed.RemovedItemsWithIndex.Select(i => (i.Index, i.Item)));
        Assert.Equal([1, 4], view);
    }

    [Fact]
    public void ReplaceCarriesTheIndexes()
    {
        ObservableCollection<int> source = [1, 2, 3];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source[1] = 9;

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], replaced.OldItems);
        Assert.Equal([9], replaced.NewItems);
        Assert.Equal([1], replaced.Indexes);
        Assert.Equal([(1, 2, 9)], replaced.ItemChanges.Select(c => (c.Index, c.Old, c.New)));
    }

    [Fact]
    public void MoveCarriesTheOldAndNewIndex()
    {
        ObservableCollection<int> source = [1, 2, 3];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source.Move(0, 2);

        var moved = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], moved.Items);
        Assert.Equal([0], moved.OldIndexes);
        Assert.Equal([2], moved.NewIndexes);
        Assert.Equal([(1, 0, 2)], moved.ItemMoves.Select(m => (m.Item, m.OldIndex, m.NewIndex)));
        Assert.Equal([2, 3, 1], view);
    }

    [Fact]
    public void MoveCarryingSeveralItemsIsDropped()
    {
        var source = new RaisableObservableCollection<int> { 1, 2, 3 };
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        // 标准库用一对索引描述整批被移动的项，还原不出每一项各自的新旧位置，所以这种形状目前整条丢掉。
        // （ObservableCollection 自己一次只移动一项，单项那条路是通的，见上一条。）
        source.Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new List<int> { 2, 3 }, 2, 1));

        Assert.Empty(updates);
    }

    [Fact]
    public void ResetIsReportedAsResetWithoutTheClearedItems()
    {
        ObservableCollection<int> source = [1, 2];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        source.Clear();

        var reset = Assert.IsAssignableFrom<IListResetEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(reset.ListView);
        Assert.IsNotAssignableFrom<IListItemRemovedEventArgs<int>>(reset);
    }

    [Fact]
    public void EventArgsPointAtTheViewItself()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        ListUpdateEventArgs<int>? captured = null;
        view.ListUpdated += (_, args) => captured = args;

        source.Add(1);

        Assert.Same(view, captured!.ListView);
        // 列表视图本身也是集合视图，两者指向同一个对象
        Assert.Same(view, captured.CollectionView);
    }

    [Fact]
    public void SenderIsTheViewItself()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        object? sender = null;
        view.ListUpdated += (s, _) => sender = s;

        source.Add(1);

        Assert.Same(view, sender);
    }

    [Fact]
    public void ListNotificationsAlsoReachTheCollectionLevelSubscription()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        // 列表版的事件参数本身就实现了集合版接口，所以按集合订阅也照样收到
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        source.Add(1);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], added.AddedItems);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var count = 0;
        EventHandler<ListUpdateEventArgs<int>> handler = (_, _) => count++;
        view.ListUpdated += handler;
        source.Add(1);

        view.ListUpdated -= handler;
        source.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void UnsubscribingAnUnknownHandlerIsIgnored()
    {
        ObservableCollection<int> source = [];
        var view = source.AsNotifiableList(TypeA.Of<int>());
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);
        EventHandler<ListUpdateEventArgs<int>> unknown = (_, _) => { };
        source.Add(1);

        var exception = Record.Exception(() => view.ListUpdated -= unknown);
        source.Add(2);

        Assert.Null(exception);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void NonGenericEnumerationAlsoYieldsTheElements()
    {
        ObservableCollection<int> source = [1, 2, 3];
        var view = source.AsNotifiableList(TypeA.Of<int>());

        Assert.Equal([1, 2, 3], ((IEnumerable)view).Cast<int>());
    }
}
