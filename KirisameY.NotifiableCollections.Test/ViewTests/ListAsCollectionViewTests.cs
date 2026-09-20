using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

/// <summary>
///     把列表当成更基础的「集合」包装起来时，事件要跟着降级：
///     集合层没有对应语义的事件（ListSorted）不该继续派发，也不该抛异常；
///     其余事件要换成只实现集合级接口的参数，不能把列表级的接口漏出去。
///     原样包装和投影包装两条路都得满足。
/// </summary>
public class ListAsCollectionViewTests
{
    /// <summary>
    ///     <paramref name="projecting"/> 为 <see langword="false"/> 走原样包装，
    ///     为 <see langword="true"/> 走元素投影包装（投影函数取恒等，好让两条路元素类型一致）。
    /// </summary>
    private static IReadOnlyNotifiableCollection<int> AsCollection(NotifiableList<int> list, bool projecting) =>
        projecting
            ? list.AsReadOnlyNotifiableCollection(v => v)
            : list.AsReadOnlyNotifiableCollection();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SortIsNotDispatchedThroughTheCollectionView(bool projecting)
    {
        var list = new NotifiableList<int> { 3, 1, 2 };
        var view = AsCollection(list, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        // 排序在集合层没有对应的事件，应该直接不发，而不是抛异常
        var exception = Record.Exception(() => list.Sort());

        Assert.Null(exception);
        Assert.Empty(updates);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReverseIsNotDispatchedThroughTheCollectionView(bool projecting)
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var view = AsCollection(list, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        var exception = Record.Exception(() => list.Reverse());

        Assert.Null(exception);
        Assert.Empty(updates);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var list = new NotifiableList<int>();
        var view = AsCollection(list, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.Add(1);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IListItemAddedEventArgs<int>>(added);
        Assert.IsNotAssignableFrom<IListUpdateEventArgs<int>>(added);
        Assert.Equal([1], added.AddedItems);
        Assert.Same(view, added.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemovedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var list = new NotifiableList<int> { 1, 2 };
        var view = AsCollection(list, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.RemoveAt(0);

        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IListItemRemovedEventArgs<int>>(removed);
        Assert.IsNotAssignableFrom<IListUpdateEventArgs<int>>(removed);
        Assert.Equal([1], removed.RemovedItems);
        Assert.Same(view, removed.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var list = new NotifiableList<int> { 1, 2 };
        var view = AsCollection(list, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list.Clear();

        var cleared = Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IListItemClearedEventArgs<int>>(cleared);
        Assert.IsNotAssignableFrom<IListItemRemovedEventArgs<int>>(cleared);
        Assert.Equal([1, 2], cleared.RemovedItems);
        Assert.Same(view, cleared.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReplacedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var list = new NotifiableList<int> { 1 };
        var view = AsCollection(list, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list[0] = 2;

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IListItemReplacedEventArgs<int>>(replaced);
        Assert.IsNotAssignableFrom<IListUpdateEventArgs<int>>(replaced);
        Assert.Equal([1], replaced.OldItems);
        Assert.Equal([2], replaced.NewItems);
        Assert.Same(view, replaced.CollectionView);
    }

    // 下面两条是反向对照：包装成列表时不该降级，同一个事件照常带列表级的接口和索引信息

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WrappingAsListKeepsTheListLevelEventArgs(bool projecting)
    {
        var list = new NotifiableList<int>();
        var view = projecting
            ? list.AsReadOnlyNotifiableList(v => v)
            : list.AsReadOnlyNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.Add(1);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], added.AddedItems);
        Assert.Equal(0, added.StartIndex);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SortIsStillDispatchedThroughTheListView(bool projecting)
    {
        var list = new NotifiableList<int> { 2, 1 };
        var view = projecting
            ? list.AsReadOnlyNotifiableList(v => v)
            : list.AsReadOnlyNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list.Sort();

        // 同一个事件：走列表包装器照常派发，走集合包装器则被挡掉
        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2], sorted.ListView);
    }
}
