using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     字典作为 <see cref="INotifiableCollection{T}"/> 时新桥接上来的操作：
///     按整对移除、按谓词移除、按整对替换与批量替换。
/// </summary>
/// <remarks>
///     字典的“集合”视角以 <see cref="KeyValuePair{TKey,TValue}"/> 为单位，所以 <c>RemoveRange</c> /
///     <c>Replace</c> 都要求键和值同时匹配；只给对键、值不对的项必须被当成“不在集合里”。
/// </remarks>
public class DictionaryCollectionInterfaceTests
{
    private static KeyValuePair<string, int>[] Sorted(IEnumerable<KeyValuePair<string, int>> pairs) =>
        [.. pairs.OrderBy(p => p.Key, StringComparer.Ordinal)];

    [Fact]
    public void AddRangeThroughTheCollectionInterfaceAddsEveryPair()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        collection.AddRange([new("b", 2), new("c", 3)]);

        Assert.Equal(3, dictionary.Count);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["b", "c"], Sorted(added.AddedItems).Select(p => p.Key));
    }

    [Fact]
    public void RemoveRangeThroughTheCollectionInterfaceRequiresWholePairsToMatch()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        // b 的值不对，整对不在集合里，应被静默跳过
        var removed = collection.RemoveRange([new("a", 1), new("b", 99), new("c", 3)]);

        Assert.Equal(["a", "c"], Sorted(removed).Select(p => p.Key));
        Assert.Equal(["b"], dictionary.Keys);

        var removedArgs = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["a", "c"], Sorted(removedArgs.RemovedItems).Select(p => p.Key));
    }

    [Fact]
    public void RemoveRangeThroughTheCollectionInterfaceWithNothingMatchingRaisesNothing()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var removed = collection.RemoveRange([new("a", 99)]);

        Assert.Empty(removed);
        Assert.Empty(updates);
        Assert.Equal(1, dictionary["a"]);
    }

    [Fact]
    public void RemoveAllThroughTheCollectionInterfaceRemovesMatchingPairs()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var removed = collection.RemoveAll(p => p.Value % 2 == 1);

        Assert.Equal(["a", "c"], Sorted(removed).Select(p => p.Key));
        Assert.Equal(["b"], dictionary.Keys);

        var removedArgs = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["a", "c"], Sorted(removedArgs.RemovedItems).Select(p => p.Key));
    }

    [Fact]
    public void ReplaceThroughTheCollectionInterfaceRequiresTheSameKeyAndCurrentValue()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(collection.Replace(new("a", 1), new("a", 9)));

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(9, replaced.NewItems["a"]);

        // 旧值已经变了 / 想换键，都不算“替换集合里的这一项”
        Assert.False(collection.Replace(new("a", 1), new("a", 8)));
        Assert.False(collection.Replace(new("b", 2), new("x", 7)));

        Assert.Single(updates);
        Assert.Equal(9, dictionary["a"]);
        Assert.Equal(2, dictionary["b"]);
    }

    [Fact]
    public void ReplaceRangeThroughTheCollectionInterfaceReportsSuccessesAndFailures()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var results = collection.ReplaceRange([
            (new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("a", 9)),
            (new KeyValuePair<string, int>("b", 2), new KeyValuePair<string, int>("x", 8)),
            (new KeyValuePair<string, int>("zz", 0), new KeyValuePair<string, int>("zz", 5))
        ]);

        Assert.Equal(3, results.Count);
        Assert.Contains((true, new KeyValuePair<string, int>("a", 1)), results);
        Assert.Contains((false, new KeyValuePair<string, int>("x", 8)), results);
        Assert.Contains((false, new KeyValuePair<string, int>("zz", 5)), results);

        Assert.Equal(9, dictionary["a"]);
        Assert.Equal(2, dictionary["b"]);

        // 只有成功的 a 产生了一次替换通知
        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(9, replaced.NewItems["a"]);
    }

    [Fact]
    public void ReplaceRangeThroughTheCollectionInterfaceRejectsASecondCommandForTheSameKey()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var results = collection.ReplaceRange([
            (new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("a", 9)),
            (new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("a", 8))
        ]);

        Assert.Contains((true, new KeyValuePair<string, int>("a", 1)), results);
        Assert.Contains((false, new KeyValuePair<string, int>("a", 8)), results);
        Assert.Equal(9, dictionary["a"]);
        Assert.Single(updates);
    }

    [Fact]
    public void ClearThroughICollectionClearsTheDictionary()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        ((ICollection<KeyValuePair<string, int>>)dictionary).Clear();

        Assert.Empty(dictionary);

        var cleared = Assert.IsAssignableFrom<IDictionaryItemClearedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["a", "b"], Sorted(cleared.RemovedItems).Select(p => p.Key));
    }

    [Fact]
    public void RemoveThroughIDictionaryReturnsWhetherItRemoved()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);
        var asDictionary = (IDictionary<string, int>)dictionary;

        Assert.True(asDictionary.Remove("a"));

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);

        updates.Clear();

        Assert.False(asDictionary.Remove("a"));
        Assert.Empty(updates);
    }
}
