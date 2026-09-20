using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

public class DictionaryChangeTests
{
    [Fact]
    public void AddReportsTheAddedPair()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Add("a", 1);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, added.AddedItems["a"]);
        Assert.Single(dictionary);
    }

    [Fact]
    public void AddKeyValuePairOverloadBehavesTheSame()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Add(new KeyValuePair<string, int>("a", 1));

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, added.AddedItems["a"]);
    }

    [Fact]
    public void AddThrowsForADuplicateKey()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.Throws<ArgumentException>(() => dictionary.Add("a", 2));
    }

    [Fact]
    public void IndexerSetOnANewKeyRaisesAnAdd()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary["a"] = 1;

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, added.AddedItems["a"]);
    }

    [Fact]
    public void IndexerSetOnAnExistingKeyRaisesAReplace()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(2, replaced.NewItems["a"]);
    }

    [Fact]
    public void ItemChangesPairKeyWithOldAndNewValue()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal("a", change.Key);
        Assert.Equal(1, change.OldValue);
        Assert.Equal(2, change.NewValue);
    }

    [Fact]
    public void TryAddReportsWhetherItAdded()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.False(dictionary.TryAdd("a", 2));
        Assert.Empty(updates);

        Assert.True(dictionary.TryAdd("b", 2));
        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, added.AddedItems["b"]);
    }

    [Fact]
    public void RemoveByKeyReportsTheRemovedPair()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(dictionary.Remove("a"));

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
        Assert.Empty(dictionary);
    }

    [Fact]
    public void RemoveByKeyReturnsFalseAndRaisesNothingWhenAbsent()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.False(dictionary.Remove("b"));

        Assert.Empty(updates);
        Assert.Single(dictionary);
    }

    [Fact]
    public void RemoveByPairRequiresTheValueToMatch()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        // 键对了但值不对：ICollection<KVP>.Remove 要求两者都相等
        Assert.False(dictionary.Remove(new KeyValuePair<string, int>("a", 2)));

        Assert.Empty(updates);
        Assert.Single(dictionary);
    }

    [Fact]
    public void RemoveByPairReportsTheRemovedPair()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(dictionary.Remove(new KeyValuePair<string, int>("a", 1)));

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
    }

    [Fact]
    public void ClearRemovesEverything()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Clear();

        Assert.IsAssignableFrom<IDictionaryItemClearedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Empty(dictionary);
    }

    [Fact]
    public void ClearReportsEveryPairItRemoved()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Clear();

        // 清空是移除的特例，RemovedItems 应当是清空前的全部内容
        var cleared = Assert.IsAssignableFrom<IDictionaryItemClearedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, cleared.RemovedItems.Count);
        Assert.Equal(1, cleared.RemovedItems["a"]);
        Assert.Equal(2, cleared.RemovedItems["b"]);
    }

    [Fact]
    public void ClearedEventArgsAreAlsoRemovedEventArgs()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Clear();

        // 清空是移除的特例：字典级和集合级都能按各自的移除接口接住
        var cleared = Assert.IsAssignableFrom<IDictionaryItemClearedEventArgs<string, int>>(Assert.Single(updates));
        Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(cleared);
        Assert.IsAssignableFrom<ICollectionItemClearedEventArgs<KeyValuePair<string, int>>>(cleared);
    }
}
