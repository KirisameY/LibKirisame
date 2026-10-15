using System.Collections.ObjectModel;

using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;
using KirisameY.BindingBridge.Test.TestDoubles;
using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     自定义集合观察者（<see cref="ISimpleCollectionObserver{TElement}"/>）当绑定目标：
///     不在乎顺序的容器只要实现这一组按值操作的方法，就能接住集合源的变更。
/// </summary>
/// <remarks>
///     <para>
///         和列表版一样只钉<b>结果</b>：把收到的调用依序照做一遍之后，观察者的内容必须与源一致。
///         区别在于这一族全是"按值"的，没有索引，所以源那边是列表还是集合都无所谓。
///     </para>
///     <para>
///         无序容器不承诺顺序，比对内容一律先排序。
///     </para>
/// </remarks>
public class SimpleCollectionObserverTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();

    [Fact]
    public void TheObserverStartsOutWithTheSourceContents()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal([1, 2, 3], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void AnItemAddedToTheSourceIsAddedToTheObserver()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Add(4);

        Assert.Equal([1, 2, 3, 4], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void AnItemRemovedFromTheSourceIsRemovedFromTheObserver()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Remove(2);

        Assert.Equal([1, 3], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void AReplacedItemIsReplacedInTheObserver()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Replace(2, 9);

        // 替换是一个动作，不该落成"旧的没了新的也在"这种中间态
        Assert.Equal([1, 3, 9], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void ClearingTheSourceClearsTheObserver()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Clear();

        Assert.Empty(target.Items);
    }

    [Fact]
    public void ALongerListSourceCanFeedAnUnorderedObserver()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        Assert.Equal([1, 2, 3], target.Items.OrderBy(v => v));

        source.Add(4);
        source.Remove(2);
        source[0] = 9;

        Assert.Equal([3, 4, 9], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void MovingInAListSourceDoesNotDisturbAnUnorderedObserver()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleCollectionObserverTarget();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        // 无序观察者没有"挪"这个概念，位置变化与它无关，内容应当原样不动
        source.Move(0, 2);

        Assert.Equal([1, 2, 3], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void DisposingTheHandleStopsThePropagation()
    {
        var source = new ObservableSet<int> { 1 };
        var target = new SimpleCollectionObserverTarget();

        var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        handle.Dispose();

        source.Add(2);

        Assert.Equal([1], target.Items.OrderBy(v => v));
    }
}
