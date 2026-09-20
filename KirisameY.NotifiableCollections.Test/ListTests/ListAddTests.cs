using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

public class ListAddTests
{
    [Fact]
    public void AddAppendsAndReportsTheIndexItLandedOn()
    {
        var list = new NotifiableList<int>();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Add(42);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([42], added.AddedItems);
        Assert.Equal(0, added.StartIndex);
        Assert.Equal([42], list);
    }

    [Fact]
    public void AddReportsTheIndexRelativeToTheCurrentLength()
    {
        var list = new NotifiableList<int> { 1, 2 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Add(3);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal(2, added.StartIndex);
    }

    [Fact]
    public void AddRangeFromCollectionReportsTheWholeRunAtOneStartIndex()
    {
        var list = new NotifiableList<int> { 1 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.AddRange(new[] { 2, 3 });

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal(1, added.StartIndex);
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void AddRangeFromEnumerableBehavesTheSame()
    {
        var list = new NotifiableList<int>();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.AddRange(Enumerable.Range(1, 3));

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([1, 2, 3], added.AddedItems);
        Assert.Equal(0, added.StartIndex);
    }

    [Fact]
    public void InsertReportsTheInsertionIndex()
    {
        var list = new NotifiableList<int> { 1, 3 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Insert(1, 2);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], added.AddedItems);
        Assert.Equal(1, added.StartIndex);
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void InsertRangeReportsTheInsertionIndex()
    {
        var list = new NotifiableList<int> { 1, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.InsertRange(1, new[] { 2, 3 });

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal(1, added.StartIndex);
        Assert.Equal([1, 2, 3, 4], list);
    }

    [Fact]
    public void InsertRangeFromEnumerableBehavesTheSame()
    {
        var list = new NotifiableList<int> { 1, 4 };
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.InsertRange(1, Enumerable.Range(2, 2));

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2, 3], added.AddedItems);
        Assert.Equal(1, added.StartIndex);
    }
}
