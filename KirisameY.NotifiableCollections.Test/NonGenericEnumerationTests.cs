using System.Collections;

using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test;

/// <summary>
///     非泛型 IEnumerable.GetEnumerator 的覆盖。
///     所有集合和包装器都得在只经过非泛型枚举器时也列出正确的内容；
///     投影包装器尤其容易在这里漏掉投影，把源元素原样吐出来。
/// </summary>
/// <remarks>
///     刻意用 IEnumerator 手工遍历而不是 <c>Enumerable.Cast</c>：
///     Cast 在源自身就实现了 <c>IEnumerable&lt;T&gt;</c> 时会抄近路走泛型枚举器，那样就测不到这里想测的东西了。
/// </remarks>
public class NonGenericEnumerationTests
{
    private static object?[] Enumerate(IEnumerable source)
    {
        List<object?> items = [];
        var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext()) items.Add(enumerator.Current);
        return [..items];
    }

    [Fact]
    public void EmptyCollectionStillEnumerates()
    {
        Assert.Empty(Enumerate(new NotifiableList<int>()));
        Assert.Empty(Enumerate(new NotifiableDictionary<string, int>()));
    }

    [Fact]
    public void NotifiableListEnumeratesNonGenerically()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        Assert.Equal([1, 2, 3], Enumerate(list));
    }

    [Fact]
    public void NotifiableDictionaryEnumeratesNonGenerically()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.Equal(
            [
                new KeyValuePair<string, int>("a", 1),
                new KeyValuePair<string, int>("b", 2)
            ],
            Enumerate(dictionary)
        );
    }

    [Fact]
    public void CollectionViewEnumeratesNonGenerically()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        Assert.Equal([1, 2, 3], Enumerate(list.AsReadOnlyNotifiableCollection()));
    }

    [Fact]
    public void ListViewEnumeratesNonGenerically()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        Assert.Equal([1, 2, 3], Enumerate(list.AsReadOnlyNotifiableList()));
    }

    [Fact]
    public void DictionaryViewEnumeratesNonGenerically()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.Equal(
            [
                new KeyValuePair<string, int>("a", 1),
                new KeyValuePair<string, int>("b", 2)
            ],
            Enumerate(dictionary.AsReadOnlyNotifiableDictionary())
        );
    }

    [Fact]
    public void ProjectingCollectionViewEnumeratesNonGenerically()
    {
        var list = new NotifiableList<string> { "a", "bb" };

        Assert.Equal([1, 2], Enumerate(list.AsReadOnlyNotifiableCollection(s => s.Length)));
    }

    [Fact]
    public void ProjectingListViewEnumeratesNonGenerically()
    {
        var list = new NotifiableList<string> { "a", "bb" };

        Assert.Equal([1, 2], Enumerate(list.AsReadOnlyNotifiableList(s => s.Length)));
    }

    [Fact]
    public void ProjectingDictionaryViewEnumeratesNonGenerically()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        // 非泛型枚举也要走投影，而不是把源键值对原样吐出来
        Assert.Equal(
            [
                new KeyValuePair<string, int>("a", 10),
                new KeyValuePair<string, int>("b", 20)
            ],
            Enumerate(dictionary.AsReadOnlyNotifiableDictionary(v => v * 10))
        );
    }

    [Fact]
    public void DictionaryKeysViewEnumeratesNonGenerically()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.Equal(["a", "b"], Enumerate(dictionary.Keys));
    }

    [Fact]
    public void DictionaryValuesViewEnumeratesNonGenerically()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        // Values 是个值投影包装器，非泛型枚举同样要投影
        Assert.Equal([1, 2], Enumerate(dictionary.Values));
    }
}
