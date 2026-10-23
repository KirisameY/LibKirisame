using KirisameY.NotifiableCollections.Collections;

// 本文件的目的是测 ICollection<KeyValuePair<,>>.Contains 本身，所以刻意直接调它而不是改用 Assert.Contains：
// xUnit 的 Assert.Contains 只在目标是 ISet<T> 时才会转交给集合自己的实现，
// 其余情况一律自己枚举、用自己的比较器，压根走不到这个 Contains 上。
#pragma warning disable xUnit2017

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     <see cref="ICollection{T}.Contains"/> 在字典上的行为（实现下沉到
///     <see cref="INotifiableDictionary{TKey,TValue}"/> 的默认接口实现）。
///     它比 ContainsKey 严：键对上了还得值也相等才算命中。
/// </summary>
/// <remarks>
///     具体类型上已经没有公开的 <c>Contains(KeyValuePair)</c> 了：直接写 <c>dictionary.Contains(...)</c>
///     会被解析到 LINQ 的 <c>Enumerable.Contains</c>，测不到字典自己的实现。所以统一走接口。
/// </remarks>
public class DictionaryContainsTests
{
    private static bool CollectionContains<TKey, TValue>(
        NotifiableDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value
    ) where TKey : notnull =>
        ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(new KeyValuePair<TKey, TValue>(key, value));

    [Fact]
    public void EmptyDictionaryContainsNothing()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.False(CollectionContains(dictionary, "a", 1));
    }

    [Fact]
    public void ContainsRequiresBothKeyAndValue()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.True(CollectionContains(dictionary, "a", 1));
        Assert.False(CollectionContains(dictionary, "a", 2)); // 键对值不对
        Assert.False(CollectionContains(dictionary, "b", 1)); // 键不对
    }

    [Fact]
    public void ContainsIsStricterThanContainsKey()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        // 同一个键：ContainsKey 只看键在不在，Contains 还要值相等
        Assert.True(dictionary.ContainsKey("a"));
        Assert.False(CollectionContains(dictionary, "a", 2));
    }

    [Fact]
    public void ContainsFindsTheDefaultValueOfAValueType()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 0 } };

        // 值类型不存在「读出来是缺省值就等于没存」这回事：存的就是 0 时照样算命中
        Assert.True(CollectionContains(dictionary, "a", 0));
    }

    [Fact]
    public void ContainsFindsANullValue()
    {
        var dictionary = new NotifiableDictionary<string, string?> { { "a", null } };

        Assert.True(CollectionContains(dictionary, "a", null));
    }

    [Fact]
    public void ContainsUsesTheDefaultComparers()
    {
        var dictionary = new NotifiableDictionary<string, string> { { "a", "x" } };

        // 键和值都走默认的序数比较，大小写不折叠
        Assert.True(CollectionContains(dictionary, "a", "x"));
        Assert.False(CollectionContains(dictionary, "a", "X"));
        Assert.False(CollectionContains(dictionary, "A", "x"));
    }

    [Fact]
    public void ContainsIgnoresInsertionOrder()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.True(CollectionContains(dictionary, "b", 2));
        Assert.True(CollectionContains(dictionary, "a", 1));
    }

    [Fact]
    public void ContainsComparesValuesByValueRatherThanByReference()
    {
        var dictionary = new NotifiableDictionary<string, Box> { { "a", new Box(1) } };

        // 值换一个实例，只要 Equals 说相等就算命中
        Assert.True(CollectionContains(dictionary, "a", new Box(1)));
        Assert.False(CollectionContains(dictionary, "a", new Box(2)));
    }

    private sealed record Box(int Value);
}
