using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

public class DictionaryReadTests
{
    [Fact]
    public void NewDictionaryIsEmpty()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void IndexerReadsByKey()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.Equal(1, dictionary["a"]);
        Assert.Equal(2, dictionary["b"]);
    }

    [Fact]
    public void IndexerThrowsForAMissingKey()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.Throws<KeyNotFoundException>(() => _ = dictionary["a"]);
    }

    [Fact]
    public void ContainsKeyAndTryGetValueAgree()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.True(dictionary.ContainsKey("a"));
        Assert.False(dictionary.ContainsKey("b"));
        Assert.True(dictionary.TryGetValue("a", out var value));
        Assert.Equal(1, value);
    }

    [Fact]
    public void TryGetValueReportsFalseAndDefaultForAMissingKey()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.False(dictionary.TryGetValue("b", out var value));

        Assert.Equal(0, value);
    }

    [Fact]
    public void ContainsRequiresBothKeyAndValueToMatch()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.Contains(new KeyValuePair<string, int>("a", 1), dictionary);
        Assert.DoesNotContain(new KeyValuePair<string, int>("a", 2), dictionary);
        Assert.DoesNotContain(new KeyValuePair<string, int>("b", 1), dictionary);
    }

    [Fact]
    public void EnumerationYieldsKeyValuePairs()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.Equal(
            [
                new KeyValuePair<string, int>("a", 1),
                new KeyValuePair<string, int>("b", 2)
            ],
            dictionary
        );
    }

    [Fact]
    public void KeysAndValuesExposeTheContents()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.Equal(["a", "b"], dictionary.Keys);
        Assert.Equal([1, 2], dictionary.Values);
        Assert.Equal(2, dictionary.Keys.Count);
        Assert.Equal(2, dictionary.Values.Count);
    }

    [Fact]
    public void CopyToCopiesPairsIntoTheArray()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var array = new KeyValuePair<string, int>[2];

        dictionary.CopyTo(array, 1);

        Assert.Equal(new KeyValuePair<string, int>("a", 1), array[1]);
    }

    [Fact]
    public void ReadsAgreeAcrossTheBridgedInterfaces()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.Single((ICollection<KeyValuePair<string, int>>)dictionary);
        Assert.Single((IReadOnlyCollection<KeyValuePair<string, int>>)dictionary);
        Assert.Equal(1, ((IDictionary<string, int>)dictionary)["a"]);
        Assert.Equal(1, ((IReadOnlyDictionary<string, int>)dictionary)["a"]);
        Assert.True(((IDictionary<string, int>)dictionary).ContainsKey("a"));
        Assert.True(((IReadOnlyDictionary<string, int>)dictionary).ContainsKey("a"));
        Assert.True(((IDictionary<string, int>)dictionary).TryGetValue("a", out var fromDictionary));
        Assert.Equal(1, fromDictionary);
        Assert.True(((IReadOnlyDictionary<string, int>)dictionary).TryGetValue("a", out var fromReadOnly));
        Assert.Equal(1, fromReadOnly);
        Assert.Equal(["a"], ((IDictionary<string, int>)dictionary).Keys);
        Assert.Equal(["a"], ((IReadOnlyDictionary<string, int>)dictionary).Keys);
        Assert.Equal([1], ((IDictionary<string, int>)dictionary).Values);
        Assert.Equal([1], ((IReadOnlyDictionary<string, int>)dictionary).Values);
    }

    [Fact]
    public void DictionaryIsNotReadOnly()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.False(((ICollection<KeyValuePair<string, int>>)dictionary).IsReadOnly);
    }

    [Fact]
    public void WritingThroughTheDictionaryInterfaceStillNotifies()
    {
        var dictionary = new NotifiableDictionary<string, int>();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);
        var asDictionary = (IDictionary<string, int>)dictionary;

        asDictionary.Add("a", 1);
        asDictionary["b"] = 2;
        asDictionary["a"] = 3;
        asDictionary.Remove("b");

        Assert.Equal(4, updates.Count);
        Assert.Equal(3, dictionary["a"]);
        Assert.Single(dictionary);
    }
}
