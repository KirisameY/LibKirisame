using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.NotifiableCollections.Test.TestDoubles;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     <see cref="NotifiableDictionarySimpleBase{TKey,TValue}"/> 的默认实现：只实现三个抽象成员的子类
///     应当直接获得整套 <see cref="INotifiableDictionary{TKey,TValue}"/> 行为。
/// </summary>
/// <remarks>
///     <see cref="NotifiableDictionary{TKey,TValue}"/> 把所有 protected virtual 钩子都重写了，
///     所以这里用 <see cref="MinimalNotifiableDictionary{TKey,TValue}"/> 专门验证那些钩子的默认实现，
///     包括默认的 <c>TryAddValue</c> / <c>AddValue</c> / <c>RemoveValue(key)</c> / <c>ClearValues</c>。
/// </remarks>
public class DictionarySimpleBaseTests
{
    private static KeyValuePair<string, int>[] Sorted(IEnumerable<KeyValuePair<string, int>> pairs) =>
        [.. pairs.OrderBy(p => p.Key, StringComparer.Ordinal)];

    [Fact]
    public void MinimalSubclassOnlyNeedsTheThreeAbstractMembers()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int>();
        dictionary.Add("a", 1);
        dictionary["b"] = 2;

        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.ContainsKey("a"));
        Assert.True(dictionary.TryGetValue("b", out var b));
        Assert.Equal(2, b);
        Assert.Equal(["a", "b"], dictionary.Keys.Order(StringComparer.Ordinal));
        Assert.Equal([1, 2], dictionary.Values.Order());
    }

    [Fact]
    public void AddThroughTheBaseRaisesOneAddedEvent()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Add("a", 1);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Single(added.AddedItems);
        Assert.Equal(1, added.AddedItems["a"]);
    }

    [Fact]
    public void IndexerThroughTheBaseRaisesAddThenReplace()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary["a"] = 1;
        dictionary["a"] = 2;

        Assert.Equal(2, updates.Count);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(updates[0]);
        Assert.Equal(1, added.AddedItems["a"]);

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(updates[1]);
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(2, replaced.NewItems["a"]);
    }

    [Fact]
    public void TryAddThroughTheBaseUsesTheDefaultTryAddValue()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(dictionary.TryAdd("a", 1));
        Assert.Single(updates);

        Assert.False(dictionary.TryAdd("a", 2));
        Assert.Single(updates);
        Assert.Equal(1, dictionary["a"]);
    }

    [Fact]
    public void AddRangeThroughTheBaseAddsEveryPairInOneEvent()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.AddRange([new("a", 1), new("b", 2)]);

        Assert.Equal(2, dictionary.Count);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["a", "b"], Sorted(added.AddedItems).Select(p => p.Key));
    }

    [Fact]
    public void TryAddRangeThroughTheBaseAddsOnlyMissingKeys()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.TryAddRange([new("a", 9), new("b", 2)]);

        Assert.Equal(["b"], returned.Keys);
        Assert.Equal(1, dictionary["a"]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, added.AddedItems["b"]);
        Assert.Single(added.AddedItems);
    }

    [Fact]
    public void PutRangeThroughTheBaseReportsReplacementBeforeAddition()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var returned = dictionary.PutRange([new("a", 9), new("b", 2)]);

        Assert.Equal(2, updates.Count);

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(updates[0]);
        Assert.Equal(1, replaced.OldItems["a"]);
        Assert.Equal(9, replaced.NewItems["a"]);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(updates[1]);
        Assert.Equal(2, added.AddedItems["b"]);

        Assert.Equal(1, returned["a"]);
        Assert.Equal(9, dictionary["a"]);
        Assert.Equal(2, dictionary["b"]);
    }

    [Fact]
    public void RemoveThroughTheBaseHandsBackTheValueAndRaisesOneEvent()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(dictionary.Remove("a", out var value));
        Assert.Equal(1, value);

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);

        Assert.False(dictionary.Remove("a", out _));
        Assert.Single(updates);
    }

    [Fact]
    public void RemoveRangeThroughTheBaseSkipsMissingKeysAndRepeats()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var removed = dictionary.RemoveRange(["a", "zz", "a"]);

        Assert.Equal(["a"], removed.Keys);
        Assert.Equal(["b"], dictionary.Keys);

        var removedArgs = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Single(removedArgs.RemovedItems);
    }

    [Fact]
    public void RemoveAllThroughTheBaseRemovesMatchingPairs()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var removed = dictionary.RemoveAll(p => p.Value >= 2);

        Assert.Equal(["b", "c"], Sorted(removed).Select(p => p.Key));
        Assert.Equal(["a"], dictionary.Keys);

        var removedArgs = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["b", "c"], Sorted(removedArgs.RemovedItems).Select(p => p.Key));
    }

    [Fact]
    public void ClearThroughTheBaseUsesTheDefaultClearValues()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        var before = dictionary.Clear();

        Assert.Equal(["a", "b"], Sorted(before).Select(p => p.Key));
        Assert.Empty(dictionary);

        var cleared = Assert.IsAssignableFrom<IDictionaryItemClearedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(["a", "b"], Sorted(cleared.RemovedItems).Select(p => p.Key));
    }

    [Fact]
    public void ReplaceAndReplaceRangeAreInheritedFromTheCollectionInterface()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        INotifiableCollection<KeyValuePair<string, int>> collection = dictionary;
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(collection.Replace(new("a", 1), new("a", 9)));

        var results = collection.ReplaceRange([
            (new KeyValuePair<string, int>("b", 2), new KeyValuePair<string, int>("b", 20))
        ]);

        Assert.Equal([(true, new KeyValuePair<string, int>("b", 2))], results);
        Assert.Equal(9, dictionary["a"]);
        Assert.Equal(20, dictionary["b"]);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void KeysAndValuesAreNotifyingViews()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 } };
        var keyUpdates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Keys);
        var valueUpdates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(dictionary.Values);

        dictionary.Add("b", 2);

        var addedKeys = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<string>>(Assert.Single(keyUpdates));
        Assert.Equal(["b"], addedKeys.AddedItems);

        var addedValues = Assert.IsAssignableFrom<ICollectionItemAddedEventArgs<int>>(Assert.Single(valueUpdates));
        Assert.Equal([2], addedValues.AddedItems);
    }

    [Fact]
    public void KeyCollectionIsAReadOnlyAdapter()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        IDictionary<string, int> asDictionary = dictionary;
        var keys = asDictionary.Keys;

        Assert.Equal(2, keys.Count);
        Assert.True(keys.IsReadOnly);
        Assert.True(keys.Contains("a"));
        Assert.False(keys.Contains("zz"));

        var array = new string[3];
        keys.CopyTo(array, 1);
        Assert.Equal(["a", "b"], array[1..3].Order(StringComparer.Ordinal));

        Assert.Throws<NotSupportedException>(() => keys.Add("c"));
        Assert.Throws<NotSupportedException>(() => keys.Remove("a"));
        Assert.Throws<NotSupportedException>(() => keys.Clear());
    }

    [Fact]
    public void ValueCollectionIsAReadOnlyAdapter()
    {
        var dictionary = new MinimalNotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        IDictionary<string, int> asDictionary = dictionary;
        var values = asDictionary.Values;

        Assert.Equal(2, values.Count);
        Assert.True(values.IsReadOnly);
        Assert.True(values.Contains(2));
        Assert.False(values.Contains(99));

        var array = new int[3];
        values.CopyTo(array, 1);
        Assert.Equal([1, 2], array[1..3].Order());

        Assert.Throws<NotSupportedException>(() => values.Add(3));
        Assert.Throws<NotSupportedException>(() => values.Remove(1));
        Assert.Throws<NotSupportedException>(() => values.Clear());
    }
}
