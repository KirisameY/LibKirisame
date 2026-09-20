using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ViewTests;

/// <summary>
///     把字典当成更基础的「集合」（键值对的集合）包装起来时，事件同样要降级成
///     只实现集合级接口的参数，不能把字典级的接口漏出去。
///     原样包装和投影包装两条路都得满足。
/// </summary>
public class DictionaryAsCollectionViewTests
{
    /// <summary>
    ///     <paramref name="projecting"/> 为 <see langword="false"/> 走原样包装，
    ///     为 <see langword="true"/> 走值投影包装（投影函数取恒等，好让两条路元素类型一致）。
    /// </summary>
    private static IReadOnlyNotifiableCollection<KeyValuePair<string, int>> AsCollection(
        NotifiableDictionary<string, int> dictionary,
        bool projecting
    ) =>
        projecting
            ? dictionary.AsReadOnlyNotifiableCollection(p => p)
            : dictionary.AsReadOnlyNotifiableCollection();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = AsCollection(dictionary, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        dictionary.Add("a", 1);

        var added = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<KeyValuePair<string, int>>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(added);
        Assert.IsNotAssignableFrom<IDictionaryUpdateEventArgs<string, int>>(added);
        Assert.Equal([new KeyValuePair<string, int>("a", 1)], added.AddedItems);
        Assert.Same(view, added.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemovedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var view = AsCollection(dictionary, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        dictionary.Remove("a");

        var removed = Assert.IsAssignableFrom<ICollectionItemRemovedEventArgs<KeyValuePair<string, int>>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(removed);
        Assert.IsNotAssignableFrom<IDictionaryUpdateEventArgs<string, int>>(removed);
        Assert.Equal([new KeyValuePair<string, int>("a", 1)], removed.RemovedItems);
        Assert.Same(view, removed.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var view = AsCollection(dictionary, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        dictionary.Clear();

        var cleared = Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<KeyValuePair<string, int>>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IDictionaryItemClearedEventArgs<string, int>>(cleared);
        Assert.IsNotAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(cleared);

        // 快照是 ImmutableDictionary，枚举顺序不保证，比较前先按 key 排一下
        Assert.Equal(
            [new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2)],
            cleared.RemovedItems.OrderBy(p => p.Key, StringComparer.Ordinal)
        );
        Assert.Same(view, cleared.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReplacedItemsAreNarrowedToTheCollectionLevel(bool projecting)
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var view = AsCollection(dictionary, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<KeyValuePair<string, int>>>(Assert.Single(updates));
        Assert.IsNotAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(replaced);
        Assert.IsNotAssignableFrom<IDictionaryUpdateEventArgs<string, int>>(replaced);
        Assert.Equal([new KeyValuePair<string, int>("a", 1)], replaced.OldItems);
        Assert.Equal([new KeyValuePair<string, int>("a", 2)], replaced.NewItems);
        Assert.Same(view, replaced.CollectionView);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryDictionaryChangePassesThroughWithoutThrowing(bool projecting)
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var view = AsCollection(dictionary, projecting);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        // 字典的四种变更在集合层都有对应事件，应该一个不落、也不抛异常
        var exception = Record.Exception(() =>
        {
            dictionary.Add("a", 1);
            dictionary["a"] = 2;
            dictionary.Remove("a");
            dictionary.Add("b", 2);
            dictionary.Clear();
        });

        Assert.Null(exception);
        Assert.Equal(5, updates.Count);
    }
}
