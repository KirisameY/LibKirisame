using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.Data;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.DataTests;

public class ChangeInfoTests
{
    [Fact]
    public void ItemWithIndexPairsAnIndexWithAnItem()
    {
        var item = ItemWithIndex.From(2, "x");

        Assert.Equal(2, item.Index);
        Assert.Equal("x", item.Item);
    }

    [Fact]
    public void WithIndexBuildsTheSameThing()
    {
        var item = "x".WithIndex(2);

        Assert.Equal(2, item.Index);
        Assert.Equal("x", item.Item);
    }

    [Fact]
    public void ItemWithIndexFromDeconstructsThroughTheInterface()
    {
        // From 返回的是 IItemWithIndex<T>，这里走的是挂在接口上的解构扩展
        var (index, item) = ItemWithIndex.From(2, "x");

        Assert.Equal(2, index);
        Assert.Equal("x", item);
    }

    [Fact]
    public void WithIndexDeconstructsThroughTheInterface()
    {
        var (index, item) = "x".WithIndex(2);

        Assert.Equal(2, index);
        Assert.Equal("x", item);
    }

    [Fact]
    public void ItemWithIndexFromRemovedItemsDeconstructs()
    {
        var list = new NotifiableList<int> { 1, 2, 3, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.RemoveAll(i => i % 2 == 0);

        // RemovedItemsWithIndex 里装的就是 IItemWithIndex<T>，可以直接逐个解构
        var removed = Assert.IsAssignableFrom<IListItemRemovedEventArgs<int>>(Assert.Single(updates));
        List<(int Index, int Item)> pairs = [];
        foreach (var item in removed.RemovedItemsWithIndex)
        {
            var (index, element) = item;
            pairs.Add((index, element));
        }

        Assert.Equal([(1, 2), (3, 4)], pairs);
    }

    [Fact]
    public void ItemReplaceInfoKeepsTheOldAndNewValue()
    {
        var info = ItemReplaceInfo.From(1, 2);

        Assert.Equal(1, info.Old);
        Assert.Equal(2, info.New);
    }

    [Fact]
    public void ItemReplaceInfoDeconstructs()
    {
        var (oldValue, newValue) = ItemReplaceInfo.From(1, 2);

        Assert.Equal(1, oldValue);
        Assert.Equal(2, newValue);
    }

    [Fact]
    public void ItemReplaceInfoFromIndexOverloadCarriesTheIndex()
    {
        var info = ItemReplaceInfo.From(2, 1, 3);

        Assert.Equal(2, info.Index);
        Assert.Equal(1, info.Old);
        Assert.Equal(3, info.New);
    }

    [Fact]
    public void ListItemReplaceInfoDeconstructsTheIndexToo()
    {
        var (index, oldValue, newValue) = ItemReplaceInfo.From(2, 1, 3);

        Assert.Equal(2, index);
        Assert.Equal(1, oldValue);
        Assert.Equal(3, newValue);
    }

    [Fact]
    public void DictionaryItemReplaceInfoExposesKeyAndBothValues()
    {
        var info = DictionaryItemReplaceInfo.From("k", 1, 2);

        Assert.Equal("k", info.Key);
        Assert.Equal(1, info.OldValue);
        Assert.Equal(2, info.NewValue);
    }

    [Fact]
    public void DictionaryItemReplaceInfoAlsoReadsAsKeyValuePairs()
    {
        var info = DictionaryItemReplaceInfo.From("k", 1, 2);

        // 它同时也是 IItemReplaceInfo<KeyValuePair<TKey, TValue>>，旧值对和新值对的键相同
        Assert.Equal(new KeyValuePair<string, int>("k", 1), info.Old);
        Assert.Equal(new KeyValuePair<string, int>("k", 2), info.New);
    }

    [Fact]
    public void DictionaryItemReplaceInfoDeconstructsIntoKeyValuePairs()
    {
        var (oldPair, newPair) = DictionaryItemReplaceInfo.From("k", 1, 2);

        Assert.Equal(new KeyValuePair<string, int>("k", 1), oldPair);
        Assert.Equal(new KeyValuePair<string, int>("k", 2), newPair);
    }
}
