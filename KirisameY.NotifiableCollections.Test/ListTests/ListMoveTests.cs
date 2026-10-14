using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="NotifiableList{T}"/> 上的移动：单项的 <c>Move</c>、整段挪窝的 <c>MoveRange</c>，
///     以及按一组步骤走的 <c>MultiMove</c>。
/// </summary>
/// <remarks>
///     <para>
///         这一族都发 <see cref="IListItemMovedEventArgs{T}"/>，事件里报的是一串「从哪个索引挪到哪个索引」，
///         但<b>拆成哪几步、按什么顺序报，都不算契约</b>——多步的挪法本来就是一种分解方式，换一种报法同样合法。
///         所以下面只钉「移动后的结果能对上」：把报出来的操作依序重放到操作前的列表上，
///         结果必须正好等于操作之后的列表（<see cref="NotifiableCollectionsTestUtils.ReplayMoves{T}"/>）。
///     </para>
///     <para>
///         另一条贯穿的约定是出错时<b>静默回退</b>：列表回到调用前的样子，一条通知都不发。
///         挪到原地、挪空区间之类的空操作则连「发不发通知」都不管——发不发都合法，因此一概不钉。
///     </para>
///     <para>
///         索引口径：本组的 <c>to</c> 都是「把这段拿出来之后，插到剩下那个列表的哪个索引上」，
///         和 <see cref="List{T}.InsertRange(int, IEnumerable{T})"/> 一致，所以 <c>to</c> 可以取到挪完之后的长度（即接到末尾），
///         也就是挪完之后这一段所在的起始位置。
///     </para>
/// </remarks>
public class ListMoveTests
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


    // ---------- Move ----------

    [Fact]
    public void MovePutsTheItemAtTheTargetIndex()
    {
        var list = Numbered(4);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Move(0, 2);

        Assert.Equal([1, 2, 0, 3], list);

        var moved = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 0, 3], moved.ListView);
    }

    [Fact]
    public void MoveBackwardsPutsTheItemInFrontOfItsTarget()
    {
        var list = Numbered(4);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Move(3, 1);

        // 目标索引是「拿走之后」的口径：项插在原来的索引 1 之前，于是 1、2 往后各让一位
        Assert.Equal([0, 3, 1, 2], list);
        Assert.Single(updates);
    }

    [Fact]
    public void MoveWithAnOutOfRangeSourceThrowsAndLeavesTheListAlone()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.ThrowsAny<ArgumentException>(() => list.Move(9, 0));

        Assert.Equal([0, 1, 2], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MoveWithAnOutOfRangeTargetPutsTheItemBack()
    {
        var list = Numbered(3);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 挪到不存在的索引：应当和 MoveRange / MultiMove 一样悄悄回退，列表保持原样
        Assert.ThrowsAny<ArgumentException>(() => list.Move(0, 99));

        Assert.Equal([0, 1, 2], list);
        Assert.Empty(updates);
    }


    // ---------- MoveRange ----------

    [Fact]
    public void MoveRangeForwardLandsTheBlockAtTheTargetIndex()
    {
        var list = Numbered(7);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MoveRange(1, 2, 4);

        // 目标索引是「拿走之后」的口径：拿走 1、2 之后 6 顶到了索引 4，于是这一段落在 6 前面
        Assert.Equal([0, 3, 4, 5, 1, 2, 6], list);
        Assert.Single(updates);
    }

    [Fact]
    public void MoveRangeBackwardLandsTheBlockAtTheTargetIndex()
    {
        var list = Numbered(7);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MoveRange(4, 2, 1);

        // 整段往前挪并不翻转段内顺序，挪完还是 4、5 挨着
        Assert.Equal([0, 4, 5, 1, 2, 3, 6], list);
        Assert.Single(updates);
    }

    [Theory]
    [InlineData(1, 2, 4)]
    [InlineData(4, 2, 1)]
    [InlineData(0, 3, 4)]
    [InlineData(5, 2, 0)]
    public void MoveRangeReportsMovesThatReplayToTheResult(int index, int count, int to)
    {
        var list = Numbered(7);
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MoveRange(index, count, to);

        var moved = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(list, NotifiableCollectionsTestUtils.ReplayMoves(original, moved));
        // 三串索引和项数逐位对齐，ItemMoves 才拼得出来
        Assert.Equal(moved.Items.Count, moved.OldIndexes.Count);
        Assert.Equal(moved.Items.Count, moved.NewIndexes.Count);
        Assert.Equal(moved.Items.Count, moved.ItemMoves.Count);
        Assert.Equal(list, moved.ListView);
    }

    [Fact]
    public void MoveRangeWithAnOutOfRangeTargetThrowsAndPutsTheBlockBack()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.ThrowsAny<ArgumentException>(() => list.MoveRange(1, 2, 99));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MoveRangeCanMoveABlockToTheEndOfTheList()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 拿走 3 个之后只剩 2 个，索引 2 正是末尾
        list.MoveRange(0, 3, 2);

        Assert.Equal([3, 4, 0, 1, 2], list);
        Assert.Single(updates);
    }


    // ---------- MultiMove ----------

    [Fact]
    public void MultiMoveAppliesEveryStepInOrder()
    {
        var list = Numbered(4);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiMove([(0, 2), (3, 0)]);

        Assert.Equal([3, 1, 2, 0], list);
        Assert.Single(updates);
    }

    [Fact]
    public void MultiMoveStepsAreMeasuredAgainstTheListAtThatMoment()
    {
        var list = Numbered(4);

        // 两遍一模一样的 (0, 2) 挪的是两个不同的项：第一遍挪完之后，原来索引 1 的项顶到了索引 0
        list.MultiMove([(0, 2), (0, 2)]);

        Assert.Equal([2, 0, 1, 3], list);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void MultiMoveReportsMovesThatReplayToTheResult(int count)
    {
        var steps = Enumerable.Range(0, count / 2).Select(i => (From: i, To: count - 1 - i)).ToArray();
        var list = Numbered(count);
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.MultiMove(steps);

        var moved = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(list, NotifiableCollectionsTestUtils.ReplayMoves(original, moved));
        // 三串索引和项数逐位对齐，ItemMoves 才拼得出来
        Assert.Equal(moved.Items.Count, moved.OldIndexes.Count);
        Assert.Equal(moved.Items.Count, moved.NewIndexes.Count);
        Assert.Equal(moved.Items.Count, moved.ItemMoves.Count);
    }

    [Fact]
    public void MultiMoveWithAnOutOfRangeSourceThrowsAndLeavesTheListAlone()
    {
        var list = Numbered(4);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.ThrowsAny<ArgumentException>(() => list.MultiMove([(2, 3), (9, 0)]));

        Assert.Equal([0, 1, 2, 3], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiMoveRollsBackTheStepsItAlreadyAppliedWhenALaterStepFails()
    {
        var list = Numbered(4);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 第一步挪完了，第二步的目标索引越界——已经挪过的要按原样放回去
        Assert.ThrowsAny<ArgumentException>(() => list.MultiMove([(0, 3), (1, 99)]));

        Assert.Equal([0, 1, 2, 3], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void MultiMoveRollsBackTheSourceIndexesItAlreadyVacated()
    {
        var list = Numbered(6);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 前两段都挪好了，第三段的来源索引越界，整批打回原形
        Assert.ThrowsAny<ArgumentException>(() => list.MultiMove([(0, 4), (1, 5), (9, 0)]));

        Assert.Equal([0, 1, 2, 3, 4, 5], list);
        Assert.Empty(updates);
    }
}
