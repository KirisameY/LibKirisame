using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding.Resolver;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.ResolverTests;

/// <summary>
///     <c>PropertyUpdateNotifyResolverBuilder</c>：按成员名手工登记解析方式；
///     没登记的名字返回 <c>null</c>，由绑定器的兜底路由接手。
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
                      .WithProperty("Number", (_, _) => { }, (_, _) => { })
                      .Build();

        // 传别的类型也照样命中——记录表只按成员名匹配
        Assert.NotNull(resolver.Resolve(typeof(PlainObject), PlainProp(nameof(PlainObject.Number))));
    }

    [Fact]
    public void UnregisteredNamesReturnsNull()
    {
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .WithProperty("Number", (_, _) => { }, (_, _) => { })
                      .Build();

        Assert.Null(resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text))));
    }

    [Fact]
    public void ResolvingWithANullMemberInfoReturnsNull()
    {
        var resolver = new PropertyUpdateNotifyResolverBuilder<NotifyObject>()
                      .Build();

        Assert.Null(resolver.Resolve(typeof(NotifyObject), null));
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
                     .WithProperty(nameof(NotifyObject.Number), (_, _) => { }, (_, _) => { });
        var built = builder.Build();

        // Build 之后再登记，不该影响已经建出来的解析器
        builder.WithProperty(nameof(NotifyObject.Text), (_, _) => { }, (_, _) => { });

        Assert.NotNull(built.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number))));
        Assert.Null(built.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text))));
    }
}
