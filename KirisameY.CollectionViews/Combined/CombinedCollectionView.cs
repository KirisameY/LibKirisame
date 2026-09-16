using System.Collections;

using KirisameY.Relinq.Extensions;

namespace KirisameY.CollectionViews.Combined;

public class CombinedCollectionView<T>(IReadOnlyCollection<IReadOnlyCollection<T>> collections) : IReadOnlyCollection<T>
{
    public static CombinedCollectionView<T> Create(IReadOnlyCollection<IReadOnlyCollection<T>> collections) => new(collections);
    public static CombinedCollectionView<T> CreateFrozen(IReadOnlyCollection<IReadOnlyCollection<T>> collections) => new([..collections]);

    public IEnumerator<T> GetEnumerator() => collections.Flatten().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => collections.Sum(c => c.Count);
}