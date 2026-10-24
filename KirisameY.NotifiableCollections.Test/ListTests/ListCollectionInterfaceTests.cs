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
///         这些按值的批量操作不再由 <see cref="INotifiableList{T}"/> 提供默认实现，而是落在
///         <see cref="NotifiableListSimpleBase{T}"/>：先按值扫描列表、收集要作用的位置，
///         再交给 <see cref="INotifiableList{T}.MultiRemove"/> / <see cref="INotifiableList{T}.MultiReplace"/>。
///         扫描时按列表顺序消费入参，重复入参对应重复出现，而不是先去重。所以下面重点钉四件事：
///         索引 0 也必须能被找到、入参顺序不应影响结果、重复入参按出现次数生效、入参比出现次数多时静默跳过多余部分。
///     </para>
///     <para>
///         These value-based batch operations are no longer default interface implementations on
///         <see cref="INotifiableList{T}"/>; they live on <see cref="NotifiableListSimpleBase{T}"/>, which scans the
///         list in order, collects the affected positions and then delegates to
///         <see cref="INotifiableList{T}.MultiRemove"/> / <see cref="INotifiableList{T}.MultiReplace"/>. The scan
///         consumes the arguments in list order, so a repeated argument means a repeated occurrence rather than a
///         deduplicated one. The tests below therefore pin four things: index 0 must be found, the argument order must
///         not matter, repeated arguments must apply once per occurrence, and surplus arguments must be skipped silently.
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

        // 返回值与事件里的项都按列表原顺序排列，而不是按入参顺序
        Assert.Equal([10, 30], removed);
        Assert.Equal([20], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([10, 30], removedArgs.RemovedItems);
        Assert.Equal([0, 2], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeByValuesRemovesOneOccurrencePerRepeatedEntry()
    {
        var list = new NotifiableList<int> { 10, 20, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 重复入参不再先去重：两个 20 从前往后各移除一次
        var removed = collection.RemoveRange([20, 20]);

        Assert.Equal([20, 20], removed);
        Assert.Equal([10, 30], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([20, 20], removedArgs.RemovedItems);
        Assert.Equal([1, 2], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeByValuesRemovesOnlyTheFirstOccurrenceOfASingleEntry()
    {
        var list = new NotifiableList<int> { 20, 10, 20 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 一条入参只消费一个位置，不会把后面同值的项一起带走
        var removed = collection.RemoveRange([20]);

        Assert.Equal([20], removed);
        Assert.Equal([10, 20], list);

        var removedArgs = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([0], removedArgs.Indexes);
    }

    [Fact]
    public void RemoveRangeByValuesSkipsRepeatedEntriesOnceOccurrencesRunOut()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 入参比实际出现次数多时，多出来的那份静默跳过，仍只移除一个 20
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
        // 成功项按列表索引升序返回，入参顺序不影响结果顺序
        Assert.Equal([10, 30], results.Select(r => r.Left));
        Assert.Equal([11, 20, 33], list);
        Assert.Single(updates);
    }

    [Fact]
    public void ReplaceRangeReplacesOnlyOneOccurrencePerCommand()
    {
        var list = new NotifiableList<int> { 20, 10, 20 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 一条指令只替换一个位置，后面同值的项保持原样
        var results = collection.ReplaceRange([(20, 21)]);

        Assert.Equal([(true, 20)], results);
        Assert.Equal([21, 10, 20], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([0], replaced.Indexes);
    }

    [Fact]
    public void ReplaceRangePairsRepeatedCommandsWithSuccessiveOccurrences()
    {
        var list = new NotifiableList<int> { 10, 20, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 同一个旧值出现几次就消费几条指令，并按列表顺序配对；命令顺序决定新值落在哪个位置
        var results = collection.ReplaceRange([(20, 22), (20, 21)]);

        Assert.Equal([(true, 20), (true, 20)], results);
        Assert.Equal([10, 22, 21, 30], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([20, 20], replaced.OldItems);
        Assert.Equal([22, 21], replaced.NewItems);
        Assert.Equal([1, 2], replaced.Indexes);
    }

    [Fact]
    public void ReplaceRangeReportsExtraCommandsOnceOccurrencesRunOut()
    {
        var list = new NotifiableList<int> { 10, 20, 30 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 两条指令都指向 20，但列表里只有一个 20：第一条生效，第二条按失败返回它的新元素
        var results = collection.ReplaceRange([(20, 21), (20, 22)]);

        Assert.Equal([(true, 20), (false, 22)], results);
        Assert.Equal([10, 21, 30], list);

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([20], replaced.OldItems);
        Assert.Equal([21], replaced.NewItems);
        Assert.Equal([1], replaced.Indexes);
    }

    [Fact]
    public void ReplaceRangeReportsEveryCommandThatFoundNoOccurrence()
    {
        var list = new NotifiableList<int> { 20 };
        INotifiableCollection<int> collection = list;
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        // 列表里只有一个 20：第一条指令生效，其余都按失败返回，且一条都不能丢
        var results = collection.ReplaceRange([(20, 21), (20, 22), (20, 23)]);

        Assert.Equal([(true, 20)], results.Where(r => r.Replaced));
        Assert.Equal([22, 23], results.Where(r => !r.Replaced).Select(r => r.Left).Order());
        Assert.Equal([21], list);
        Assert.Single(updates);
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
