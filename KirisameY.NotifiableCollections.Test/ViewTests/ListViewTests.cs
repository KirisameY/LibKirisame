using System.Collections;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

public class ListViewTests
{
    [Fact]
    public void ViewIsItselfAReadOnlyList()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        var view = list.AsReadOnlyNotifiableList();

        Assert.Equal(3, view.Count);
        Assert.Equal(2, view[1]);
        Assert.Equal([1, 2, 3], view);
    }

    [Fact]
    public void ViewIsLive()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();

        list.Add(1);

        Assert.Equal([1], view);
    }

    [Fact]
    public void ViewForwardsIndexInformationUnchanged()
    {
        var list = new NotifiableList<int> { 1, 2, 3, 4 };
        var view = list.AsReadOnlyNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.RemoveAll(i => i % 2 == 1);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([0, 2], removed.Indexes);
        Assert.Equal([1, 3], removed.RemovedItems);
    }

    [Fact]
    public void ViewForwardsSortedNotifications()
    {
        var list = new NotifiableList<int> { 2, 1 };
        var view = list.AsReadOnlyNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.Sort();

        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2], sorted.ListView);
    }

    [Fact]
    public void ViewSendsItselfAsSender()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();
        object? sender = null;
        view.ListUpdated += (s, _) => sender = s;

        list.Add(1);

        Assert.Same(view, sender);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();
        var count = 0;
        EventHandler<ListUpdateEventArgs<int>> handler = (_, _) => count++;
        view.ListUpdated += handler;
        list.Add(1);

        view.ListUpdated -= handler;
        list.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProjectionViewProjectsEveryElement()
    {
        var list = new NotifiableList<string> { "a", "bb" };

        var view = list.AsReadOnlyNotifiableList(s => s.Length);

        Assert.Equal([1, 2], view);
    }

    [Fact]
    public void ProjectionViewMapsAddedItemsAndKeepsTheStartIndex()
    {
        var list = new NotifiableList<string> { "a", "bb" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.Add("ccc");

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3], added.AddedItems);

        // 索引是位置信息、与元素类型无关，投影后原样透传
        Assert.Equal(2, added.StartIndex);
    }

    [Fact]
    public void ProjectionViewMapsReplacedItems()
    {
        var list = new NotifiableList<string> { "a" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list[0] = "bbb";

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([0], replaced.Indexes);
        Assert.Equal([1], replaced.OldItems);
        Assert.Equal([3], replaced.NewItems);
    }

    [Fact]
    public void ProjectionViewMapsClearedItems()
    {
        var list = new NotifiableList<string> { "a", "bb" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.Clear();

        var cleared = Assert.IsAssignableFrom<IListItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([0, 1], cleared.Indexes);
        Assert.Equal([1, 2], cleared.RemovedItems);
    }

    [Fact]
    public void ProjectionViewMapsSortedNotifications()
    {
        var list = new NotifiableList<int> { 2, 1 };
        var view = list.AsReadOnlyNotifiableList(v => v * 10);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.Sort();

        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 20], sorted.ListView);
    }

    [Fact]
    public void UpcastViewUsesTheDummyTypeArgument()
    {
        var list = new NotifiableList<string> { "a" };

        IReadOnlyNotifiableList<object> view = list.AsReadOnlyNotifiableList(TypeA.Of<object>());

        Assert.Equal(["a"], view);
    }

    [Fact]
    public void CollectionLevelSubscriptionOnTheViewAlsoFires()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates<int>(view);

        list.Add(1);

        Assert.Single(updates);
    }

    [Fact]
    public void NonGenericEnumerationAlsoYieldsProjectedElements()
    {
        var list = new NotifiableList<string> { "a", "bb" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);

        // 非泛型枚举也要走投影，而不是把源元素原样吐出来
        Assert.Equal([1, 2], ((IEnumerable)view).Cast<int>());
    }

    [Fact]
    public void SameHandlerSubscribedTwiceIsCountedSeparately()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableList();
        var count = 0;
        EventHandler<ListUpdateEventArgs<int>> handler = (_, _) => count++;
        view.ListUpdated += handler;
        view.ListUpdated += handler;

        list.Add(1);
        Assert.Equal(2, count);

        // 退订一次只摘掉一份，另一份还在
        view.ListUpdated -= handler;
        list.Add(2);
        Assert.Equal(3, count);

        view.ListUpdated -= handler;
        list.Add(3);
        Assert.Equal(3, count);
    }
}
