using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     <see cref="CommonCollectionExtensions.ToNotifiableDictionary{TKey,TValue}(IEnumerable{KeyValuePair{TKey,TValue}})"/>：
///     现在返回 <see cref="INotifiableDictionary{TKey,TValue}"/>，而不是具体实现类型；
///     同时去掉了多余的二次枚举，源序列只应被枚举一次。
/// </summary>
public class DictionaryCreationTests
{
    [Fact]
    public void ToNotifiableDictionaryKeepsEverySourcePair()
    {
        INotifiableDictionary<string, int> dictionary = new[]
        {
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("b", 2)
        }.ToNotifiableDictionary();

        // 字典不承诺枚举顺序，只确认每个键值对都在
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(1, dictionary["a"]);
        Assert.Equal(2, dictionary["b"]);
    }

    [Fact]
    public void ToNotifiableDictionaryProducesADictionaryThatStillNotifies()
    {
        var dictionary = new[] { new KeyValuePair<string, int>("a", 1) }.ToNotifiableDictionary();
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary.Add("b", 2);

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, added.AddedItems["b"]);
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(2, dictionary["b"]);
    }

    [Fact]
    public void ToNotifiableDictionaryFromAnEmptySourceIsEmpty()
    {
        INotifiableDictionary<string, int> dictionary =
            Array.Empty<KeyValuePair<string, int>>().ToNotifiableDictionary();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToNotifiableDictionaryEnumeratesTheSourceOnlyOnce()
    {
        var enumerations = 0;
        IEnumerable<KeyValuePair<string, int>> Source()
        {
            enumerations++;
            yield return new("a", 1);
            yield return new("b", 2);
        }

        var dictionary = Source().ToNotifiableDictionary();

        Assert.Equal(1, enumerations);
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(["a", "b"], dictionary.Select(pair => pair.Key).Order());
    }
}
