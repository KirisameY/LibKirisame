using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DictionaryTests;

/// <summary>
///     <see cref="NotifiableDictionaryFunctionalExtensions"/> 中由 <see cref="NotifiableDictionary{TKey,TValue}"/>
///     实例方法拆出来的重载：键值对版本的 <c>TryAdd</c> 与单参数的 <c>Remove</c>。
/// </summary>
/// <remarks>
///     两个方法都通过同样签名的键 / 值版本转发，测试要确认的是「同一行为 + 同一通知」，
///     并且通过接口引用也能解析到扩展方法。
/// </remarks>
public class DictionaryFunctionalExtensionTests
{
    [Fact]
    public void TryAddPairExtensionMatchesTheKeyValueOverload()
    {
        INotifiableDictionary<string, int> dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.False(dictionary.TryAdd(new KeyValuePair<string, int>("a", 9)));
        Assert.Empty(updates);
        Assert.Equal(1, dictionary["a"]);

        Assert.True(dictionary.TryAdd(new KeyValuePair<string, int>("b", 2)));

        var added = Assert.IsAssignableFrom<IDictionaryItemAddedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(2, added.AddedItems["b"]);
        Assert.Single(added.AddedItems);
    }

    [Fact]
    public void SingleArgumentRemoveExtensionMatchesTheOutOverload()
    {
        INotifiableDictionary<string, int> dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        Assert.True(dictionary.Remove("a"));

        var removed = Assert.IsAssignableFrom<IDictionaryItemRemovedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Equal(1, removed.RemovedItems["a"]);
        Assert.Empty(dictionary);

        Assert.False(dictionary.Remove("zz"));
        Assert.Single(updates);
    }

    [Fact]
    public void ExtensionsAreAlsoAvailableOnTheConcreteType()
    {
        var dictionary = new NotifiableDictionary<string, int>();

        Assert.True(dictionary.TryAdd(new KeyValuePair<string, int>("a", 1)));
        Assert.True(dictionary.Remove("a"));
        Assert.Empty(dictionary);
    }
}
