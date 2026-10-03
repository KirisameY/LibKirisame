using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;
using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     链式集合绑定（<c>BindCollection(obj, o =&gt; o.Items, ...)</c>）：绑定的两头是"对象上的集合属性"。
///     除了集合内部的增删要传到目标，源侧的集合属性被整个换成另一个集合时，目标也得跟着换过去。
/// </summary>
public class ChainedCollectionBindingTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    [Fact]
    public void AChainedCollectionSourcePullsItsInitialContentsIntoTheTarget()
    {
        var source = new ChainedCollectionHolder { Items = [1, 2, 3] };
        var target = new ChainedCollectionHolder();

        using var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t.Items, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target.Items);
    }

    [Fact]
    public void ElementsAddedToAChainedCollectionSourceLandInTheTarget()
    {
        var source = new ChainedCollectionHolder { Items = [1, 2, 3] };
        var target = new ChainedCollectionHolder();

        using var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t.Items, TypeA.Of<int>());

        source.Items.Add(4);

        Assert.Equal(new[] { 1, 2, 3, 4 }, target.Items);
    }

    [Fact]
    public void ElementsRemovedFromAChainedCollectionSourceLeaveTheTarget()
    {
        var source = new ChainedCollectionHolder { Items = [1, 2, 3] };
        var target = new ChainedCollectionHolder();

        using var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t.Items, TypeA.Of<int>());

        source.Items.Remove(2);

        Assert.Equal(new[] { 1, 3 }, target.Items);
    }

    [Fact]
    public void ReplacingTheWholeSourceCollectionReloadsTheTarget()
    {
        var source = new ChainedCollectionHolder { Items = [1, 2, 3] };
        var target = new ChainedCollectionHolder();

        using var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t.Items, TypeA.Of<int>());
        Assert.Equal(new[] { 1, 2, 3 }, target.Items);

        var oldItems = source.Items;
        source.Items = [7, 8];

        Assert.Equal(new[] { 7, 8 }, target.Items);

        // 之后跟着新集合走
        source.Items.Add(9);
        Assert.Equal(new[] { 7, 8, 9 }, target.Items);

        // 旧集合已经不在链上了
        oldItems.Add(4);
        Assert.Equal(new[] { 7, 8, 9 }, target.Items);
    }

    [Fact]
    public void AChainedCollectionThatIsNotAListStillBindsToAListTarget()
    {
        var source = new ChainedSetHolder { Items = [1, 2, 3] };
        var target = new ChainedCollectionHolder();

        using var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t.Items, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target.Items.OrderBy(v => v));

        source.Items.Add(4);
        Assert.Equal(new[] { 1, 2, 3, 4 }, target.Items.OrderBy(v => v));

        source.Items.Remove(2);
        Assert.Equal(new[] { 1, 3, 4 }, target.Items.OrderBy(v => v));
    }

    [Fact]
    public void APlainCollectionObjectCanBeTheTargetOfAChainedSource()
    {
        var source = new ChainedCollectionHolder { Items = [1, 2, 3] };
        var target = new List<int>();

        using var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t, TypeA.Of<int>());

        Assert.Equal(new[] { 1, 2, 3 }, target);

        source.Items.Add(4);
        Assert.Equal(new[] { 1, 2, 3, 4 }, target);
    }

    [Fact]
    public void DisposingAChainedCollectionBindingStopsThePropagation()
    {
        var source = new ChainedCollectionHolder { Items = [1] };
        var target = new ChainedCollectionHolder();

        var handle = DefaultBinder().BindCollection(source, s => s.Items, target, t => t.Items, TypeA.Of<int>());
        Assert.Equal(new[] { 1 }, target.Items);

        source.Items.Add(2);
        Assert.Equal(new[] { 1, 2 }, target.Items);

        // 换过集合之后再退订：现在挂在链上的是新集合，退订得把它也摘干净
        source.Items = [7, 8];
        Assert.Equal(new[] { 7, 8 }, target.Items);

        handle.Dispose();

        source.Items.Add(9);
        Assert.Equal(new[] { 7, 8 }, target.Items);
    }

    [Fact]
    public void ACollectionPropertyThatIsNotObservableIsRejected()
    {
        var source = new ChainedCollectionHolder();
        var target = new ChainedCollectionHolder();

        var ex = Assert.Throws<ArgumentException>(
            () => DefaultBinder().BindCollection(source, s => s.PlainItems, target, t => t.Items, TypeA.Of<int>())
        );

        Assert.Equal(
            "System.Collections.Generic.List`1[System.Int32] is not a observable collection of System.Int32",
            ex.Message
        );
    }
}
