using System.Linq.Expressions;

using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.Test.TestDoubles;

namespace KirisameY.BindingBridge.Test.BinderTests;

/// <summary>
///     覆盖 <c>DataBinderBase.ResolveProperty</c> 对表达式树形状的分支：
///     属性 / 字段 / 索引器 / 数组元素分别怎么解析，以及各种非法形状报什么错。
/// </summary>
/// <remarks>
///     实测：C# 编译器为元素访问生成的节点并不是 <c>IndexExpression</c>——
///     <list type="bullet">
///         <item>类 / List 的索引器 <c>x =&gt; x["k"]</c> → <c>MethodCallExpression</c>（直接调用 get 访问器）</item>
///         <item>一维数组 <c>x =&gt; x[1]</c> → <c>BinaryExpression</c>，<c>NodeType == ExpressionType.ArrayIndex</c></item>
///         <item>多维数组 <c>x =&gt; x[1, 2]</c> → <c>MethodCallExpression</c></item>
///     </list>
///     所以 <c>ResolveProperty</c> 得把上面几种形状分别识别出来，再统一归一化成
///     <c>IndexExpression</c>（数组和索引器都走 <c>Expression.MakeIndex</c>）交给 <c>Expression.Assign</c> 生成 setter。
///     那个 <c>IndexExpression</c> 分支则是留给手工构造的表达式树的。
/// </remarks>
public class PropertyExpressionResolutionTests
{
    private static IDataBinder Binder() => new DataBinderBuilder().Build();

    private static NotifyObject SourceOf(int number) => new() { Number = number };

    // ---------- 可以解析的形状 ----------

    [Fact]
    public void APropertyResolvesOnBothSides()
    {
        var target = new PlainObject();

        using var handle = Binder().BindPropertyOneWay(SourceOf(5), s => s.Number, target, t => t.Number);

        Assert.Equal(5, target.Number);
    }

    [Fact]
    public void AReferenceTypePropertyResolvesOnBothSides()
    {
        var target = new PlainObject();

        using var handle = Binder().BindPropertyOneWay(new NotifyObject { Text = "abc" }, s => s.Text, target, t => t.Text);

        Assert.Equal("abc", target.Text);
    }

    [Fact]
    public void AFieldResolvesOnBothSides()
    {
        var target = new PlainObject();

        using var handle = Binder().BindPropertyOneWay(new NotifyObject { Field = 4 }, s => s.Field, target, t => t.Field);

        Assert.Equal(4, target.Field);
    }

    // ---------- 元素访问：几种节点形状见类注释里的实测 ----------

    [Fact]
    public void AnIndexerResolvesOnBothSides()
    {
        var source = new NotifyObject();
        source["k"] = 6;
        var target = new PlainObject();

        using var handle = Binder().BindPropertyOneWay(source, s => s["k"], target, t => t["k"]);

        Assert.Equal(6, target["k"]);
    }

    [Fact]
    public void AnArrayElementResolvesAsATarget()
    {
        // 数组的 IndexExpression.Indexer 是 null，不可能有通知，因此只可能是可写端点
        var target = new int[3];

        using var handle = Binder().BindPropertyOneWay(SourceOf(5), s => s.Number, target, a => a[1]);

        Assert.Equal(5, target[1]);
    }

    [Fact]
    public void AListElementResolvesAsATarget()
    {
        var target = new List<int> { 0, 0, 0 };

        using var handle = Binder().BindPropertyOneWay(SourceOf(5), s => s.Number, target, l => l[2]);

        Assert.Equal(5, target[2]);
    }

    // ---------- 不可写的目标 ----------

    [Fact]
    public void AGetterOnlyTargetPropertyIsRejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(SourceOf(5), s => s.Number, new PlainObject(), t => t.ReadOnlyNumber));

        Assert.Equal("targetProperty is not writable.", ex.Message);
    }

    [Fact]
    public void AReadOnlyFieldTargetIsRejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(SourceOf(5), s => s.Number, new NotifyObject(), t => t.ReadOnlyField));

        Assert.Equal("targetProperty is not writable.", ex.Message);
    }

    // ---------- 不可观察的源 ----------

    [Fact]
    public void ASourceThatDoesNotNotifyIsRejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(new PlainObject(), s => s.Number, new PlainObject(), t => t.Number));

        Assert.Equal("sourceProperty is not observable.", ex.Message);
    }

    [Fact]
    public void AReadOnlySourceIsRejectedForTwoWayBinding()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyTwoWay(new NotifyObject(), s => s.ReadOnlyNumber, new NotifyObject(), t => t.Number));

        Assert.Equal("sourceProperty is not writable or not observable.", ex.Message);
    }

    [Fact]
    public void ATargetThatDoesNotNotifyIsRejectedForTwoWayBinding()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyTwoWay(new NotifyObject(), s => s.Number, new PlainObject(), t => t.Number));

        Assert.Equal("targetProperty is not writable or not observable.", ex.Message);
    }

    // ---------- 表达式树形状不合法 ----------

    [Fact]
    public void AStaticMemberIsRejected()
    {
        Expression<Func<PlainObject, int>> staticMember = _ => StaticHolder.Value;

        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(SourceOf(1), s => s.Number, new PlainObject(), staticMember));

        Assert.Equal($"Expression is not from an instance of type <{typeof(PlainObject)}>.", ex.Message);
    }

    [Fact]
    public void AMemberOfACapturedVariableIsRejected()
    {
        var captured = new NotifyObject();
        Expression<Func<PlainObject, int>> capturedMember = _ => captured.Number;

        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(SourceOf(1), s => s.Number, new PlainObject(), capturedMember));

        Assert.Equal($"Expression is not from an instance of type <{typeof(PlainObject)}>.", ex.Message);
    }

    [Fact]
    public void AMethodCallIsRejected()
    {
        Expression<Func<PlainObject, int>> methodCall = t => t.GetHashCode();

        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(SourceOf(1), s => s.Number, new PlainObject(), methodCall));

        Assert.Equal("Expression is neither a property, field, nor indexer with constant index.", ex.Message);
    }

    [Fact]
    public void AConstantIsRejected()
    {
        Expression<Func<PlainObject, int>> constant = _ => 5;

        var ex = Assert.Throws<ArgumentException>(() =>
            Binder().BindPropertyOneWay(SourceOf(1), s => s.Number, new PlainObject(), constant));

        Assert.Equal("Expression is neither a property, field, nor indexer with constant index.", ex.Message);
    }
}
