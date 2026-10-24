using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ObservableTests;

/// <summary>
///     可观测包装器的缓存：同一个源、同一个有效阈值返回同一个实例。
///     负阈值在进缓存前统一折算成 <c>-1</c>，所以 -1 / -3 / 默认参数拿到的是同一个视图。
/// </summary>
public class ObservableWrapperCacheTests
{
    [Fact]
    public void CollectionObservableViewIsCachedPerThreshold()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;

        var defaultView = source.AsReadOnlyObservableCollection();
        var unlimitedView = source.AsReadOnlyObservableCollection(-1);
        var zeroView = source.AsReadOnlyObservableCollection(0);

        Assert.Same(defaultView, unlimitedView);
        Assert.Same(zeroView, source.AsReadOnlyObservableCollection(0));
        Assert.NotSame(defaultView, zeroView);
    }

    [Fact]
    public void NegativeThresholdsAllShareTheUnlimitedView()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;

        var minusOne = source.AsReadOnlyObservableCollection(-1);
        var minusThree = source.AsReadOnlyObservableCollection(-3);

        Assert.Same(minusOne, minusThree);
    }

    [Fact]
    public void ListObservableViewIsCachedPerThreshold()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableList<int> source = list;

        var first = source.AsReadOnlyObservableCollection(2);
        var second = source.AsReadOnlyObservableCollection(2);
        var otherThreshold = source.AsReadOnlyObservableCollection(3);

        Assert.Same(first, second);
        Assert.NotSame(first, otherThreshold);
    }

    [Fact]
    public void DictionaryObservableViewIsCachedPerThreshold()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        IReadOnlyNotifiableDictionary<string, int> source = dictionary;

        var defaultView = source.AsReadOnlyObservableDictionary();
        var negativeView = source.AsReadOnlyObservableDictionary(-5);
        var resetView = source.AsReadOnlyObservableDictionary(0);

        Assert.Same(defaultView, negativeView);
        Assert.NotSame(defaultView, resetView);
    }

    [Fact]
    public void CachedObservableCollectionForwardsEachChangeOnce()
    {
        NotifiableList<int> list = [];
        IReadOnlyNotifiableCollection<int> source = list;
        var first = source.AsReadOnlyObservableCollection();
        var second = source.AsReadOnlyObservableCollection();
        Assert.Same(first, second);

        var events = ObservableTestUtils.RecordCollectionChanged(second);
        list.Add(1);

        // 缓存命中不应当让源事件被重复订阅
        Assert.Single(events);
    }
}
