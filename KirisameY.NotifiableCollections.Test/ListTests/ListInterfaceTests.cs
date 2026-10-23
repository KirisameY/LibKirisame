using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.Data;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="INotifiableList{T}"/> 上新提取或新桥接的成员：
///     显式只读标记、显式清空 / 按索引移除，以及直接在接口上调用的批量操作。
/// </summary>
public class ListInterfaceTests
{
    [Fact]
    public void IsReadOnlyIsFalseThroughICollection()
    {
        // IsReadOnly 已从默认实现挪到接口的显式默认实现，具体类型上不再直接暴露
        Assert.False(((ICollection<int>)new NotifiableList<int>()).IsReadOnly);
        Assert.False(((ICollection<int>)new MinimalNotifiableList<int>()).IsReadOnly);
    }

    [Fact]
    public void ClearThroughICollectionClearsAndRaisesOneClearedChange()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        ((ICollection<int>)list).Clear();

        Assert.Empty(list);

        var cleared = Assert.IsAssignableFrom<IListItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], cleared.RemovedItems);
        Assert.Equal([0, 1, 2], cleared.Indexes);
    }

    [Fact]
    public void RemoveAtThroughIListRemovesAndRaisesOneRemovedChange()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        ((IList<int>)list).RemoveAt(1);

        Assert.Equal([1, 3], list);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], removed.RemovedItems);
        Assert.Equal([1], removed.Indexes);
    }

    [Fact]
    public void AddRangeAndInsertRangeAreAvailableOnTheInterface()
    {
        INotifiableList<int> list = new NotifiableList<int> { 1, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.AddRange(new[] { 5 });
        list.InsertRange(1, new[] { 2, 3 });

        Assert.Equal([1, 2, 3, 4, 5], list);
        Assert.Equal(2, updates.Count);

        var inserted = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(updates[1]);
        Assert.Equal([2, 3], inserted.AddedItems);
        Assert.Equal([1, 2], inserted.Indexes);
    }

    [Fact]
    public void ClearOnTheInterfaceStillReturnsTheRemovedItems()
    {
        INotifiableList<int> list = new NotifiableList<int> { 1, 2 };

        Assert.Equal([1, 2], list.Clear());
        Assert.Empty(list);
    }

    [Fact]
    public void BatchedOperationsAreAvailableOnTheInterface()
    {
        INotifiableList<int> list = new NotifiableList<int> { 0, 1, 2, 3, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiInsert([10.WithIndex(1)]);
        list.MultiReplace([40.WithIndex(4)]);
        var removed = list.MultiRemove([1, 3]);

        Assert.Equal([0, 1, 40, 4], list);
        Assert.Equal([10, 2], removed);
        Assert.Equal(3, updates.Count);
    }
}
