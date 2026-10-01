using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.PropertyBinding.Resolver;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BinderTests;

/// <summary>
///     <c>DataBinderBuilder</c> + <c>RegistryDataBinder</c>：按类型挑解析器，
///     先精确匹配声明类型，再退到泛型定义、沿 <c>BaseType</c> 向上找。
///     挑中的解析器没认领的成员，以及一个类型都没挑中时，都由绑定器的兜底路由接手。
/// </summary>
public class DataBinderBuilderTests
{
    private class BaseSource
    {
        public int Number { get; set; }
    }

    private sealed class DerivedSource : BaseSource;

    /// <summary>自带事件通知的泛型源，注册表里登记的键是开放泛型定义 <c>GenericSource&lt;&gt;</c>。</summary>
    private class GenericSource<T>
    {
        public event Action? Changed;

        private int _number;

        public int Number
        {
            get => _number;
            set
            {
                if (_number == value) return;
                _number = value;
                Changed?.Invoke();
            }
        }
    }

    private sealed class DerivedGenericSource<T> : GenericSource<T>;

    [Fact]
    public void WithResolverMakesANonNotifyingTypeBindable()
    {
        var binder = new DataBinderBuilder()
                    .WithPropertyResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver())
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
                    .WithPropertyResolver(typeof(BaseSource), new AlwaysResolvePropertyUpdateNotifyResolver())
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
        var binder = new DataBinderBuilder().WithPropertyResolver(typeof(BaseSource), registered).Build();

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
                    .WithPropertyResolver(typeof(BaseSource), baseResolver)
                    .WithPropertyResolver(typeof(DerivedSource), derivedResolver)
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
                    .WithPropertyResolver(typeof(DerivedSource), derivedResolver)
                    .WithPropertyFallbackResolver(new NullPropertyUpdateNotifyResolver())
                    .Build();

        var source = new BaseSource { Number = 3 };

        Assert.Throws<ArgumentException>(() =>
            binder.BindPropertyOneWay(source, s => s.Number, new PlainObject(), t => t.Number));

