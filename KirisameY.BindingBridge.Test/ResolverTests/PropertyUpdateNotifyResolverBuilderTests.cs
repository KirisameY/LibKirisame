using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding.Resolver;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.ResolverTests;

/// <summary>
///     <c>PropertyUpdateNotifyResolverBuilder</c>：按成员名手工登记解析方式，未登记的名字落到兜底。
/// </summary>
public class PropertyUpdateNotifyResolverBuilderTests
{
    private static PropertyInfo NotifyProp(string name) => typeof(NotifyObject).GetProperty(name)!;

    private static PropertyInfo PlainProp(string name) => typeof(PlainObject).GetProperty(name)!;

    [Fact]
    public void RegisteredNameResolvesToTheGivenSubscribeAndUnsubscribe()
    {
        List<Action> subscribed = [];
        List<Action> unsubscribed = [];
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithProperty(
                           nameof(NotifyObject.Number),
                           (_, handler) => subscribed.Add(handler),
                           (_, handler) => unsubscribed.Add(handler)
                       )
                      .Build();

        var proxy = resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);
        var (subscribe, unsubscribe) = proxy!.Value;

        Action handler = () => { };
        var obj = new NotifyObject();
        subscribe(obj, handler);
        unsubscribe(obj, handler);

        Assert.Same(handler, Assert.Single(subscribed));
        Assert.Same(handler, Assert.Single(unsubscribed));
    }

    [Fact]
    public void TheObjectIsCastToTheBuilderTypeBeforeBeingHandedToTheDelegates()
    {
        object? seen = null;
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithProperty(nameof(NotifyObject.Number), (o, _) => seen = o, (_, _) => { })
                      .Build();

        var proxy = resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);

        var obj = new NotifyObject();
        proxy!.Value.SubscribeUpdate(obj, () => { });

        Assert.Same(obj, seen);
    }

    [Fact]
    public void RecordedNamesAreMatchedByMemberNameOnlyAndIgnoreTheType()
    {
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithFallback(new NullPropertyUpdateNotifyResolver())
                      .WithProperty("Number", (_, _) => { }, (_, _) => { })
                      .Build();

        // 传别的类型也照样命中——记录表只按成员名匹配
        Assert.NotNull(resolver.Resolve(typeof(PlainObject), PlainProp(nameof(PlainObject.Number))));
    }

    [Fact]
    public void UnregisteredNamesFallThroughToTheFallback()
    {
        var fallback = new NullPropertyUpdateNotifyResolver();
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithFallback(fallback)
                      .WithProperty("Number", (_, _) => { }, (_, _) => { })
                      .Build();

        Assert.Null(resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text))));
        Assert.Single(fallback.Seen);
    }

    [Fact]
    public void ResolvingWithANullMemberInfoFallsThroughToTheFallback()
    {
        var fallback = new NullPropertyUpdateNotifyResolver();
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithFallback(fallback)
                      .Build();

        Assert.Null(resolver.Resolve(typeof(NotifyObject), null));
        Assert.Single(fallback.Seen);
    }

    [Fact]
    public void TheDefaultFallbackIsTheNotifyPropertyChangedResolver()
    {
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>().Build();

        Assert.NotNull(resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text))));
        Assert.Null(resolver.Resolve(typeof(PlainObject), PlainProp(nameof(PlainObject.Number))));
    }

    [Fact]
    public void RegisteringTheSameNameTwiceThrows()
    {
        var builder = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                     .WithProperty("Number", (_, _) => { }, (_, _) => { });

        Assert.Throws<ArgumentException>(() => builder.WithProperty("Number", (_, _) => { }, (_, _) => { }));
    }

    [Fact]
    public void BuildSnapshotsTheRegistrations()
    {
        var builder = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                     .WithFallback(new NullPropertyUpdateNotifyResolver())
                     .WithProperty(nameof(NotifyObject.Number), (_, _) => { }, (_, _) => { });
        var built = builder.Build();

        // Build 之后再登记，不该影响已经建出来的解析器
        builder.WithProperty(nameof(NotifyObject.Text), (_, _) => { }, (_, _) => { });

        Assert.NotNull(built.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number))));
        Assert.Null(built.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text))));
    }

    [Fact]
    public void BuilderCopiesShareTheSameRegistrationTable()
    {
        // PropertyUpdateNotifyResolverBuilder 是 readonly struct，注册表字段按引用拷贝：
        // with / WithXxx 得到的副本与原值看到的是同一张表——这既是链式写法能累积注册的原因，
        // 也意味着"原值"会看到之后在副本上做的登记。
        var original = new PropertyUpdateNotifyResolverBuilder<NotifyObject>().WithFallback(new NullPropertyUpdateNotifyResolver());

        _ = original.WithProperty(nameof(NotifyObject.Number), (_, _) => { }, (_, _) => { });

        Assert.NotNull(original.Build().Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number))));
    }
}
