using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     <see cref="CommonCollectionExtensions.ToNotifiableList{T}"/>：现在返回 <see cref="INotifiableList{T}"/>，
///     而不是具体实现类型。
/// </summary>
public class ListCreationTests
{
    [Fact]
    public void ToNotifiableListKeepsTheSourceOrder()
    {
        INotifiableList<int> list = new[] { 3, 1, 2 }.ToNotifiableList();

        Assert.Equal([3, 1, 2], list);
    }

    [Fact]
    public void ToNotifiableListProducesAListThatStillNotifies()
    {
        var list = new[] { 1 }.ToNotifiableList();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);

        list.Add(2);

        var added = Assert.IsAssignableFrom<IListItemAddedEventArgs<int>>(Assert.Single(updates));
        Assert.Equal([2], added.AddedItems);
        Assert.Equal([1], added.Indexes);
        Assert.Equal([1, 2], list);
    }

    [Fact]
    public void ToNotifiableListFromAnEmptySourceIsEmpty()
    {
        INotifiableList<int> list = Array.Empty<int>().ToNotifiableList();

        Assert.Empty(list);
    }

    [Fact]
    public void ToNotifiableListEnumeratesTheSourceOnlyOnce()
    {
        var enumerations = 0;
        IEnumerable<int> Source()
        {
            enumerations++;
            yield return 1;
            yield return 2;
        }

        var list = Source().ToNotifiableList();

        Assert.Equal(1, enumerations);
        Assert.Equal([1, 2], list);
    }
}
