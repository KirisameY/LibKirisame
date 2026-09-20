using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

public class ListReorderTests
{
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
    public void SortedEventCarriesNoIndexInformation()
    {
        var list = new NotifiableList<int> { 2, 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Sort();

        // 元素没有增减，只是整表重排；要新顺序就直接读 ListView
        var sorted = Assert.IsAssignableFrom<IListSortedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2], sorted.ListView);
    }
}
