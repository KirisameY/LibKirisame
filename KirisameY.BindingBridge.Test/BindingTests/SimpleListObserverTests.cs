using System.Collections.ObjectModel;

using KirisameY.BindingBridge.Binder;
using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;
using KirisameY.BindingBridge.Test.TestDoubles;
using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.Test.BindingTests;

/// <summary>
///     自定义列表观察者（<see cref="ISimpleListObserver{TItem}"/>）当绑定目标：
///     消费端只要实现这一组"改一下自己的东西"的方法，就能接住列表源的全部变更。
/// </summary>
/// <remarks>
///     <para>
///         观察者怎么实现是消费端自己的事，所以这里只钉<b>结果</b>：
///         把收到的调用依序照做一遍之后，观察者的内容必须与源一致。
///         测试替身内部就是一份真列表，因此直接比对它就够了。
///     </para>
///     <para>
///         索引的两种来源都覆盖：列表源（<see cref="ObservableCollection{T}"/> 这类）的通知带索引，
///         应当走 <c>Insert</c> / <c>RemoveAt</c> / <c>Replace(index, …)</c>；
///         非列表的集合源带不了索引，只能先 <c>Find</c> 定位再照做。
///     </para>
/// </remarks>
public class SimpleListObserverTests
{
    private static IDataBinder DefaultBinder() => new DataBinderBuilder().Build();


    // ---------- 列表源：通知带索引 ----------

    [Fact]
    public void TheObserverStartsOutWithTheSourceContents()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        Assert.Equal([1, 2, 3], target.Items);
    }

    [Fact]
    public void AnItemAppendedToTheSourceIsAppendedToTheObserver()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Add(4);

        Assert.Equal([1, 2, 3, 4], target.Items);
    }

    [Fact]
    public void AnItemInsertedInTheMiddleLandsAtTheSameIndex()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Insert(1, 9);

        // 插在中间而不是接到末尾——只照做不管位置的话这里会变成 [1, 2, 3, 9]
        Assert.Equal([1, 9, 2, 3], target.Items);
    }

    [Fact]
    public void RemovingAnItemShiftsTheRestTheSameWay()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.RemoveAt(1);

        Assert.Equal([1, 3], target.Items);
    }

    [Fact]
    public void ReplacingAnItemKeepsItInPlace()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source[1] = 9;

        Assert.Equal([1, 9, 3], target.Items);
    }

    [Fact]
    public void MovingAnItemMovesItToTheSamePlace()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Move(0, 2);

        Assert.Equal([2, 3, 1], target.Items);
    }

    [Fact]
    public void ClearingTheSourceClearsTheObserver()
    {
        var source = new ObservableCollection<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.Clear();

        Assert.Empty(target.Items);
    }

    [Fact]
    public void DisposingTheHandleStopsThePropagation()
    {
        var source = new ObservableCollection<int> { 1 };
        var target = new SimpleListObserverTarget();

        var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        handle.Dispose();

        source.Add(2);

        Assert.Equal([1], target.Items);
    }


    // ---------- 集合源：通知不带索引，得靠观察者自己 Find ----------

    [Fact]
    public void ACollectionSourceWithoutIndexesStillFeedsTheObserver()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();

        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        Assert.Equal([1, 2, 3], target.Items.OrderBy(v => v));

        source.Add(4);

        // 没有索引可用，只能追加
        Assert.Equal([1, 2, 3, 4], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void RemovingFromACollectionSourceLetsTheObserverFindTheItemItself()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        target.Calls.Clear();

        source.Remove(2);

        Assert.Contains("Find(2)", target.Calls);
        // 找错了就会删掉别的项，所以这里比对内容而不是"有没有删"
        Assert.Equal([1, 3], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void ReplacingInACollectionSourceLetsTheObserverFindTheItemItself()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        target.Calls.Clear();

        source.Replace(2, 9);

        Assert.Contains("Find(2)", target.Calls);
        Assert.Equal([1, 3, 9], target.Items.OrderBy(v => v));
    }

    [Fact]
    public void AnItemTheObserverCannotFindIsSimplySkippedWhenRemoving()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new BlindListObserver();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        target.Calls.Clear();

        source.Remove(2);

        // 观察者说它不认识这一项：那就什么都不做，尤其不能拿着 -1 去 RemoveAt
        Assert.Contains("Find(2)", target.Calls);
        Assert.DoesNotContain(target.Calls, call => call.StartsWith("RemoveAt"));
    }

    [Fact]
    public void AnItemTheObserverCannotFindIsSimplySkippedWhenReplacing()
    {
        var source = new ObservableSet<int> { 1, 2, 3 };
        var target = new BlindListObserver();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());
        target.Calls.Clear();

        source.Replace(2, 9);

        Assert.Contains("Find(2)", target.Calls);
        Assert.DoesNotContain(target.Calls, call => call.StartsWith("Replace"));
    }


    // ---------- 一次带多项的通知 ----------

    [Fact]
    public void ABatchInsertLandsInTheObserverInSourceOrder()
    {
        var source = new RaisableObservableList<int>([1, 2, 3]);
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.InsertRange(1, [7, 8, 9]);

        // 一次带三项的通知：这些项该按源里的顺序落在同一个位置
        Assert.Equal(source, target.Items);
    }

    [Fact]
    public void ABatchReplaceLandsInTheObserverInSourceOrder()
    {
        var source = new RaisableObservableList<int>([1, 2, 3]);
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.ReplaceRange(1, [7, 8]);

        Assert.Equal(source, target.Items);
    }

    [Fact]
    public void ABatchRemoveTakesOutExactlyWhatTheSourceLost()
    {
        var source = new RaisableObservableList<int>([1, 2, 3, 4]);
        var target = new SimpleListObserverTarget();
        using var handle = DefaultBinder().BindCollection(source, target, TypeA.Of<int>());

        source.RemoveRange(1, 2);

        Assert.Equal(source, target.Items);
    }
}
