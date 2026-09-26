using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding.Resolver;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.ResolverTests;

/// <summary>
///     默认解析器：只认实现了 <see cref="System.ComponentModel.INotifyPropertyChanged"/> 的类型，
///     把成员名映射成 <c>PropertyChanged</c> 的通知名。
/// </summary>
public class DefaultPropertyUpdateNotifyResolverTests
{
    private sealed class DerivedNotifyObject : NotifyObject
    {
        public int Extra { get; set; }
    }

    private static DefaultPropertyUpdateNotifyResolver Resolver => DefaultPropertyUpdateNotifyResolver.Instance;

    private static PropertyInfo Prop(Type type, string name) => type.GetProperty(name)!;

    private static PropertyInfo NotifyProp(string name) => Prop(typeof(NotifyObject), name);

    [Fact]
    public void InstanceIsASingleton() =>
        Assert.Same(DefaultPropertyUpdateNotifyResolver.Instance, DefaultPropertyUpdateNotifyResolver.Instance);

    [Fact]
    public void ResolveReturnsNullForATypeThatDoesNotNotify() =>
        Assert.Null(Resolver.Resolve(typeof(PlainObject), Prop(typeof(PlainObject), nameof(PlainObject.Number))));

    [Fact]
    public void ResolveReturnsNullWhenTheMemberInfoIsNull()
    {
        // 数组元素的 IndexExpression.Indexer 就是 null
        Assert.Null(Resolver.Resolve(typeof(int[]), null));
        Assert.Null(Resolver.Resolve(typeof(NotifyObject), null));
    }

