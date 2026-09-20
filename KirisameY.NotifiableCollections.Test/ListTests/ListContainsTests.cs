using KirisameY.NotifiableCollections.Collections;

// 本文件的目的是测 Contains 方法本身，所以刻意直接调它而不是改用 Assert.Contains：
// xUnit 的 Assert.Contains 只在目标是 ISet<T> 时才会转交给集合自己的实现，
// 其余情况一律自己枚举、用自己的比较器，压根走不到这个 Contains 上。
#pragma warning disable xUnit2017

namespace KirisameY.NotifiableCollections.Test.ListTests;

/// <summary>
///     NotifiableList.Contains 本身的行为：按元素的默认相等语义判断存在性。
/// </summary>
public class ListContainsTests
{
    [Fact]
    public void EmptyListContainsNothing()
    {
        var list = new NotifiableList<int>();

        Assert.False(list.Contains(0));
        Assert.False(list.Contains(1));
    }

    [Fact]
    public void ContainsFindsAnElementAtAnyPosition()
    {
        var list = new NotifiableList<int> { 1, 2, 3 };

        Assert.True(list.Contains(1)); // 第一个
        Assert.True(list.Contains(2)); // 中间
        Assert.True(list.Contains(3)); // 最后一个
        Assert.False(list.Contains(4));
    }

    [Fact]
    public void ContainsToleratesDuplicates()
    {
        var list = new NotifiableList<string> { "a", "a", "b" };

        Assert.True(list.Contains("a"));
        Assert.False(list.Contains("c"));
    }

    [Fact]
    public void ContainsUsesTheDefaultStringComparer()
    {
        var list = new NotifiableList<string> { "abc" };

        // 默认比较是序数的：大小写和空白都不会被折叠
        Assert.True(list.Contains("abc"));
        Assert.False(list.Contains("ABC"));
        Assert.False(list.Contains(" abc"));
    }

    [Fact]
    public void ContainsFindsNullInAReferenceTypedList()
    {
        var list = new NotifiableList<string?> { "a", null };

        Assert.True(list.Contains(null));

        // 没有 null 这个元素时就是普通的不命中，而不是抛异常
        Assert.False(new NotifiableList<string?>().Contains(null));
    }

    [Fact]
    public void ContainsComparesByValueRatherThanByReference()
    {
        var list = new NotifiableList<Box> { new(1) };

        // 换一个实例，只要 Equals 说相等就算命中
        Assert.True(list.Contains(new Box(1)));
        Assert.False(list.Contains(new Box(2)));
    }

    [Fact]
    public void ContainsAgreesAcrossTheBridgedInterfaces()
    {
        var list = new NotifiableList<int> { 1, 2 };

        // ICollection<T>.Contains 就是上面那个公开方法，没有另开一份实现
        Assert.True(((ICollection<int>)list).Contains(1));
        Assert.False(((ICollection<int>)list).Contains(3));
    }

    private sealed record Box(int Value);
}
