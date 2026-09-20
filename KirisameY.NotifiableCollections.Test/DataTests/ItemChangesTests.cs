using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DataTests;

/// <summary>
///     各层事件参数上的 ItemChanges：列表版带索引、字典版带键，集合版则退化成只按位配对的新旧值。
///     经过包装器转换后这份对照还得跟着重新映射一遍。
/// </summary>
public class ItemChangesTests
{
    [Fact]
    public void ListItemChangesAreAlsoVisibleThroughTheCollectionLevelInterface()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list[1] = 9;

        // 同一份参数换个接口看：集合级那份是同一批对照，只是不再暴露索引
        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        var viaCollection = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(replaced);

        Assert.Same(replaced.ItemChanges, viaCollection.ItemChanges);

        var change = Assert.Single(viaCollection.ItemChanges);
        Assert.Equal(2, change.Old);
        Assert.Equal(9, change.New);
    }

    [Fact]
    public void CollectionViewPairsItemChangesByPosition()
    {
        var list = new NotifiableList<int> { 1, 2 };
        var view = list.AsReadOnlyNotifiableCollection();
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list[0] = 9;

        // 走集合包装器时用的是集合级的实现：没有索引，只把新旧值按位配起来
        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal(1, change.Old);
        Assert.Equal(9, change.New);
    }

    [Fact]
    public void DictionaryItemChangesAreAlsoVisibleThroughTheCollectionLevelInterface()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary["a"] = 2;

        // 集合级看过去是键值对的新旧对照，键在新旧两侧一致
        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        var viaCollection = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<KeyValuePair<string, int>>>(replaced);

        var change = Assert.Single(viaCollection.ItemChanges);
        Assert.Equal(new KeyValuePair<string, int>("a", 1), change.Old);
        Assert.Equal(new KeyValuePair<string, int>("a", 2), change.New);
    }

    [Fact]
    public void ProjectingListViewMapsItemChanges()
    {
        var list = new NotifiableList<string> { "a" };
        var view = list.AsReadOnlyNotifiableList(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(view);

        list[0] = "bbb";

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal(0, change.Index);
        Assert.Equal(1, change.Old);
        Assert.Equal(3, change.New);
    }

    [Fact]
    public void ProjectingCollectionViewMapsItemChanges()
    {
        var list = new NotifiableList<string> { "a" };
        var view = list.AsReadOnlyNotifiableCollection(s => s.Length);
        var updates = NotifiableCollectionsTestUtils.RecordCollectionUpdates(view);

        list[0] = "bbb";

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal(1, change.Old);
        Assert.Equal(3, change.New);
    }

    [Fact]
    public void ProjectingDictionaryViewMapsItemChanges()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var view = dictionary.AsReadOnlyNotifiableDictionary(v => v.ToString());
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(view);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, string>>(Assert.Single(updates));
        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal("a", change.Key);
        Assert.Equal("1", change.OldValue);
        Assert.Equal("2", change.NewValue);
    }

    [Fact]
    public void ListItemChangesAreCachedPerArgsInstance()
    {
        var list = new NotifiableList<int> { 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list[0] = 2;

        // 惰性属性：同一份参数上重复读取拿到的是同一个实例，不必每次重新配一遍
        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Same(replaced.ItemChanges, replaced.ItemChanges);
    }

    [Fact]
    public void DictionaryItemChangesAreCachedPerArgsInstance()
    {
        var dictionary = new NotifiableDictionary<string, int> { { "a", 1 } };
        var updates = NotifiableCollectionsTestUtils.RecordDictionaryUpdates(dictionary);

        dictionary["a"] = 2;

        var replaced = Assert.IsAssignableFrom<IDictionaryItemReplacedEventArgs<string, int>>(Assert.Single(updates));
        Assert.Same(replaced.ItemChanges, replaced.ItemChanges);
    }
}
