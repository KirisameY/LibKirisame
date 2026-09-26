using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     走完整的 <c>DataBinderBuilder</c> → <c>IDataBinder</c> → 表达式树取成员这条链路，
///     也就是用户真正会写的那种用法。
/// </summary>
public class BinderEndToEndTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    private static IDataBinder CustomSourceBinder() =>
        new DataBinderBuilder()
           .WithResolver(typeof(ManualNotifySource), ManualNotifySource.CreateResolver())
           .Build();

    // ---------- 默认路径：INotifyPropertyChanged 数据源 ----------

    [Fact]
    public void OneWayBindingPullsTheInitialValueFromANotifyPropertyChangedSource()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(7, target.Number);
    }

    [Fact]
    public void OneWayBindingPushesLaterChangesFromANotifyPropertyChangedSource()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        source.Number = 9;

        Assert.Equal(9, target.Number);
    }

    [Fact]
    public void TwoWayBindingPullsTheInitialValueBetweenNotifyingObjects()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyTwoWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(7, target.Number);
    }

    [Fact]
    public void TwoWayBindingPropagatesInBothDirections()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyTwoWay(source, s => s.Number, target, t => t.Number);

        source.Number = 9;
        Assert.Equal(9, target.Number);

        target.Number = 11;
        Assert.Equal(11, source.Number);
    }

    [Fact]
    public void OneWayBindingWithAConverterUsesItForTheInitialValue()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Text, v => $"#{v}");

        Assert.Equal("#7", target.Text);
    }

    [Fact]
    public void BindingToAFieldTargetWorks()
    {
        var source = new NotifyObject { Number = 4 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Field);

        Assert.Equal(4, target.Field);
    }

    [Fact]
    public void BindingToAnIndexerTargetWorks()
    {
        var source = new NotifyObject { Number = 6 };
        var target = new NotifyObject();

        using var handle = DefaultBinder().BindPropertyOneWay(source, s => s.Number, target, t => t["k"]);

        Assert.Equal(6, target["k"]);
    }

    // ---------- 任意类型：不用 INotifyPropertyChanged，靠自定义解析器接进来 ----------

    [Fact]
    public void ATypeThatOnlyHasACustomResolverCanBeABindingSource()
    {
        var source = new ManualNotifySource { Number = 3 };
        var target = new PlainObject();

        using var handle = CustomSourceBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(3, target.Number);

        source.Number = 8;
        Assert.Equal(8, target.Number);
    }

    [Fact]
    public void ATypeThatOnlyHasACustomResolverCanBeATwoWayParticipant()
    {
        var source = new ManualNotifySource { Number = 1 };
        var target = new ManualNotifySource();

        using var handle = CustomSourceBinder().BindPropertyTwoWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(1, target.Number);

        source.Number = 2;
        Assert.Equal(2, target.Number);

        target.Number = 5;
        Assert.Equal(5, source.Number);
    }

    [Fact]
    public void DisposingACustomResolvedBindingStopsThePropagation()
    {
        var source = new ManualNotifySource { Number = 3 };
        var target = new PlainObject();

        var handle = CustomSourceBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);
        Assert.Equal(3, target.Number);

        handle.Dispose();

        source.Number = 12;
        Assert.Equal(3, target.Number);
    }

    [Fact]
    public void ACustomResolvedSourceCanFeedANotifyingTarget()
    {
        var source = new ManualNotifySource { Number = 4 };
        var target = new NotifyObject();

        using var handle = CustomSourceBinder().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(4, target.Number);

        source.Number = 6;
        Assert.Equal(6, target.Number);
    }
}
