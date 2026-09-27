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

        // 沿 BaseType 找到了 BaseSource 的解析器，但传进去的仍是原始的声明类型。
        // 只断言"被问了什么"，不断言被问了几次——调用次数属于库的内部实现（比如缓存）。
        Assert.Contains(
            registered.Seen, seen => seen.Type == typeof(DerivedSource) && seen.MemberName == nameof(BaseSource.Number)
        );
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

        Assert.Equal(3, target.Number);

        // 声明类型是 DerivedSource：登记的 DerivedSource 解析器被问了，BaseSource 那个没被问
        Assert.Contains(derivedResolver.Seen, seen => seen.Type == typeof(DerivedSource));
        Assert.DoesNotContain(baseResolver.Seen, seen => seen.Type == typeof(DerivedSource));
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

        // 声明类型是 BaseSource，DerivedSource 上的登记不该被问到
        Assert.DoesNotContain(derivedResolver.Seen, seen => seen.Type == typeof(BaseSource));
    }

    [Fact]
    public void WithFallbackResolverIsUsedWhenNoTypeIsRegistered()
    {
        // 刻意只设兜底、一个类型都不登记：ManualNotifySource 不是 INotifyPropertyChanged，
        // 它能被绑上、还能收到后续变更，就说明兜底确实被选中了。
        var binder = new DataBinderBuilder()
                    .WithFallbackResolver(ManualNotifySource.CreateResolver())
                    .Build();

        var source = new ManualNotifySource { Number = 5 };
        var target = new ManualNotifySource();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(5, target.Number);

        source.Number = 9;
        Assert.Equal(9, target.Number);
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
        // 目标侧的表达式同样要走解析器选择：双向绑定要求目标也能解析出"可观察"端点，
        // 而 ManualNotifySource 不是 INotifyPropertyChanged，全靠兜底。
        var binder = new DataBinderBuilder()
                    .WithFallbackResolver(ManualNotifySource.CreateResolver())
                    .Build();

        var source = new ManualNotifySource { Number = 4 };
        var target = new ManualNotifySource();

        using var handle = binder.BindPropertyTwoWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(4, target.Number);

        source.Number = 6;
        Assert.Equal(6, target.Number); // 源 → 目标

        // 目标 → 源：只有目标侧也解析出了可观察端点，这个方向才可能成立
        target.Number = 8;
        Assert.Equal(8, source.Number);
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
}
