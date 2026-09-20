using KirisameY.NotifiableCollections.Collections;

// 本文件的目的是测 Contains 方法本身，所以刻意直接调它而不是改用 Assert.Contains：
// xUnit 的 Assert.Contains 只在目标是 ISet<T> 时才会转交给集合自己的实现，
// 其余情况一律自己枚举、用自己的比较器，压根走不到这个 Contains 上。
#pragma warning disable xUnit2017

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     NotifiableDictionary.Contains 本身的行为。
///     它比 ContainsKey 严：键对上了还得值也相等才算命中。
/// </summary>
public class DictionaryContainsTests
{
    [Fact]
    public void EmptyDictionaryContainsNothing()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.False(dictionary.Contains(new KeyValuePair<string, int>("a", 1)));
    }

    [Fact]
    public void ContainsRequiresBothKeyAndValue()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        Assert.True(dictionary.Contains(new KeyValuePair<string, int>("a", 1)));
        Assert.False(dictionary.Contains(new KeyValuePair<string, int>("a", 2))); // 键对值不对
        Assert.False(dictionary.Contains(new KeyValuePair<string, int>("b", 1))); // 键不对
    }

    [Fact]
    public void ContainsIsStricterThanContainsKey()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        // 同一个键：ContainsKey 只看键在不在，Contains 还要值相等
        Assert.True(dictionary.ContainsKey("a"));
        Assert.False(dictionary.Contains(new KeyValuePair<string, int>("a", 2)));
    }

    [Fact]
    public void ContainsFindsTheDefaultValueOfAValueType()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 0 } };

        // 值类型不存在「读出来是缺省值就等于没存」这回事：存的就是 0 时照样算命中
        Assert.True(dictionary.Contains(new KeyValuePair<string, int>("a", 0)));
    }

    [Fact]
    public void ContainsFindsANullValue()
    {
        var dictionary = new NotifiableDictionary<string, string?> { { "a", null } };

        Assert.True(dictionary.Contains(new KeyValuePair<string, string?>("a", null)));
    }

    [Fact]
    public void ContainsUsesTheDefaultComparers()
    {
        var dictionary = new NotifiableDictionary<string, string> { { "a", "x" } };

        // 键和值都走默认的序数比较，大小写不折叠
        Assert.True(dictionary.Contains(new KeyValuePair<string, string>("a", "x")));
        Assert.False(dictionary.Contains(new KeyValuePair<string, string>("a", "X")));
        Assert.False(dictionary.Contains(new KeyValuePair<string, string>("A", "x")));
    }

    [Fact]
    public void ContainsIgnoresInsertionOrder()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 }, { "b", 2 } };

        Assert.True(dictionary.Contains(new KeyValuePair<string, int>("b", 2)));
        Assert.True(dictionary.Contains(new KeyValuePair<string, int>("a", 1)));
    }

    [Fact]
    public void ContainsComparesValuesByValueRatherThanByReference()
    {
        var dictionary = new NotifiableDictionary<string, Box> { { "a", new Box(1) } };

        // 值换一个实例，只要 Equals 说相等就算命中
        Assert.True(dictionary.Contains(new KeyValuePair<string, Box>("a", new Box(1))));
        Assert.False(dictionary.Contains(new KeyValuePair<string, Box>("a", new Box(2))));
    }

    [Fact]
    public void ContainsAgreesAcrossTheBridgedInterfaces()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };

        // ICollection<KeyValuePair<,>>.Contains 就是上面那个公开方法，没有另开一份实现
        Assert.True(((ICollection<KeyValuePair<string, int>>)dictionary).Contains(new KeyValuePair<string, int>("a", 1)));
        Assert.False(((ICollection<KeyValuePair<string, int>>)dictionary).Contains(new KeyValuePair<string, int>("a", 2)));
    }

    private sealed record Box(int Value);
}
