using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.ListTests;

public class ListReadTests
{
    [Fact]
    public void NewListIsEmpty()
    {
        var list = new NotifiableList<int>();

        Assert.Empty(list);
        Assert.Equal(0, list.Count);
        Assert.False(list.IsReadOnly);
    }

    [Fact]
    public void IndexerReadsByIndex()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void ContainsAndIndexOfUseDefaultEquality()
    {
        var list = new NotifiableList<string> { "a", "b", "a" };

        Assert.True(list.Contains("a"));
        Assert.False(list.Contains("c"));
        Assert.Equal(0, list.IndexOf("a"));
        Assert.Equal(-1, list.IndexOf("c"));
    }

    [Fact]
    public void EnumerationYieldsElementsInOrder()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void CopyToCopiesElementsIntoTheArray()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };
        var array = new int[5];

        list.CopyTo(array, 1);

        Assert.Equal([0, 1, 2, 3, 0], array);
    }

    [Fact]
    public void ReadsAgreeAcrossTheBridgedInterfaces()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        // ICollection<T> / IReadOnlyList<T> 等基接口上的同名成员是显式实现桥接回同一份实现的
        Assert.Equal(3, ((ICollection<int>)list).Count);
        Assert.Equal(3, ((IReadOnlyCollection<int>)list).Count);
        Assert.Equal(3, ((IReadOnlyList<int>)list).Count);
        Assert.Equal(2, ((IList<int>)list)[1]);
        Assert.Equal(2, ((IReadOnlyList<int>)list)[1]);
    }

    [Fact]
    public void WritingThroughTheListInterfaceStillNotifies()
    {
        var list = new NotifiableList<int>();
        var updates = NotifiableCollectionsTestUtils.RecordListUpdates(list);
        var asList = (IList<int>)list;

        asList.Add(1);
        asList.Insert(0, 0);
        asList[1] = 9;
        asList.RemoveAt(0);

        Assert.Equal(4, updates.Count);
        Assert.Equal([9], list);
    }
}
