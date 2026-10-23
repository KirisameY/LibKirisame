using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="NotifiableListSimpleBase{T}"/> 的默认实现：只实现四个抽象成员的子类
///     应当直接获得整套 <see cref="INotifiableList{T}"/> 行为。
/// </summary>
/// <remarks>
///     <see cref="NotifiableList{T}"/> 把所有 protected virtual 钩子都重写了，
///     所以这里用 <see cref="MinimalNotifiableList{T}"/> 专门验证那些钩子的默认实现。
/// </remarks>
public class ListSimpleBaseTests
{
    [Fact]
    public void MinimalSubclassOnlyNeedsTheFourAbstractMembers()
    {
        var list = new MinimalNotifiableList<int>();
        list.Add(1);
        list.Insert(1, 3);
        list.Insert(1, 2);

        Assert.Equal([1, 2, 3], list);
        Assert.Equal(3, list.Count);
        Assert.Contains(2, list);
        Assert.Equal(1, list.IndexOf(2));
        Assert.False(((ICollection<int>)list).IsReadOnly);
    }

    [Fact]
    public void AddThroughTheBaseReportsTheIndexItLandedOn()
    {
        var list = new MinimalNotifiableList<int> { 1, 2 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Add(3);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3], added.AddedItems);
        Assert.Equal([2], added.Indexes);
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void AddRangeThroughTheBaseAppendsEveryItem()
    {
        var list = new MinimalNotifiableList<int> { 1, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 显式传 ICollection，走基类的 AddRange，而不是 functional extensions 的 IEnumerable 重载
        list.AddRange(new[] { 2, 3 });

        Assert.Equal([1, 4, 2, 3], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal([2, 3], added.Indexes);
    }

    [Fact]
    public void InsertRangeThroughTheBaseUsesTheDefaultInsertItems()
    {
        var list = new MinimalNotifiableList<int> { 1, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.InsertRange(1, new[] { 2, 3 });

        Assert.Equal([1, 2, 3, 4], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal([1, 2], added.Indexes);
    }

    [Fact]
    public void ClearThroughTheBaseUsesTheDefaultClearItems()
    {
        var list = new MinimalNotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var removed = list.Clear();

        Assert.Equal([1, 2, 3], removed);
        Assert.Empty(list);

        var cleared = Assert.IsAssignableFrom<IListItemClearedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], cleared.RemovedItems);
        Assert.Equal([0, 1, 2], cleared.Indexes);
    }

    [Fact]
    public void RemoveRangeThroughTheBaseUsesTheDefaultGetItemsAndRemoveItems()
    {
        var list = new MinimalNotifiableList<int> { 0, 1, 2, 3, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var removed = list.RemoveRange(1, 3);

        Assert.Equal([1, 2, 3], removed);
        Assert.Equal([0, 4], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], removedArgs.RemovedItems);
        Assert.Equal([1, 2, 3], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeReturnsASnapshotFromTheDefaultGetItems()
    {
        var list = new MinimalNotifiableList<int> { 0, 1, 2, 3 };

        var removed = list.RemoveRange(1, 2);
        list.Add(9);

        Assert.Equal([1, 2], removed);
    }

    [Fact]
    public void SortThroughTheBaseUsesTheDefaultSortItems()
    {
        var list = new MinimalNotifiableList<int> { 3, 1, 2 };
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort();

        Assert.Equal([1, 2, 3], list);

        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], sorted.ListView);
        Assert.Equal([1, 2, 3], NotifiableCollectionsTestUtils.ReplayMoves(original, sorted));
    }

    [Fact]
    public void SortRangeThroughTheBaseOnlyReordersTheGivenRange()
    {
        var list = new MinimalNotifiableList<int> { 9, 3, 1, 2, 8 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // Sort(int, int) 是 functional extensions 提供的重载，最终会落到基类的 SortItems
        list.Sort(1, 3);

        Assert.Equal([9, 1, 2, 3, 8], list);

        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([9, 1, 2, 3, 8], sorted.ListView);
    }

    [Fact]
    public void ReverseThroughTheBaseReversesInsteadOfClearing()
    {
        var list = new MinimalNotifiableList<int> { 1, 2, 3 };
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Reverse();

        Assert.Equal([3, 2, 1], list);

        var reversed = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3, 2, 1], reversed.ListView);
        Assert.Equal([3, 2, 1], NotifiableCollectionsTestUtils.ReplayMoves(original, reversed));
    }

    [Fact]
    public void ReverseRangeThroughTheBaseUsesTheDefaultReverseItems()
    {
        var list = new MinimalNotifiableList<int> { 1, 2, 3, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Reverse(1, 2);

        Assert.Equal([1, 3, 2, 4], list);
        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
    }

    [Fact]
    public void MoveRangeThroughTheBaseUsesTheDefaultRangeHooks()
    {
        var list = new MinimalNotifiableList<int>();
        list.AddRange(Enumerable.Range(0, 7).ToArray());
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MoveRange(1, 2, 4);

        Assert.Equal([0, 3, 4, 5, 1, 2, 6], list);

        var moved = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(list, NotifiableCollectionsTestUtils.ReplayMoves(original, moved));
    }

    [Fact]
    public void MoveRangeThroughTheBaseRollsBackWhenTheTargetIsOutOfRange()
    {
        var list = new MinimalNotifiableList<int> { 0, 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.ThrowsAny<ArgumentException>(() => list.MoveRange(1, 2, 99));

        Assert.Equal([0, 1, 2, 3], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiRemoveThroughTheBaseReportsAscendingIndexesAlignedWithTheRemovedItems()
    {
        var list = new MinimalNotifiableList<int> { 0, 1, 2, 3, 4, 5 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var removed = list.MultiRemove([1, 3, 5]);

        Assert.Equal([1, 3, 5], removed);
        Assert.Equal([0, 2, 4], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3, 5], removedArgs.Indexes);
        Assert.Equal([1, 3, 5], removedArgs.RemovedItems);
    }
}