        // 声明类型是 BaseSource，DerivedSource 上的登记不该被问到
        Assert.DoesNotContain(derivedResolver.Seen, seen => seen.Type == typeof(BaseSource));
    }

    [Fact]
    public void AResolverRegisteredForAnOpenGenericTypeServesItsConstructedInstances()
    {
        // 登记的是开放泛型定义，绑定时的声明类型是构造后的 GenericSource<int>
        var resolver = new PropertyUpdateNotifyResolverBuilder<GenericSource<int>>()
                      .WithProperty(nameof(GenericSource<int>.Number), (o, h) => o.Changed += h, (o, h) => o.Changed -= h)
                      .Build();
        var binder = new DataBinderBuilder().WithPropertyResolver(typeof(GenericSource<>), resolver).Build();

        var source = new GenericSource<int> { Number = 3 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(3, target.Number);

        source.Number = 8;
        Assert.Equal(8, target.Number);
    }

    [Fact]
    public void AnOpenGenericRegistrationIsAlsoFoundThroughAGenericBaseType()
    {
        var resolver = new PropertyUpdateNotifyResolverBuilder<DerivedGenericSource<int>>()
                      .WithProperty(nameof(GenericSource<int>.Number), (o, h) => o.Changed += h, (o, h) => o.Changed -= h)
                      .Build();
        var binder = new DataBinderBuilder().WithPropertyResolver(typeof(GenericSource<>), resolver).Build();

        var source = new DerivedGenericSource<int> { Number = 4 };
        var target = new PlainObject();

        using var handle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);

        Assert.Equal(4, target.Number);

        source.Number = 6;
        Assert.Equal(6, target.Number);
    }

    [Fact]
    public void WithFallbackResolverIsUsedWhenNoTypeIsRegistered()
    {
        // 刻意只设兜底、一个类型都不登记：ManualNotifySource 不是 INotifyPropertyChanged，
        // 它能被绑上、还能收到后续变更，就说明兜底确实被选中了。
        var binder = new DataBinderBuilder()
                    .WithPropertyFallbackResolver(ManualNotifySource.CreateResolver())
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
        var binder = new DataBinderBuilder().WithPropertyFallbackResolver(new NullPropertyUpdateNotifyResolver()).Build();

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
    public void AMemberTheRegisteredResolverDidNotClaimFallsThroughToTheBindersFallback()
    {
        // 类型级解析器只登记了 Number；Text 没人认领，
        // 它不会卡在类型级解析器那里，而是落到绑定器的兜底路由（默认那套 INotifyPropertyChanged）。
        var binder = new DataBinderBuilder()
                    .WithPropertyResolver(typeof(SplitNotifySource), SplitNotifySource.NumberResolver())
                    .Build();

        var source = new SplitNotifySource { Number = 3, Text = "a" };
        var target = new PlainObject();

        using var numberHandle = binder.BindPropertyOneWay(source, s => s.Number, target, t => t.Number);
        using var textHandle   = binder.BindPropertyOneWay(source, s => s.Text, target, t => t.Text);

        Assert.Equal(3, target.Number);
        Assert.Equal("a", target.Text);

        source.Number = 5; // 只会响 NumberChanged
        source.Text   = "b"; // 只会响 PropertyChanged

        Assert.Equal(5, target.Number);
        Assert.Equal("b", target.Text);
    }

    [Fact]
    public void AResolverDoesNotCarryItsOwnFallbackSoTheBindersOneDecides()
    {
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithProperty(nameof(NotifyObject.Number), (_, _) => { }, (_, _) => { })
                      .Build();

        // NotifyObject.Text 没被登记：兜底换成什么都不认的解析器之后，它就绑不上了——
        // 说明接手的是绑定器的兜底，而不是类型级解析器自带的那套 NotifyPropertyChanged 默认。
        var nullFallback = new DataBinderBuilder()
                          .WithPropertyResolver(typeof(NotifyObject), resolver)
                          .WithPropertyFallbackResolver(new NullPropertyUpdateNotifyResolver())
                          .Build();

        Assert.Throws<ArgumentException>(() =>
            nullFallback.BindPropertyOneWay(new NotifyObject(), s => s.Text, new PlainObject(), t => t.Text));

        // 换回默认兜底，同一个成员就又绑得上、而且真的会跟着变。
        var source = new NotifyObject { Text = "a" };
        var target = new PlainObject();

        using var handle = new DataBinderBuilder()
                          .WithPropertyResolver(typeof(NotifyObject), resolver)
                          .WithPropertyFallbackResolver(DefaultPropertyUpdateNotifyResolver.Instance)
                          .Build()
                          .BindPropertyOneWay(source, s => s.Text, target, t => t.Text);

        Assert.Equal("a", target.Text);

        source.Text = "b";
        Assert.Equal("b", target.Text);
    }

    [Fact]
    public void ResolverSelectionAlsoAppliesToTheTargetExpression()
    {
        // 目标侧的表达式同样要走解析器选择：双向绑定要求目标也能解析出"可观察"端点，
        // 而 ManualNotifySource 不是 INotifyPropertyChanged，全靠兜底。
        var binder = new DataBinderBuilder()
                    .WithPropertyFallbackResolver(ManualNotifySource.CreateResolver())
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
                     .WithPropertyResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver());

        Assert.Throws<ArgumentException>(() =>
            builder.WithPropertyResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver()));
    }

    [Fact]
    public void BuildSnapshotsTheRegistrations()
    {
        var builder = new DataBinderBuilder().WithPropertyFallbackResolver(new NullPropertyUpdateNotifyResolver());
        var built = builder.Build();

        // Build 之后再登记，不该影响已经建出来的绑定器
        builder.WithPropertyResolver(typeof(PlainObject), new AlwaysResolvePropertyUpdateNotifyResolver());

        Assert.Throws<ArgumentException>(() =>
            built.BindPropertyOneWay(new PlainObject(), s => s.Number, new PlainObject(), t => t.Number));
    }
}
