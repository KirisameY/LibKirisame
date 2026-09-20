using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

public class ListRemoveTests
{
    [Fact]
    public void RemoveReturnsFalseAndRaisesNothingWhenTheItemIsAbsent()
    {
        var list = new NotifiableList<int> { 1, 2 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.False(list.Remove(3));

        Assert.Empty(updates);
        Assert.Equal([1, 2], list);
    }

    [Fact]
    public void RemoveReportsTheIndexItWasAt()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.True(list.Remove(2));

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], removed.RemovedItems);
        Assert.Equal([1], removed.Indexes);
        Assert.Equal([1, 3], list);
    }

    [Fact]
    public void RemoveDropsOnlyTheFirstMatch()
    {
        var list = new NotifiableList<int> { 1, 2, 1 };

        list.Remove(1);

        Assert.Equal([2, 1], list);
    }

    [Fact]
    public void RemoveAtReportsTheIndex()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.RemoveAt(1);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], removed.RemovedItems);
        Assert.Equal([1], removed.Indexes);
        Assert.Equal([1, 3], list);
    }

    [Fact]
    public void RemoveRangeReportsContiguousAscendingIndexes()
    {
        var list = new NotifiableList<int> { 0, 1, 2, 3, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.RemoveRange(1, 3);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], removed.Indexes);
        Assert.Equal([1, 2, 3], removed.RemovedItems);
        Assert.Equal([0, 4], list);
    }

    [Fact]
    public void RemoveAllReportsAscendingIndexesAlignedWithTheRemovedItems()
    {
        var list = new NotifiableList<int> { 1, 2, 3, 4, 5, 6 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.RemoveAll(i => i % 2 == 0);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));

        // 约定：Indexes 严格升序，RemovedItems 与它逐位对应
        Assert.Equal([1, 3, 5], removed.Indexes);
        Assert.Equal([2, 4, 6], removed.RemovedItems);
        Assert.Equal([1, 3, 5], removed.RemovedItemsWithIndex.Select(i => i.Index));
        Assert.Equal([2, 4, 6], removed.RemovedItemsWithIndex.Select(i => i.Item));

        // 索引是移除前的口径，元素是移除后剩下的
        Assert.Equal([1, 3, 5], list);
    }

    [Fact]
    public void RemoveAllWithNoMatchStillRaisesAnEmptyChange()
    {
        var list = new NotifiableList<int> { 1, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.RemoveAll(i => i % 2 == 0);

        // 实现是无条件触发的，没命中也会收到一条空变更
        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(removed.RemovedItems);
        Assert.Empty(removed.Indexes);
        Assert.Equal([1, 3], list);
    }

    [Fact]
    public void ClearReportsTheWholeOldListWithContiguousIndexes()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Clear();

        var cleared = Assert.IsAssignableFrom<IListItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], cleared.RemovedItems);
        Assert.Equal([0, 1, 2], cleared.Indexes);
        Assert.Empty(list);
    }

    [Fact]
    public void ClearedEventArgsAreAlsoRemovedEventArgs()
    {
        var list = new NotifiableList<int> { 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Clear();

        // 清空是移除的特例：列表级和集合级都能按各自的移除接口接住
        var cleared = Assert.IsAssignableFrom<IListItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(cleared);
        Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<int>>(cleared);
    }
}
