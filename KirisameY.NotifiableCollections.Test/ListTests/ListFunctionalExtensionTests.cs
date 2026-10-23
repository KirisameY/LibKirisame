using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="NotifiableListFunctionalExtensions"/> 中由原 <see cref="NotifiableList{T}"/> 实例方法
///     拆出来的重载：惰性序列版本、按位配对的批量版本，以及不带比较器的排序版本。
/// </summary>
public class ListFunctionalExtensionTests
{
    [Fact]
    public void SortRangeWithoutAComparerUsesTheDefaultOrder()
    {
        var list = new NotifiableList<int> { 9, 3, 1, 2 };

        list.Sort(1, 3);

        Assert.Equal([9, 1, 2, 3], list);
    }

    [Fact]
    public void SortRangeWithAComparisonUsesTheComparison()
    {
        var list = new NotifiableList<int> { 9, 1, 3, 2 };

        list.Sort(1, 3, (a, b) => b.CompareTo(a));

        Assert.Equal([9, 3, 2, 1], list);
    }

    [Fact]
    public void FunctionalOverloadsAreUsableThroughTheInterface()
    {
        INotifiableList<int> list = new NotifiableList<int>();

        // 这几行都只能解析到 extensions：接口上只有 ICollection / IComparer 版本
        list.AddRange(Enumerable.Range(0, 5));
        list.InsertRange(2, Enumerable.Range(10, 2));
        list.MultiInsert(new[] { 0 }, new[] { -1 });
        list.MultiReplace(new[] { 0 }, new[] { -2 });
        list.Sort((a, b) => b.CompareTo(a));

        Assert.Equal([11, 10, 4, 3, 2, 1, 0, -2], list);
    }

    [Fact]
    public void LazyAddRangeIsMaterializedBeforeTheListChanges()
    {
        var list = new NotifiableList<int> { 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var enumerations = 0;
        IEnumerable<int> Source()
        {
            enumerations++;
            yield return 2;
            yield return 3;
        }

        list.AddRange(Source());

        Assert.Equal(1, enumerations);
        Assert.Equal([1, 2, 3], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal([1, 2], added.Indexes);
    }
}
