using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.Data;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="NotifiableList{T}"/> 上批量操作的语义：
///     <c>MultiInsert</c> / <c>MultiRemove</c> / <c>MultiReplace</c>，
///     以及改成返回被移除项的 <c>RemoveRange</c>。
/// </summary>
/// <remarks>
///     这批操作都以「一批带索引的项」为单位收发，索引要求严格升序、不重复。
///     所以下面大多在钉三件事：索引是怎么解释的、发了几次带了什么通知、抛异常时列表有没有整批退回原样。
///     <br/>
///     抛异常的那几条一律要求<b>静默回退</b>：列表回到调用前的样子，并且一条通知都不发。
///     <br/>
///     异常类型分两档：索引顺序不对、索引重复是库自己抛的 <see cref="ArgumentException"/>，按精确类型钉住；
///     索引越界则是内层 <see cref="List{T}"/> 抛的 <see cref="ArgumentOutOfRangeException"/>
///     （<see cref="ArgumentException"/> 的派生类型），文档只承诺到基类型，所以那几条按 <c>ThrowsAny</c> 接。
/// </remarks>
public class ListBatchChangeTests
{
    /// <summary>
    ///     造一个内容为 <c>0..count-1</c> 的列表：元素值和它的下标一致，一眼能看出谁被挪到了哪。
    /// </summary>
    private static NotifiableList<int> Numbered(int count)
    {
        var list = new NotifiableList<int>();
        list.AddRange(Enumerable.Range(0, count));
        return list;
    }

    /// <summary>
    ///     把「索引 + 值」的元组包成 <see cref="IItemWithIndex{T}"/> 列表。
    /// </summary>
    private static IReadOnlyList<IItemWithIndex<T>> Pairs<T>(params (int Index, T Item)[] pairs) =>
        [.. pairs.Select(p => ItemWithIndex.From(p.Index, p.Item))];

    /// <summary>
    ///     同上，但给的是惰性序列，用来走 <see cref="IEnumerable{T}"/> 那个重载。
    /// </summary>
    private static IEnumerable<IItemWithIndex<T>> LazyPairs<T>(params (int Index, T Item)[] pairs) =>
        pairs.Select(p => ItemWithIndex.From(p.Index, p.Item));


    // ---------- MultiInsert ----------

    [Fact]
    public void MultiInsertPutsEveryItemAtTheIndexItWasGiven()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiInsert(Pairs((1, 10), (3, 30)));

