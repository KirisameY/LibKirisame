using System.Collections.ObjectModel;

using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.CollectionBinding.Resolver;
using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.BindingBridge.NotifiableCollections.Test.BindingTests;

/// <summary>
///     把 Notifiable 的集合作绑定源：<c>NotifiableCollectionSourceEndpointResolver</c> 兜在默认源解析器外面，
///     于是 <c>NotifiableList&lt;T&gt;</c> 与非列表的 <c>IReadOnlyNotifiableCollection&lt;T&gt;</c>
///     都能直接交给 <c>BindCollection</c>，目标侧仍旧用默认解析器。
/// </summary>
public class NotifiableCollectionBindingTests
{
    private static IDataBinder Binder() =>
        new DataBinderBuilder()
           .WithCollectionSourceFallbackResolver(
                new NotifiableCollectionSourceEndpointResolver(DefaultCollectionEndpointSourceResolver.Instance)
            )
           .Build();

    // ---------- NotifiableList<T> 源 → 列表目标 ----------

    [Fact]
    public void ANotifiableListSourcePullsItsInitialContentsIntoTheTarget()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void AnItemAddedToANotifiableListSourceLandsInTheTarget()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        source.Add(4);

        Assert.Equal(new[] { 1, 2, 3, 4 }, target);
    }

    [Fact]
    public void ARangeInsertedIntoANotifiableListSourceLandsInTheTargetInOrder()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        source.InsertRange(1, [7, 8]);

        Assert.Equal(new[] { 1, 7, 8, 2, 3 }, target);
    }

    [Fact]
    public void AnItemRemovedFromANotifiableListSourceLeavesTheTarget()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        source.Remove(2);

        Assert.Equal(new[] { 1, 3 }, target);
    }

    [Fact]
    public void AssigningThroughTheIndexerReplacesTheItemInTheTarget()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        source[0] = 9;

        Assert.Equal(new[] { 9, 2, 3 }, target);
    }

    [Fact]
    public void MovingAnItemInANotifiableListReordersTheTargetTheSameWay()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        source.Move(0, 2);

        Assert.Equal(new[] { 2, 3, 1 }, target);
    }

    [Fact]
    public void ClearingANotifiableListEmptiesTheTarget()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int> { 9 };

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        source.Clear();

        Assert.Empty(target);
    }

    [Fact]
    public void SortingANotifiableListReloadsTheTarget()
    {
        var source = new NotifiableList<int> { 3, 1, 2 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        // 重排通知不携带索引，只会让目标整体重载
        source.Sort();

        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void DisposingTheHandleStopsThePropagation()
    {
        var source = new NotifiableList<int> { 1, 2, 3 };
        var target = new List<int>();

        var handle = Binder().BindCollection(source, target, TypeA.Of<int>());
        handle.Dispose();

        source.Add(4);

        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    // ---------- 非列表的 Notifiable 集合源 ----------

    [Fact]
    public void ANonListNotifiableCollectionSourceBindsToAListTarget()
    {
        var backing = new ObservableCollection<int> { 1, 2, 3 };
        IReadOnlyNotifiableCollection<int> source = backing.AsNotifiableCollection(TypeA.Of<int>());
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target);

        backing.Add(4);
        Assert.Equal(new[] { 1, 2, 3, 4 }, target);

        backing.Remove(2);
        Assert.Equal(new[] { 1, 3, 4 }, target);

        backing.Clear();
        Assert.Empty(target);
    }

    [Fact]
    public void ANonListNotifiableCollectionSourceCanFeedACollectionTarget()
    {
        var backing = new ObservableCollection<int> { 1, 2, 3 };
        IReadOnlyNotifiableCollection<int> source = backing.AsNotifiableCollection(TypeA.Of<int>());
        var target = new HashSet<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target.OrderBy(v => v));

        backing.Add(4);
        backing.Remove(2);

        Assert.Equal(new[] { 1, 3, 4 }, target.OrderBy(v => v));
    }

    // ---------- 与默认解析器的接力 ----------

    [Fact]
    public void TheResolversInnerFallbackStillServesPlainObservableCollections()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new List<int>();

        using var handle = Binder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target);

        source.Add(4);

        Assert.Equal(new[] { 1, 2, 3, 4 }, target);
    }

    [Fact]
    public void ATypeThatIsNeitherNotifiableNorObservableIsRejected() =>
        Assert.Throws<ArgumentException>(() =>
            Binder().BindCollection(new List<int> { 1 }, new List<int>(), TypeA.Of<int>()));
}
