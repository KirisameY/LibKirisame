using KirisameLib.Extensions.Enumerable;

namespace KirisameLib.Core.Test.Extensions;

public class RangeEnumerableTests
{
    [Test]
    public void ForeachTest()
    {
        List<int> list = [];
        foreach (int i in 5..^2)
        {
            list.Add(i);
        }
        int[] arr = [5, 4, 3, 2, 1, 0, -1];
        Assert.That(list, Is.EquivalentTo(arr));
    }
}