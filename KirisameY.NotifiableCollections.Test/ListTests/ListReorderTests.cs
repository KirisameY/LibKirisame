using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="NotifiableList{T}"/> 的整理与反转：四个 <c>Sort</c> 重载和两个 <c>Reverse</c> 重载。
/// </summary>
/// <remarks>
///     <para>
///         这几样发的是 <see cref="IListSortedEventArgs{T}"/>，而排序事件是移动事件的特例：
///         事件里报一串「从哪挪到哪」，照做一遍必须正好把整理<b>前</b>的列表变成整理<b>后</b>的列表
///         （统一用 <see cref="NotifiableCollectionsTestUtils.ReplayMoves{T}"/> 重放来对）。
///     </para>
///     <para>
///         整理那边靠最长上升子序列挑出「本来就不用动的项」，剩下的才逐项挪，所以已经排好的列表一条移动都不报；
///         反转则是确定性的——整表反转就是把末尾那项一次次挪到最前面。
///     </para>
/// </remarks>
public class ListReorderTests
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

    [Fact]
    public void SortOrdersTheElements()
    {
        var list = new NotifiableList<int> { 3, 1, 2 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort();

        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void SortWithComparisonUsesTheComparison()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort((a, b) => b.CompareTo(a));

        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3, 2, 1], list);
    }

    [Fact]
    public void SortWithComparerUsesTheComparer()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort(Comparer<int>.Create((a, b) => b.CompareTo(a)));

        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3, 2, 1], list);
    }

    [Fact]
    public void SortRangeOnlyReordersTheGivenRange()
    {
        var list = new NotifiableList<int> { 9, 3, 1, 2 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort(1, 3, Comparer<int>.Default);

        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([9, 1, 2, 3], list);
    }

    [Fact]
    public void ReverseReversesTheWholeList()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Reverse();

        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([3, 2, 1], list);
    }

    [Fact]
    public void ReverseRangeOnlyReversesTheGivenRange()
    {
        var list = new NotifiableList<int> { 1, 2, 3, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Reverse(1, 2);

        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 3, 2, 4], list);
    }

    [Fact]
    public void SortedEventsAreAlsoMovedEvents()
    {
        var list = new NotifiableList<int> { 2, 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort();

        // 排序是移动的特例：按移动订阅一样收得到
        var moved = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.IsAssignableFrom<IListSortedEventArgs<int>>(moved);
        Assert.Equal([1, 2], moved.ListView);
    }

    [Theory]
    [InlineData(new[] { 3, 1, 2 })]
    [InlineData(new[] { 2, 1 })]
    [InlineData(new[] { 5, 4, 3, 2, 1 })]
    [InlineData(new[] { 1, 3, 2, 5, 4 })]
    [InlineData(new[] { 1, 2, 3 })]
    public void SortReportsMovesThatReplayToTheSortedList(int[] items)
    {
        NotifiableList<int> list = [..items];
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort();

        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(original.Order(), sorted.ListView);
        Assert.Equal(sorted.ListView, NotifiableCollectionsTestUtils.ReplayMoves(original, sorted));
        Assert.Equal(sorted.Items.Count, sorted.OldIndexes.Count);
        Assert.Equal(sorted.Items.Count, sorted.NewIndexes.Count);
        Assert.Equal(sorted.Items.Count, sorted.ItemMoves.Count);
    }

    [Fact]
    public void SortingAnAlreadySortedListReportsNoMoves()
    {
        var list = Numbered(5);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort();

        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(sorted.ItemMoves);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void SortOfAnEmptyOrSingleElementListIsHarmless(int count)
    {
        var list = Numbered(count);

        // 空表和单项都没有可挪的：不抛异常、列表照旧；这种空操作发不发通知都合法，不钉
        list.Sort();

        Assert.Equal(Enumerable.Range(0, count), list);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    [InlineData(new[] { 1, 2 })]
    [InlineData(new[] { 9 })]
    public void ReverseReportsMovesThatReplayToTheReversedList(int[] items)
    {
        NotifiableList<int> list = [..items];
        var original = list.ToList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Reverse();

        var reversed = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(original.AsEnumerable().Reverse(), reversed.ListView);
        Assert.Equal(reversed.ListView, NotifiableCollectionsTestUtils.ReplayMoves(original, reversed));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 3)]
    [InlineData(2, 2)]
    public void ReverseRangeReportsMovesThatReplayToTheReversedRange(int index, int count)
    {
        var list = Numbered(5);
        var original = list.ToList();
        var expected = original.ToList();
        expected.Reverse(index, count);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Reverse(index, count);

        var reversed = Assert.IsAssignableFrom<IListItemMovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(expected, reversed.ListView);
        Assert.Equal(expected, NotifiableCollectionsTestUtils.ReplayMoves(original, reversed));
    }

    [Fact]
    public void ReverseWithNothingToReverseIsHarmless()
    {
        var empty = new NotifiableList<int>();
        var single = Numbered(1);
        var list = Numbered(4);

        // 空表、单项、空区间都没什么可反转的：不抛异常也不动列表，发不发通知都合法
        empty.Reverse();
        single.Reverse();
        list.Reverse(0, 0);

        Assert.Empty(empty);
        Assert.Equal([0], single);
        Assert.Equal([0, 1, 2, 3], list);
    }
}