        // 索引按「最终布局」解释：插进去的项最后就落在它自己那个索引上
        Assert.Equal([0, 10, 1, 30, 2, 3, 4], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 30], added.AddedItems);
        Assert.Equal([1, 3], added.Indexes);
        Assert.Equal([1, 3], added.AddedItemsWithIndex.Select(i => i.Index));
        Assert.Equal([10, 30], added.AddedItemsWithIndex.Select(i => i.Item));
        Assert.Equal([0, 10, 1, 30, 2, 3, 4], added.ListView);
    }

    [Fact]
    public void MultiInsertFromEnumerableBehavesTheSame()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiInsert(LazyPairs((1, 10), (3, 30)));

        Assert.Equal([0, 10, 1, 30, 2, 3, 4], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 30], added.AddedItems);
        Assert.Equal([1, 3], added.Indexes);
    }

    [Fact]
    public void MultiInsertAcceptsTheChainedWithIndexForm()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        IReadOnlyList<IItemWithIndex<int>> items = [10.WithIndex(1), 30.WithIndex(3)];
        list.MultiInsert(items);

        Assert.Equal([0, 10, 1, 30, 2, 3, 4], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 30], added.AddedItems);
        Assert.Equal([1, 3], added.Indexes);
    }

    [Fact]
    public void MultiInsertFromSeparateIndexesAndItemsPairsThemPositionally()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiInsert(new[] { 1, 3 }, new[] { 10, 30 });

        Assert.Equal([0, 10, 1, 30, 2, 3, 4], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 30], added.AddedItems);
        Assert.Equal([1, 3], added.Indexes);
    }

    [Fact]
    public void MultiInsertThrowsWhenTheTwoSequencesHaveDifferentLengths()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 两个序列按位配对，数量对不上就整批拒绝，不做「有多少配多少」
        Assert.Throws<ArgumentException>(() => list.MultiInsert(new[] { 1, 2 }, new[] { 10 }));
        Assert.Throws<ArgumentException>(() => list.MultiInsert(new[] { 1 }, new[] { 10, 30 }));

        Assert.Equal([0, 1, 2], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiInsertFromTwoEmptySequencesIsAccepted()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 两边都是空的算「配得上」，走和空入参一样的那条空变更路径
        list.MultiInsert(Array.Empty<int>(), Array.Empty<int>());

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(added.AddedItems);
        Assert.Equal([0, 1, 2], list);
    }

    [Fact]
    public void MultiInsertWithNoItemsStillRaisesAnEmptyChange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 和 AddRange / RemoveAll 一样，这边是无条件触发的，没东西可加也会收到一条空变更
        list.MultiInsert(Pairs<int>());

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(added.AddedItems);
        Assert.Empty(added.Indexes);
        Assert.Equal([0, 1, 2], list);
    }

    [Fact]
    public void MultiInsertThrowsForIndexesThatAreNotAscendingAndLeavesTheListAlone()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiInsert(Pairs((1, 10), (4, 40), (2, 20))));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiInsertThrowsForDuplicatedIndexesAndLeavesTheListAlone()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiInsert(Pairs((2, 20), (2, 21))));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiInsertRollsBackWhatItAlreadyInsertedWhenAnIndexIsOutOfRange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 前两项插进去了，第三项越界——已经插进去的要按原样撤回来
        Assert.ThrowsAny<ArgumentException>(() => list.MultiInsert(Pairs((1, 10), (2, 20), (9, 90))));

        Assert.Equal([0, 1, 2], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiInsertRollsBackEverythingWhenTheFirstIndexIsOutOfRange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.ThrowsAny<ArgumentException>(() => list.MultiInsert(Pairs((9, 90))));

        Assert.Equal([0, 1, 2], list);
        Assert.Empty(updates);
    }


    // ---------- MultiRemove ----------

    [Fact]
    public void MultiRemoveDropsEveryGivenIndexAndHandsBackTheirValues()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.MultiRemove([1, 3]);

        Assert.Equal([0, 2, 4, 5], list);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3], removed.Indexes);
        Assert.Equal([1, 3], removed.RemovedItems);
        Assert.Equal([1, 3], removed.RemovedItemsWithIndex.Select(i => i.Index));
        Assert.Equal([1, 3], removed.RemovedItemsWithIndex.Select(i => i.Item));

        // 返回值与入参索引逐位对齐
        Assert.Equal([1, 3], returned);
    }

    [Fact]
    public void MultiRemoveReportsIndexesInThePreRemovalScale()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiRemove([0, 5]);

        // 索引是移除前的口径：报的是 5，而不是移除后它所在的位置
        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([0, 5], removed.Indexes);
        Assert.Equal([0, 5], removed.RemovedItems);
        Assert.Equal([1, 2, 3, 4], list);
    }

    [Fact]
    public void MultiRemoveWithNoIndexesStillRaisesAnEmptyChange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.MultiRemove([]);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(removed.RemovedItems);
        Assert.Empty(removed.Indexes);
        Assert.Empty(returned);
        Assert.Equal([0, 1, 2], list);
    }

    [Fact]
    public void MultiRemoveThrowsForIndexesThatAreNotAscendingAndLeavesTheListAlone()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiRemove([3, 1]));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiRemoveThrowsForDuplicatedIndexesAndLeavesTheListAlone()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiRemove([2, 2]));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiRemovePutsBackTheHigherIndexesItAlreadyDropped()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 倒着走：先把索引 2 删掉，再到索引 1 时才发现它比前一个索引大——此时必须把 2 放回去
        Assert.Throws<ArgumentException>(() => list.MultiRemove([3, 1, 2]));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiRemovePutsBackTheHigherIndexesWhenALowerIndexIsOutOfRange()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 索引 3 先被删掉，然后负数索引把整个批次打回原形
        Assert.ThrowsAny<ArgumentException>(() => list.MultiRemove([-1, 3]));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }


    // ---------- MultiReplace ----------

    [Fact]
    public void MultiReplaceSwapsEveryGivenIndexAndHandsBackTheOldValues()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.MultiReplace(Pairs((1, 10), (3, 30)));

        Assert.Equal([0, 10, 2, 30, 4], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3], replaced.Indexes);
        Assert.Equal([1, 3], replaced.OldItems);
        Assert.Equal([10, 30], replaced.NewItems);
        Assert.Equal([0, 10, 2, 30, 4], replaced.ListView);

        // 返回值与入参逐位对齐，是替换前的旧值
        Assert.Equal([1, 3], returned);
    }

    [Fact]
    public void MultiReplacePairsIndexWithOldAndNewInItemChanges()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiReplace(Pairs((1, 10), (3, 30)));

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Collection(
            replaced.ItemChanges,
            change =>
            {
                Assert.Equal(1, change.Index);
                Assert.Equal(1, change.Old);
                Assert.Equal(10, change.New);
            },
            change =>
            {
                Assert.Equal(3, change.Index);
                Assert.Equal(3, change.Old);
                Assert.Equal(30, change.New);
            }
        );
    }

    [Fact]
    public void MultiReplaceFromEnumerableBehavesTheSame()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.MultiReplace(LazyPairs((1, 10), (3, 30)));

        Assert.Equal([0, 10, 2, 30, 4], list);
        Assert.Equal([1, 3], returned);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3], replaced.OldItems);
        Assert.Equal([10, 30], replaced.NewItems);
    }

    [Fact]
    public void MultiReplaceFromSeparateIndexesAndItemsPairsThemPositionally()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.MultiReplace(new[] { 1, 3 }, new[] { 10, 30 });

        Assert.Equal([0, 10, 2, 30, 4], list);
        Assert.Equal([1, 3], returned);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3], replaced.OldItems);
        Assert.Equal([10, 30], replaced.NewItems);
    }

    [Fact]
    public void MultiReplaceThrowsWhenTheTwoSequencesHaveDifferentLengths()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiReplace(new[] { 1, 3 }, new[] { 10 }));
        Assert.Throws<ArgumentException>(() => list.MultiReplace(new[] { 1 }, new[] { 10, 30 }));

        Assert.Equal([0, 1, 2, 3, 4], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiReplaceWithNoItemsStillRaisesAnEmptyChange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.MultiReplace(Pairs<int>());

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(replaced.OldItems);
        Assert.Empty(replaced.NewItems);
        Assert.Empty(replaced.Indexes);
        Assert.Empty(returned);
        Assert.Equal([0, 1, 2], list);
    }

    [Fact]
    public void MultiReplaceThrowsForIndexesThatAreNotAscendingAndPutsTheOldValuesBack()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiReplace(Pairs((1, 10), (4, 40), (2, 20))));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiReplaceThrowsForDuplicatedIndexesAndLeavesTheListAlone()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.Throws<ArgumentException>(() => list.MultiReplace(Pairs((2, 20), (2, 21))));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiReplaceRollsBackWhatItAlreadySwappedWhenAnIndexIsOutOfRange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.ThrowsAny<ArgumentException>(() => list.MultiReplace(Pairs((1, 10), (2, 20), (9, 90))));

        Assert.Equal([0, 1, 2], list);
        Assert.Empty(updates);
    }


    // ---------- RemoveRange ----------

    [Fact]
    public void RemoveRangeHandsBackTheItemsItRemovedAlignedWithTheReportedIndexes()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.RemoveRange(1, 3);

        Assert.Equal([0, 4, 5], list);
        Assert.Equal([1, 2, 3], returned);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], removed.Indexes);
        Assert.Equal([1, 2, 3], removed.RemovedItems);
        Assert.Equal(returned, removed.RemovedItems);
    }

    [Fact]
    public void RemoveRangeWithZeroCountRemovesNothingButStillRaisesAnEmptyChange()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.RemoveRange(1, 0);

        Assert.Empty(returned);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(removed.RemovedItems);
        Assert.Empty(removed.Indexes);
        Assert.Equal([0, 1, 2], list);
    }

    [Fact]
    public void RemoveRangeHandsBackASnapshotThatSurvivesLaterChanges()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var returned = list.RemoveRange(1, 2);

        // 返回值是切下来的快照而不是活视图，之后列表再变也与它无关
        list.Add(99);

        Assert.Equal([1, 2], returned);

        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(updates[0]);
        Assert.Equal([1, 2], removed.RemovedItems);
    }
}
