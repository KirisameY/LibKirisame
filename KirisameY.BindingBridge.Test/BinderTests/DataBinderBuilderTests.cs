using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BinderTests;

/// <summary>
///     <c>DataBinderBuilder</c> + <c>RegistryDataBinder</c>：按类型挑解析器，
///     先精确匹配声明类型，再沿 <c>BaseType</c> 向上找，都没有就落到兜底。
/// </summary>
public class DataBinderBuilderTests
{
    private class BaseSource
    {
        public int Number { get; set; }
    }

    private sealed class DerivedSource : BaseSource;

    [Fact]
    public void WithResolverMakesANonNotifyingTypeBindable()
    {
        var binder = new DataBinderBuilder()
                    .WithResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver())
                    .Build();

        var source = new PlainObject { Number = 4 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(4, target.Number);
    }

    [Fact]
    public void AResolverRegisteredForABaseTypeAlsoServesDerivedInstancesTypedAsTheBase()
    {
        var binder = new DataBinderBuilder()
                    .WithResolver(typeof(BaseSource), new AlwaysResolvePropertyUpdateNotifyResolver())
                    .Build();

        BaseSource source = new DerivedSource { Number = 2 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(2, target.Number);
    }

    [Fact]
    public void AResolverRegisteredForABaseTypeIsFoundByWalkingUpFromTheDeclaredType()
    {
        var registered = new AlwaysResolvePropertyUpdateNotifyResolver();
        var binder = new DataBinderBuilder().WithResolver(typeof(BaseSource), registered).Build();

        // TSource 推导为 DerivedSource，注册表里只有 BaseSource
        var source = new DerivedSource { Number = 2 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(2, target.Number);

        // 沿 BaseType 找到了 BaseSource 的解析器，但传进去的仍是原始的声明类型
        var seen = Assert.Single(registered.Seen);
        Assert.Equal(typeof(DerivedSource), seen.Type);
    }

    [Fact]
    public void TheMostDerivedRegistrationWins()
    {
        var baseResolver = new AlwaysResolvePropertyUpdateNotifyResolver();
        var derivedResolver = new AlwaysResolvePropertyUpdateNotifyResolver();
        var binder = new DataBinderBuilder()
                    .WithResolver(typeof(BaseSource), baseResolver)
                    .WithResolver(typeof(DerivedSource), derivedResolver)
                    .Build();

        var source = new DerivedSource { Number = 3 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Single(derivedResolver.Seen);
        Assert.Empty(baseResolver.Seen);
    }

    [Fact]
    public void AResolverRegisteredForADerivedTypeDoesNotApplyToTheBaseType()
    {
        var derivedResolver = new AlwaysResolvePropertyUpdateNotifyResolver();
        var binder = new DataBinderBuilder()
                    .WithResolver(typeof(DerivedSource), derivedResolver)
                    .WithFallbackResolver(new NullPropertyUpdateNotifyResolver())
                    .Build();

        var source = new BaseSource { Number = 3 };

        Assert.Throws<ArgumentException>(() =>
            binder.BindPropertyOneWay(source, s => s.Number, new PlainObject(), t => t.Number));
        Assert.Empty(derivedResolver.Seen);
    }

    [Fact]
    public void WithFallbackResolverIsUsedWhenNoTypeIsRegistered()
    {
        var fallback = new AlwaysResolvePropertyUpdateNotifyResolver();
        var binder = new DataBinderBuilder().WithFallbackResolver(fallback).Build();

        var source = new PlainObject { Number = 5 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(5, target.Number);
        Assert.Equal(2, fallback.Calls);
    }

    [Fact]
    public void WithFallbackResolverReplacesTheDefaultNotifyPropertyChangedFallback()
    {
        var binder = new DataBinderBuilder().WithFallbackResolver(new NullPropertyUpdateNotifyResolver()).Build();

        // NotifyObject 本来能被默认兜底解析，换成 Null 解析器之后就不行了
        Assert.Throws<ArgumentException>(() =>
            binder.BindPropertyOneWay(new NotifyObject(), s => s.Number, new PlainObject(), t => t.Number));
    }

    [Fact]
    public void TheDefaultFallbackResolverHandlesNotifyPropertyChangedTypes()
    {
        var source = new NotifyObject { Number = 7 };
        var target = new PlainObject();

        using var handle = new DataBinderBuilder().Build().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(7, target.Number);
    }

    [Fact]
    public void TheDefaultFallbackResolverRejectsTypesThatDoNotNotify() =>
        Assert.Throws<ArgumentException>(() =>
            new DataBinderBuilder().Build().BindPropertyOneWay(new PlainObject(), s => s.Number, new PlainObject(), t => t.Number));

    [Fact]
    public void ResolverSelectionAlsoAppliesToTheTargetExpression()
    {
        var always = new AlwaysResolvePropertyUpdateNotifyResolver();
        var binder = new DataBinderBuilder().WithFallbackResolver(always).Build();

        var source = new PlainObject { Number = 4 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(4, target.Number);

        // 源、目标各解析一次
        Assert.Equal(2, always.Seen.Count);
        Assert.All(always.Seen, seen => Assert.Equal(nameof(PlainObject.Number), seen.MemberName));
    }

    [Fact]
    public void RegisteringTheSameTypeTwiceThrows()
    {
        var builder = new DataBinderBuilder()
                     .WithResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver());

        Assert.Throws<ArgumentException>(() =>
            builder.WithResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver()));
    }

    [Fact]
    public void BuildSnapshotsTheRegistrations()
    {
        var builder = new DataBinderBuilder().WithFallbackResolver(new NullPropertyUpdateNotifyResolver());
        var built = builder.Build();

        // Build 之后再登记，不该影响已经建出来的绑定器
        builder.WithResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver());

        Assert.Throws<ArgumentException>(() =>
            built.BindPropertyOneWay(new PlainObject(), s => s.Number, new PlainObject(), t => t.Number));
    }

    [Fact]
    public void BuilderCopiesShareTheSameRegistrationTable()
    {
        // DataBinderBuilder 是 readonly struct，注册表字段按引用拷贝：
        // with / WithXxx 得到的副本与原值看到的是同一张表——这既是链式写法能累积注册的原因，
        // 也意味着从"原值"建出来的绑定器同样能看到之后在副本上做的登记。
        var original = new DataBinderBuilder().WithFallbackResolver(new NullPropertyUpdateNotifyResolver());

        _ = original.WithResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver());

        var source = new PlainObject { Number = 1 };
        var target = new PlainObject();

        using var handle = original.Build().BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(1, target.Number);
    }
}
