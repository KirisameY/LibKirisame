using System.Collections.ObjectModel;

using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;
using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     <c>BindCollection</c> 的默认解析链路：源只要是能发 <c>INotifyCollectionChanged</c> 的
///     <c>IReadOnlyCollection&lt;T&gt;</c>、目标只要是 <c>IList&lt;T&gt;</c> 或 <c>ICollection&lt;T&gt;</c>，
///     就都该被默认解析器认出来，并开始同步。
/// </summary>
public class CollectionBindingTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    // ---------- 列表源 → 列表目标 ----------

    [Fact]
    public void BindingPullsTheInitialContentsIntoTheTarget()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void AddedItemsAreAppendedToTheTarget()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Add(4);

        Assert.Equal(new[] { 1, 2, 3, 4 }, target);
    }

    [Fact]
    public void RemovedItemsDisappearFromTheTarget()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Remove(2);

        Assert.Equal(new[] { 1, 3 }, target);
    }

    [Fact]
    public void ReplacedItemsAreReplacedInTheTarget()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source[0] = 9;

        Assert.Equal(new[] { 9, 2, 3 }, target);
    }

    [Fact]
    public void MovingAnItemReordersTheTargetTheSameWay()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Move(0, 2);

        Assert.Equal(new[] { 2, 3, 1 }, target);
    }

    [Fact]
    public void ClearingTheSourceEmptiesTheTarget()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int> { 9 };

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Clear();

        Assert.Empty(target);
    }

    [Fact]
    public void DisposingTheHandleStopsThePropagation()
    {
        var source = new ObservableCollection<int> { 1 };
        var target = new List<int>();

        var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        handle.Dispose();

        source.Add(2);

        Assert.Equal(new[] { 1 }, target);
    }

    // ---------- 集合源（不是列表） → 列表目标 ----------

    [Fact]
    public void ACollectionSourceCanFeedAListTarget()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target.OrderBy(v => v));
    }

    [Fact]
    public void AnItemAddedToACollectionSourceLandsInTheListTarget()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Add(4);

        Assert.Equal(4, target.Count);
        Assert.Contains(4, target);
    }

    [Fact]
    public void AnItemRemovedFromACollectionSourceLeavesTheListTarget()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Remove(2);

        Assert.Equal(new[] { 1, 3 }, target.OrderBy(v => v));
    }

    [Fact]
    public void ClearingACollectionSourceEmptiesTheListTarget()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new List<int> { 9 };

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Clear();

        Assert.Empty(target);
    }

    // ---------- 集合源 → 集合目标 ----------

    [Fact]
    public void ACollectionSourceCanFeedACollectionTarget()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new HashSet<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target.OrderBy(v => v));

        source.Add(4);
        source.Remove(2);

        Assert.Equal(new[] { 1, 3, 4 }, target.OrderBy(v => v));
    }

    // ---------- 列表源 → 集合目标 ----------

    [Fact]
    public void AListSourceCanFeedACollectionTarget()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new HashSet<int>();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target.OrderBy(v => v));

        source.Add(4);
        source.Remove(2);

        Assert.Equal(new[] { 1, 3, 4 }, target.OrderBy(v => v));
    }
}
