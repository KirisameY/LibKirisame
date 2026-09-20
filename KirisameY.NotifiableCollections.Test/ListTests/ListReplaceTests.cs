using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

public class ListReplaceTests
{
    [Fact]
    public void IndexerSetReportsTheOldAndNewValueAtThatIndex()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list[1] = 9;

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], replaced.Indexes);
        Assert.Equal([2], replaced.OldItems);
        Assert.Equal([9], replaced.NewItems);
        Assert.Equal([1, 9, 3], list);
    }

    [Fact]
    public void ItemChangesPairIndexWithOldAndNew()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list[2] = 9;

        var replaced = Assert.IsAssignableFrom<IListItemReplacedEventArgs<int>>(Assert.Single(updates));
        var change = Assert.Single(replaced.ItemChanges);
        Assert.Equal(2, change.Index);
        Assert.Equal(3, change.Old);
        Assert.Equal(9, change.New);
    }

    [Fact]
    public void ReplacementIsAlsoVisibleThroughTheCollectionLevelArgs()
    {
        var list = new NotifiableList<int> { 1, 2 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list[0] = 7;

        var replaced = Assert.IsAssignableFrom<ICollectionItemReplacedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1], replaced.OldItems);
        Assert.Equal([7], replaced.NewItems);
    }

    [Fact]
    public void WritingTheSameValueStillRaisesAReplacement()
    {
        var list = new NotifiableList<int> { 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list[0] = 1;

        // 不做等值判断，写一次就报一次
        Assert.Single(updates);
    }
}
