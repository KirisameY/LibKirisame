using System.Collections.Immutable;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     列表作为 <see cref="INotifiableCollection{T}"/> 时走上来的那批集合级操作：
///     <see cref="INotifiableCollection{T}.AddRange"/>、<see cref="INotifiableCollection{T}.RemoveRange"/>、
///     <see cref="INotifiableCollection{T}.RemoveAll"/>、<see cref="INotifiableCollection{T}.Replace"/> 与
///     <see cref="INotifiableCollection{T}.ReplaceRange"/>。
/// </summary>
/// <remarks>
///     <para>
///         这些成员在 <see cref="INotifiableList{T}"/> 上都有显式接口实现：按值移除 / 替换要先
///         <see cref="IList{T}.IndexOf"/> 定位，再转交给列表自己的批量索引操作。所以下面重点钉三件事：
///         索引 0 也必须能被找到、入参顺序不应影响结果、重复入参不应把列表搞崩。
///     </para>
///     <para>
///         These members are implemented explicitly on <see cref="INotifiableList{T}"/>: value-based removal and
///         replacement first locate the item with <see cref="IList{T}.IndexOf"/> and then delegate to the list's own
///         index-based batch operations. The tests below therefore pin three things: index 0 must be found, the
///         argument order must not matter, and duplicate arguments must not break the list.
///     </para>
/// </remarks>
public class ListCollectionInterfaceTests
{
    [Fact]
    public void AddRangeThroughTheCollectionInterfaceAppendsEveryItem()
    {
        var list = new NotifiableList<int> { 1 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        collection.AddRange(new[] { 2, 3 });

        Assert.Equal([1, 2, 3], list);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal([1, 2], added.Indexes);
    }

    [Fact]
    public void AddRangeAcceptsAnIReadOnlyCollectionThatIsNotAnICollection()
    {
        // 签名从 ICollection<T> 放宽成了 IReadOnlyCollection<T>：不可变集合只满足后者
        var list = new NotifiableList<int> { 1 };
        IReadOnlyCollection<int> source = ImmutableList.Create(2, 3);

        list.AddRange(source);

        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void InsertRangeAcceptsAnIReadOnlyCollectionThatIsNotAnICollection()
    {
        var list = new NotifiableList<int> { 1, 4 };
        IReadOnlyCollection<int> source = ImmutableList.Create(2, 3);

        list.InsertRange(1, source);

        Assert.Equal([1, 2, 3, 4], list);
    }

    [Fact]
    public void RemoveRangeByValuesRemovesElementsAtEveryPositionIncludingTheFirst()
    {
        var list = new NotifiableList<int> { 10, 20, 30, 40 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var removed = collection.RemoveRange([10, 40]);

        // 索引 0 也是合法位置；按值移除必须能找到它
        Assert.Equal([10, 40], removed);
        Assert.Equal([20, 30], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 40], removedArgs.RemovedItems);
        Assert.Equal([0, 3], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeByValuesIgnoresInputOrderAndMissingElements()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 集合语义下入参顺序没有意义；不存在的元素静默跳过
        var removed = collection.RemoveRange([30, 99, 10]);

        Assert.Equal([10, 30], removed.Order());
        Assert.Equal([20], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 30], removedArgs.RemovedItems);
        Assert.Equal([0, 2], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeByValuesRemovesARepeatedElementOnlyOnce()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 重复的入参只能命中同一个位置，不应产生重复索引把 MultiRemove 弄崩
        var removed = collection.RemoveRange([20, 20]);

        Assert.Equal([20], removed);
        Assert.Equal([10, 30], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeByValuesWithNoMatchesRaisesAnEmptyRemoval()
    {
        var list = new NotifiableList<int> { 10, 20 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 与 MultiRemove 的空入参一致：无条件发一条空变更
        var removed = collection.RemoveRange([99]);

        Assert.Empty(removed);
        Assert.Equal([10, 20], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(removedArgs.RemovedItems);
        Assert.Empty(removedArgs.Indexes);
    }

    [Fact]
    public void RemoveAllThroughTheCollectionInterfaceReturnsEveryRemovedItem()
    {
        var list = new NotifiableList<int> { 1, 2, 3, 4 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var removed = collection.RemoveAll(i => i % 2 == 0);

        Assert.Equal([2, 4], removed);
        Assert.Equal([1, 3], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 4], removedArgs.RemovedItems);
        Assert.Equal([1, 3], removedArgs.Indexes);
    }

    [Fact]
    public void ReplaceFindsAndReplacesTheFirstOccurrenceIncludingIndexZero()
    {
        var list = new NotifiableList<int> { 1, 2, 1 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.True(collection.Replace(1, 9));

        Assert.Equal([9, 2, 1], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], replaced.OldItems);
        Assert.Equal([9], replaced.NewItems);
        Assert.Equal([0], replaced.Indexes);

        // 第二次命中剩下的那一个（现在位于索引 2）
        Assert.True(collection.Replace(1, 8));

        Assert.Equal([9, 2, 8], list);
        Assert.Equal(2, updates.Count);
        Assert.Equal([2], Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(updates[1]).Indexes);
    }

    [Fact]
    public void ReplaceReturnsFalseAndRaisesNothingForAMissingElement()
    {
        var list = new NotifiableList<int> { 1, 2 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        Assert.False(collection.Replace(3, 9));

        Assert.Equal([1, 2], list);
        Assert.Empty(updates);
    }

    [Fact]
    public void ReplaceRangeHandlesEveryPositionIncludingTheFirst()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var results = collection.ReplaceRange([(10, 11), (20, 22), (30, 33)]);

        Assert.Equal([true, true, true], results.Select(r => r.Replaced));
        Assert.Equal([10, 20, 30], results.Select(r => r.Left));
        Assert.Equal([11, 22, 33], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 20, 30], replaced.OldItems);
        Assert.Equal([11, 22, 33], replaced.NewItems);
        Assert.Equal([0, 1, 2], replaced.Indexes);
    }

    [Fact]
    public void ReplaceRangeDoesNotRequireAscendingInputOrder()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var results = collection.ReplaceRange([(30, 33), (10, 11)]);

        Assert.Equal([true, true], results.Select(r => r.Replaced));
        Assert.Equal([10, 30], results.Select(r => r.Left).Order());
        Assert.Equal([11, 20, 33], list);
        Assert.Single(updates);
    }

    [Fact]
    public void ReplaceRangeReportsOnlyTheFirstCommandForTheSameElement()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 两条指令都指向同一个旧元素：第一条生效，第二条按失败返回它的新元素
        var results = collection.ReplaceRange([(20, 21), (20, 22)]);

        Assert.Contains((false, 22), results);
        Assert.Contains((true, 20), results);
        Assert.Equal([10, 21, 30], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([20], replaced.OldItems);
        Assert.Equal([21], replaced.NewItems);
        Assert.Equal([1], replaced.Indexes);
    }

    [Fact]
    public void ReplaceRangeReportsAMissingOldElementWithoutTouchingTheList()
    {
        var list = new NotifiableList<int> { 10, 20 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        var results = collection.ReplaceRange([(99, 1)]);

        Assert.Equal([(false, 1)], results);
        Assert.Equal([10, 20], list);

        // 与 MultiReplace 的空入参一致：没有成功的替换也会收到一条空变更
        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Empty(replaced.OldItems);
        Assert.Empty(replaced.NewItems);
        Assert.Empty(replaced.Indexes);
    }
}
