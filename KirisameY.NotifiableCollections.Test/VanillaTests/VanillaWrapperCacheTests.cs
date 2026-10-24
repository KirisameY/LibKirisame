using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.VanillaTests;

/// <summary>
///     标准库通知模型到本库模型的包装器也按源缓存：
///     同一个 <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> 集合重复包装应拿回同一个视图，
///     避免每次调用都往源集合上多挂一份订阅。
/// </summary>
public class VanillaWrapperCacheTests
{
    [Fact]
    public void NotifiableCollectionWrapperIsCachedPerSource()
    {
        var source = new FakeNotifyCollection<int>();

        var first = source.AsNotifiableCollection(TypeA.Of<int>());
        var second = source.AsNotifiableCollection(TypeA.Of<int>());

        Assert.Same(first, second);
    }

    [Fact]
    public void NotifiableListWrapperIsCachedPerSource()
    {
        var source = new RaisableObservableCollection<int>();

        var first = source.AsNotifiableList(TypeA.Of<int>());
        var second = source.AsNotifiableList(TypeA.Of<int>());

        Assert.Same(first, second);
    }

    [Fact]
    public void NotifiableDictionaryWrapperIsCachedPerSource()
    {
        var source = new FakeNotifyDictionary<string, int>();

        var first = source.AsNotifiableDictionary(TypeA.Of<string>(), TypeA.Of<int>());
        var second = source.AsNotifiableDictionary(TypeA.Of<string>(), TypeA.Of<int>());

        Assert.Same(first, second);
    }

    [Fact]
    public void DifferentSourcesGetDifferentWrappers()
    {
        var firstSource = new FakeNotifyCollection<int>();
        var secondSource = new FakeNotifyCollection<int>();

        Assert.NotSame(
            firstSource.AsNotifiableCollection(TypeA.Of<int>()),
            secondSource.AsNotifiableCollection(TypeA.Of<int>())
        );
    }

    [Fact]
    public void CachedWrapperForwardsEachChangeOnce()
    {
        var source = new RaisableObservableCollection<int>();
        var first = source.AsNotifiableList(TypeA.Of<int>());
        var second = source.AsNotifiableList(TypeA.Of<int>());
        Assert.Same(first, second);

        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(second);
        source.Add(1);

        // 缓存命中不应当让源事件被重复订阅
        Assert.Single(updates);
    }
}
