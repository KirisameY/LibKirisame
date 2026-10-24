using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

/// <summary>
///     只读包装器的缓存：同一个源集合重复包装应当拿回同一个视图。
///     否则每调用一次都会多订一份源事件，通知成倍发出，退订时也理不清是哪一份。
/// </summary>
/// <remarks>
///     <para>
///         参数版（投影）不参与缓存：投影委托每次都可以不同，无法按源定位同一份视图，
///         所以它每次返回新的活视图。可缓存的只有无参版本与 <see cref="TypeA{T}"/> 上转型版本。
///     </para>
///     <para>
///         The read-only wrappers are interned per source: wrapping the same source twice returns the same view.
///         Without this, every call would subscribe to the source again, multiplying notifications and making
///         unsubscription ambiguous. The projecting overload is deliberately not interned, because each call may pass
///         a different delegate; it creates a new live view every time.
///     </para>
/// </remarks>
public class WrapCacheTests
{
    [Fact]
    public void CollectionReadOnlyViewIsCachedPerSource()
    {
        NotifiableList<int> list = [1];
        IReadOnlyNotifiableCollection<int> source = list;

        var first = source.AsReadOnlyNotifiableCollection();
        var second = source.AsReadOnlyNotifiableCollection();

        Assert.Same(first, second);
    }

    [Fact]
    public void ListReadOnlyViewIsCachedPerSource()
    {
        NotifiableList<int> list = [1];
        IReadOnlyNotifiableList<int> source = list;

        var first = source.AsReadOnlyNotifiableList();
        var second = source.AsReadOnlyNotifiableList();

        Assert.Same(first, second);
    }

    [Fact]
    public void DictionaryReadOnlyViewIsCachedPerSource()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        IReadOnlyNotifiableDictionary<string, int> source = dictionary;

        var first = source.AsReadOnlyNotifiableDictionary();
        var second = source.AsReadOnlyNotifiableDictionary();

        Assert.Same(first, second);
    }

    [Fact]
    public void WrappingAnAlreadyReadOnlyViewReturnsItUnchanged()
    {
        NotifiableList<int> list = [1];
        IReadOnlyNotifiableCollection<int> collectionView = list.AsReadOnlyNotifiableCollection();
        IReadOnlyNotifiableList<int> listView = list.AsReadOnlyNotifiableList();
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        IReadOnlyNotifiableDictionary<string, int> dictionaryView = dictionary.AsReadOnlyNotifiableDictionary();

        // 幂等：视图再包一层还是它自己，不会套娃
        Assert.Same(collectionView, collectionView.AsReadOnlyNotifiableCollection());
        Assert.Same(listView, listView.AsReadOnlyNotifiableList());
        Assert.Same(dictionaryView, dictionaryView.AsReadOnlyNotifiableDictionary());
    }

    [Fact]
    public void DifferentSourcesGetDifferentReadOnlyViews()
    {
        IReadOnlyNotifiableCollection<int> firstSource = new NotifiableList<int> { 1 };
        IReadOnlyNotifiableCollection<int> secondSource = new NotifiableList<int> { 1 };

        var first = firstSource.AsReadOnlyNotifiableCollection();
        var second = secondSource.AsReadOnlyNotifiableCollection();

        Assert.NotSame(first, second);
    }

    [Fact]
    public void UpcastCollectionViewIsCachedPerSourceAndTargetType()
    {
        NotifiableList<string> list = ["a"];
        IReadOnlyNotifiableCollection<string> source = list;

        IReadOnlyNotifiableCollection<object> first = source.AsReadOnlyNotifiableCollection(TypeA.Of<object>());
        IReadOnlyNotifiableCollection<object> second = source.AsReadOnlyNotifiableCollection(TypeA.Of<object>());
        IReadOnlyNotifiableCollection<IComparable> otherTarget =
            source.AsReadOnlyNotifiableCollection(TypeA.Of<IComparable>());

        // 目标类型也是缓存键的一部分：同一个源上转型成不同元素类型应当得到不同视图
        Assert.Same(first, second);
        Assert.NotSame(first, otherTarget);
    }

    [Fact]
    public void UpcastListViewIsCachedPerSource()
    {
        NotifiableList<string> list = ["a"];
        IReadOnlyNotifiableList<string> source = list;

        IReadOnlyNotifiableList<object> first = source.AsReadOnlyNotifiableList(TypeA.Of<object>());
        IReadOnlyNotifiableList<object> second = source.AsReadOnlyNotifiableList(TypeA.Of<object>());

        Assert.Same(first, second);
    }

    [Fact]
    public void UpcastDictionaryViewIsCachedPerSource()
    {
        var dictionary = new NotifiableDictionary<string, string> { { "a", "1" } };
        IReadOnlyNotifiableDictionary<string, string> source = dictionary;

        IReadOnlyNotifiableDictionary<string, object> first =
            source.AsReadOnlyNotifiableDictionary(TypeA.Of<object>());
        IReadOnlyNotifiableDictionary<string, object> second =
            source.AsReadOnlyNotifiableDictionary(TypeA.Of<object>());

        Assert.Same(first, second);
    }

    [Fact]
    public void ProjectionViewsAreNotCached()
    {
        NotifiableList<string> list = ["a"];

        var first = list.AsReadOnlyNotifiableList(s => s.Length);
        var second = list.AsReadOnlyNotifiableList(s => s.Length);

        // 投影委托每次调用都可能不同，所以参数版本每次都给独立视图
        Assert.NotSame(first, second);
        Assert.Equal([1], first);
        Assert.Equal([1], second);
    }

    [Fact]
    public void CachedListViewForwardsEachChangeOnce()
    {
        NotifiableList<int> list = [];
        var first = list.AsReadOnlyNotifiableList();
        var second = list.AsReadOnlyNotifiableList();
        Assert.Same(first, second);

        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(second);
        list.Add(1);

        // 缓存命中不应当让源事件被重复订阅
        Assert.Single(updates);
    }
}