    [Fact]
    public void ResolveReturnsAProxyForAPropertyOnANotifyingType() =>
        Assert.NotNull(Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number))));

    [Fact]
    public void ResolveReturnsAProxyForAFieldOnANotifyingType() =>
        Assert.NotNull(Resolver.Resolve(typeof(NotifyObject), typeof(NotifyObject).GetField(nameof(NotifyObject.Field))));

    [Fact]
    public void ResolveReturnsAProxyForAPropertyInheritedFromANotifyingBaseType() =>
        Assert.NotNull(Resolver.Resolve(typeof(DerivedNotifyObject), Prop(typeof(DerivedNotifyObject), nameof(NotifyObject.Number))));

    [Fact]
    public void ResolveReturnsAProxyForAnIndexer() =>
        Assert.NotNull(Resolver.Resolve(typeof(NotifyObject), NotifyProp("Item")));

    [Fact]
    public void ResolvedProxyDeliversNotificationsForItsOwnProperty()
    {
        var obj = new NotifyObject();
        var proxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);
        var (subscribe, _) = proxy!.Value;

        int calls = 0;
        subscribe(obj, () => calls++);

        obj.Number = 1;

        Assert.Equal(1, calls);
    }

    [Fact]
    public void ResolvedProxyOnlyDeliversNotificationsForItsOwnProperty()
    {
        var obj = new NotifyObject();
        var numberProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        var textProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text)));
        Assert.NotNull(numberProxy);
        Assert.NotNull(textProxy);

        int numberCalls = 0;
        int textCalls = 0;
        numberProxy!.Value.SubscribeUpdate(obj, () => numberCalls++);
        textProxy!.Value.SubscribeUpdate(obj, () => textCalls++);

        obj.Number = 1;
        Assert.Equal(1, numberCalls);
        Assert.Equal(0, textCalls);

        obj.Text = "x";
        Assert.Equal(1, numberCalls);
        Assert.Equal(1, textCalls);
    }

    [Fact]
    public void EveryHandlerRegisteredForTheSamePropertyReceivesTheNotification()
    {
        var obj = new NotifyObject();
        var proxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);
        var (subscribe, _) = proxy!.Value;

        int firstCalls = 0;
        int secondCalls = 0;
        subscribe(obj, () => firstCalls++);
        subscribe(obj, () => secondCalls++);

        obj.Number = 1;

        Assert.Equal(1, firstCalls);
        Assert.Equal(1, secondCalls);
    }

    [Fact]
    public void ANullPropertyNameReachesEveryRegisteredHandler()
    {
        var obj = new NotifyObject();
        var numberProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        var textProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text)));
        Assert.NotNull(numberProxy);
        Assert.NotNull(textProxy);

        int numberCalls = 0;
        int textCalls = 0;
        numberProxy!.Value.SubscribeUpdate(obj, () => numberCalls++);
        textProxy!.Value.SubscribeUpdate(obj, () => textCalls++);

        // PropertyName 为 null 是"全部属性都变了"的通行约定
        obj.Raise(null);

        Assert.Equal(1, numberCalls);
        Assert.Equal(1, textCalls);
    }

    [Fact]
    public void AnEmptyPropertyNameReachesEveryRegisteredHandler()
    {
        var obj = new NotifyObject();
        var numberProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        var textProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text)));
        Assert.NotNull(numberProxy);
        Assert.NotNull(textProxy);

        int numberCalls = 0;
        int textCalls = 0;
        numberProxy!.Value.SubscribeUpdate(obj, () => numberCalls++);
        textProxy!.Value.SubscribeUpdate(obj, () => textCalls++);

        // 空字符串是同一个约定的另一种常见写法
        obj.Raise("");

        Assert.Equal(1, numberCalls);
        Assert.Equal(1, textCalls);
    }

    [Fact]
    public void ResolvedProxyForAnIndexerListensToTheBracketedName()
    {
        var obj = new NotifyObject();
        var proxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp("Item"));
        Assert.NotNull(proxy);
        var (subscribe, _) = proxy!.Value;

        int calls = 0;
        subscribe(obj, () => calls++);

        // NotifyObject 的索引器 Setter 发的正是 "Item[]"
        obj["k"] = 1;

        Assert.Equal(1, calls);
    }

    [Fact]
    public void UnsubscribeForAnInstanceThatWasNeverSubscribedIsIgnored()
    {
        var proxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);

        // 从未订阅过就退订，不该抛异常
        proxy!.Value.UnsubscribeUpdate(new NotifyObject(), () => { });
    }

    // ---------- 通知过程中的再入：handler 在收到通知时增删订阅 ----------
    // HandlerRecord 内部用 ImmutableDictionary / ImmutableList，每次增删都换一份新快照，
    // 而 observer 每轮只读一次字段——所以本轮遍历看到的是稳定视图，
    // "收到通知 → 顺手退订自己"不会打断遍历。
    // 典型场景：某个 ViewModel 在属性变化里判断出实体已死，于是 Dispose 掉自己的订阅。

    [Fact]
    public void AHandlerMayUnsubscribeItselfDuringANamedNotification()
    {
        var obj = new NotifyObject();
        var proxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);
        var (subscribe, unsubscribe) = proxy!.Value;

        int calls = 0;
        Action handler = null!; // 自引用：handler 内部要退订自己
        handler = () =>
        {
            calls++;
            unsubscribe(obj, handler);
        };
        subscribe(obj, handler);

        obj.Number = 1; // 通知过程中退订自己，不该抛异常

        Assert.Equal(1, calls);

        obj.Number = 2; // 退订已生效，不再收到
        Assert.Equal(1, calls);

        // 引用计数回到 0 又挂回来之后，重新订阅应该照常工作
        int recalls = 0;
        subscribe(obj, () => recalls++);

        obj.Number = 3;
        Assert.Equal(1, recalls);
    }

    [Fact]
    public void AHandlerMayUnsubscribeAnotherHandlerDuringANamedNotification()
    {
        var obj = new NotifyObject();
        var proxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        Assert.NotNull(proxy);
        var (subscribe, unsubscribe) = proxy!.Value;

        int firstCalls = 0;
        int secondCalls = 0;
        Action second = () => secondCalls++;
        Action first = () =>
        {
            firstCalls++;
            unsubscribe(obj, second); // 移除同一属性上的另一个 handler
        };
        subscribe(obj, first);
        subscribe(obj, second);

        obj.Number = 1; // 不该抛异常

        Assert.Equal(1, firstCalls);

        // 退订只影响下一轮：本轮遍历用的是已经取到的快照，里面还有 second。
        // 这里只保证"之后不再收到"，本轮的行为不做约定。
        var secondCallsAfterFirstRound = secondCalls;

        obj.Number = 2;
        Assert.Equal(2, firstCalls);
        Assert.Equal(secondCallsAfterFirstRound, secondCalls); // 已退订，之后不会再收到
    }

    [Fact]
    public void AHandlerMayUnsubscribeItselfDuringAGlobalNotification()
    {
        var obj = new NotifyObject();
        var numberProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        var textProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text)));
        Assert.NotNull(numberProxy);
        Assert.NotNull(textProxy);

        int numberCalls = 0;
        int textCalls = 0;
        Action numberHandler = null!;
        numberHandler = () =>
        {
            numberCalls++;
            numberProxy!.Value.UnsubscribeUpdate(obj, numberHandler);
        };
        numberProxy!.Value.SubscribeUpdate(obj, numberHandler);
        textProxy!.Value.SubscribeUpdate(obj, () => textCalls++);

        obj.Raise(null); // 全局通知遍历到一半退订自己，不该抛异常

        Assert.Equal(1, numberCalls);
        Assert.Equal(1, textCalls);

        numberCalls = 0;
        obj.Raise(null);
        Assert.Equal(0, numberCalls); // 退订已生效
    }

    [Fact]
    public void AHandlerMaySubscribeAnotherPropertyDuringAGlobalNotification()
    {
        var obj = new NotifyObject();
        var numberProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Number)));
        var textProxy = Resolver.Resolve(typeof(NotifyObject), NotifyProp(nameof(NotifyObject.Text)));
        Assert.NotNull(numberProxy);
        Assert.NotNull(textProxy);

        int numberCalls = 0;
        int textCalls = 0;
        Action textHandler = () => textCalls++;
        numberProxy!.Value.SubscribeUpdate(obj, () =>
        {
            numberCalls++;
            textProxy!.Value.SubscribeUpdate(obj, textHandler); // 通知过程中登记新订阅
        });

        obj.Raise(null); // 不该抛异常

        Assert.Equal(1, numberCalls);

        // 新订阅进的是下一轮的快照，本轮遍历用的还是旧的那一份，
        // 所以这里只断言"订阅确实生效"，本轮的行为不做约定。
        obj.Text = "x";
        Assert.True(textCalls >= 1, "通知过程中登记的订阅应当生效");
    }
}
