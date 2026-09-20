using System.Collections;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

public class CollectionViewTests
{
    [Fact]
    public void ViewReflectsTheSourceContents()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        var view = list.AsReadOnlyNotifiableCollection();

        Assert.Equal(3, view.Count);
        Assert.Equal([1, 2, 3], view);
    }

    [Fact]
    public void ViewIsLive()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableCollection();

        list.Add(1);

        Assert.Equal([1], view);
    }

    [Fact]
    public void ViewForwardsNotificationsAndSendsItselfAsSender()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableCollection();
        var updates = new List<CollectionUpdateEventArgs<int>>();
        object? sender = null;
        view.CollectionUpdated += (s, args) =>
        {
            sender = s;
            updates.Add(args);
        };

        list.Add(1);

        Assert.Single(updates);
        Assert.Same(view, sender);
    }

    [Fact]
    public void ViewStopsForwardingAfterUnsubscribing()
    {
        var list = new NotifiableList<int>();
        var view = list.AsReadOnlyNotifiableCollection();
        var count = 0;
        EventHandler<CollectionUpdateEventArgs<int>> handler = (_, _) => count++;
        view.CollectionUpdated += handler;
        list.Add(1);

        view.CollectionUpdated -= handler;
        list.Add(2);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProjectionViewProjectsEveryElement()
    {
        var list = new NotifiableList<string> { "a", "bb" };

        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);

        Assert.Equal(2, view.Count);
        Assert.Equal([1, 2], view);
    }

    [Fact]
    public void ProjectionIsAppliedOnEachReadAndNotCached()
    {
        var list = new NotifiableList<int> { 1 };
        var calls = 0;
        var view = list.AsReadOnlyNotifiableCollection(v =>
        {
            calls++;
            return v * 2;
        });

        Assert.Equal([2], view);
        Assert.Equal(1, calls);

        Assert.Equal([2], view);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void ProjectionViewMapsTheItemsCarriedByNotifications()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.Add("abc");

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3], added.AddedItems);
    }

    [Fact]
    public void UpcastViewUsesTheDummyTypeArgument()
    {
        var list = new NotifiableList<string> { "a", "b" };

        // TypeA 那个干参数只是为了少写一个类型参数，值本身不参与运算
        IReadOnlyNotifiableCollection<object> view = list.AsReadOnlyNotifiableCollection(TypeA.Of<object>());

        Assert.Equal(2, view.Count);
        Assert.Equal(["a", "b"], view);
    }

    [Fact]
    public void UpcastViewForwardsNotificationsAsTheWidenedType()
    {
        var list = new NotifiableList<string>();
        var view = list.AsReadOnlyNotifiableCollection(TypeA.Of<object>());
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.Add("a");

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<object>>(Assert.Single(updates));
        Assert.Equal(["a"], added.AddedItems);
    }

    [Fact]
    public void ViewsCanBeStacked()
    {
        var list = new NotifiableList<string> { "a" };
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length).AsReadOnlyNotifiableCollection(v => v * 10);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.Add("ab");

        Assert.Equal([10, 20], view);
        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([20], added.AddedItems);
    }

    [Fact]
    public void NonGenericEnumerationAlsoYieldsProjectedElements()
    {
        var list = new NotifiableList<string> { "a", "bb" };
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);

        // 非泛型枚举也要走投影，而不是把源元素原样吐出来
        Assert.Equal([1, 2], ((IEnumerable)view).Cast<int>());
    }

    [Fact]
    public void ProjectionViewMapsClearedItems()
    {
        var list = new NotifiableList<string> { "a", "bb" };
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.Clear();

        var cleared = Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2], cleared.RemovedItems);
    }
}
