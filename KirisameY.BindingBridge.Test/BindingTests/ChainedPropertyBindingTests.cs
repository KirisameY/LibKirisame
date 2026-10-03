using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     链式属性路径（<c>s =&gt; s.Inner.Number</c>）的语义：链上任何一跳发通知，都让整条链重新求值——
///     所以既有"末端属性变了"，也有"中间那一跳被换成了别的对象"。
/// </summary>
public class ChainedPropertyBindingTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    // ---------- 源侧 ----------

    [Fact]
    public void AChainedSourcePullsTheInitialValue()
    {
        var source = new ChainedNotifyNode { Inner = new ChainedNotifyNode { Number = 7 } };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Number, target, t => t.Number);

        Assert.Equal(7, target.Number);
    }

    [Fact]
    public void ChangingTheEndOfAChainPropagates()
    {
        var leaf = new ChainedNotifyNode { Number = 7 };
        var source = new ChainedNotifyNode { Inner = leaf };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Number, target, t => t.Number);

        leaf.Number = 9;

        Assert.Equal(9, target.Number);
    }

    [Fact]
    public void AThreeHopChainWorks()
    {
        var leaf = new ChainedNotifyNode { Number = 5 };
        var source = new ChainedNotifyNode { Inner = new ChainedNotifyNode { Inner = leaf } };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Inner!.Number, target, t => t.Number);

        Assert.Equal(5, target.Number);

        leaf.Number = 6;

        Assert.Equal(6, target.Number);
    }

    [Fact]
    public void AChainCanGoThroughAnIndexer()
    {
        var leaf = new ChainedNotifyNode();
        var source = new ChainedNotifyNode { Inner = leaf };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!["k"], target, t => t.Number);

        leaf["k"] = 5;

        Assert.Equal(5, target.Number);
    }

    [Fact]
    public void ReplacingAMiddleNodeMakesTheBindingFollowTheNewOne()
    {
        var oldInner = new ChainedNotifyNode { Number = 1 };
        var source = new ChainedNotifyNode { Inner = oldInner };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Number, target, t => t.Number);
        Assert.Equal(1, target.Number);

        var newInner = new ChainedNotifyNode { Number = 2 };
        source.Inner = newInner;
        Assert.Equal(2, target.Number);

        // 换过之后，跟着新对象走
        newInner.Number = 3;
        Assert.Equal(3, target.Number);

        // 而旧对象已经不在链上了
        oldInner.Number = 4;
        Assert.Equal(3, target.Number);
    }

    [Fact]
    public void ReplacingAHopOfAThreeHopChainRewiresEveryHopBelowIt()
    {
        var leaf = new ChainedNotifyNode { Number = 1 };
        var middle = new ChainedNotifyNode { Inner = leaf };
        var source = new ChainedNotifyNode { Inner = middle };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Inner!.Number, target, t => t.Number);
        Assert.Equal(1, target.Number);

        // 换掉最里面那一跳：只影响它自己
        middle.Inner = new ChainedNotifyNode { Number = 2 };
        Assert.Equal(2, target.Number);
        leaf.Number = 8;
        Assert.Equal(2, target.Number);

        // 换掉最外面那一跳：下面每一跳都得重新接上
        var newMiddle = new ChainedNotifyNode { Inner = new ChainedNotifyNode { Number = 3 } };
        source.Inner = newMiddle;
        Assert.Equal(3, target.Number);

        newMiddle.Inner!.Number = 4;
        Assert.Equal(4, target.Number);
    }

    [Fact]
    public void TwoBindingsOnTheSameChainBothReceiveUpdates()
    {
        var leaf = new ChainedNotifyNode { Number = 1 };
        var source = new ChainedNotifyNode { Inner = leaf };
        var first = new NotifyObject();
        var second = new NotifyObject();
        var binder = DefaultBinder();

        using var firstHandle = binder.BindPropertyOneWay(source, s => s.Inner!.Number, first, t => t.Number);
        using var secondHandle = binder.BindPropertyOneWay(source, s => s.Inner!.Number, second, t => t.Number);

        Assert.Equal(1, first.Number);
        Assert.Equal(1, second.Number);

        leaf.Number = 5;

        Assert.Equal(5, first.Number);
        Assert.Equal(5, second.Number);
    }

    [Fact]
    public void DisposingOneBindingLeavesTheOtherOneAlive()
    {
        var leaf = new ChainedNotifyNode { Number = 1 };
        var source = new ChainedNotifyNode { Inner = leaf };
        var first = new NotifyObject();
        var second = new NotifyObject();
        var binder = DefaultBinder();

        var firstHandle = binder.BindPropertyOneWay(source, s => s.Inner!.Number, first, t => t.Number);
        using var secondHandle = binder.BindPropertyOneWay(source, s => s.Inner!.Number, second, t => t.Number);

        firstHandle.Dispose();

        leaf.Number = 5;

        Assert.Equal(1, first.Number);
        Assert.Equal(5, second.Number);
    }

    [Fact]
    public void DisposingAChainedBindingStopsThePropagation()
    {
        var leaf = new ChainedNotifyNode { Number = 1 };
        var source = new ChainedNotifyNode { Inner = leaf };
        var target = new NotifyObject();

        var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Number, target, t => t.Number);
        handle.Dispose();

        leaf.Number = 5;
        source.Inner = new ChainedNotifyNode { Number = 6 };

        Assert.Equal(1, target.Number);
    }

    [Fact]
    public void DisposingTwiceIsHarmless()
    {
        var source = new ChainedNotifyNode { Inner = new ChainedNotifyNode { Number = 1 } };
        var target = new NotifyObject();

        var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.Number, target, t => t.Number);
        handle.Dispose();
        handle.Dispose();
    }

    // ---------- 目标侧 ----------

    [Fact]
    public void AChainedTargetReceivesTheInitialValue()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new ChainedNotifyNode { Inner = new ChainedNotifyNode() };

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Inner!.Number);

        Assert.Equal(7, target.Inner!.Number);
    }

    [Fact]
    public void ChangesArePushedIntoAChainedTarget()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new ChainedNotifyNode { Inner = new ChainedNotifyNode() };

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Inner!.Number);

        source.Number = 9;

        Assert.Equal(9, target.Inner!.Number);
    }

    [Fact]
    public void ATwoWayBindingThroughAChainWorksInBothDirections()
    {
        var source = new ChainedNotifyNode { Inner = new ChainedNotifyNode { Number = 3 } };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyTwoWay(source, s => s.Inner!.Number, target, t => t.Number);

        Assert.Equal(3, target.Number);

        source.Inner!.Number = 4;
        Assert.Equal(4, target.Number);

        target.Number = 5;
        Assert.Equal(5, source.Inner!.Number);
    }

    // ---------- 拒绝 ----------

    [Fact]
    public void AReadOnlyPropertyAtTheEndOfAChainIsStillAValidOneWaySource()
    {
        var source = new ChainedNotifyNode { Inner = new ChainedNotifyNode { Number = 4 } };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Inner!.ReadOnlyNumber, target, t => t.Number);

        Assert.Equal(4, target.Number);
    }

    [Fact]
    public void AChainEndingInAReadOnlyPropertyCannotBeAUniversalSource()
    {
        var source = new ChainedNotifyNode { Inner = new ChainedNotifyNode() };
        var target = new NotifyObject();

        var ex = Assert.Throws<ArgumentException>(
            () => DefaultBinder().BindPropertyTwoWay(source, s => s.Inner!.ReadOnlyNumber, target, t => t.Number)
        );

        Assert.Equal("s => s.Inner.ReadOnlyNumber is not writable or not observable.", ex.Message);
    }

    [Fact]
    public void AChainEndingInAReadOnlyPropertyCannotBeWrittenThrough()
    {
        var source = new NotifyObject { Number = 1 };
        var target = new ChainedNotifyNode { Inner = new ChainedNotifyNode() };

        var ex = Assert.Throws<ArgumentException>(
            () => DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Inner!.ReadOnlyNumber)
        );

        Assert.Equal("t => t.Inner.ReadOnlyNumber is not writable.", ex.Message);
    }

    // ---------- 链中间那一跳 ----------

    [Fact]
    public void AMiddleHopThatNeverNotifiesKeepsTheBindingOnTheOriginalObject()
    {
        var oldNode = new ChainedNotifyNode { Number = 1 };
        var root = new SilentChainedRoot { Node = oldNode };
        var target = new NotifyObject();

        // 末端（Node.Number）可观察就绑得上，中间那一跳不发通知并不妨碍绑定成立
        using var handle = DefaultBinder().BindPropertyOneWay(root, r => r.Node.Number, target, t => t.Number);
        Assert.Equal(1, target.Number);

        oldNode.Number = 2;
        Assert.Equal(2, target.Number);

        // 但这一跳没人通知，换掉 Node 之后绑定不跟过去——它钉在原来那个对象上
        root.Node = new ChainedNotifyNode { Number = 9 };
        Assert.Equal(2, target.Number);
    }
}
