using System.Collections;

using JetBrains.Annotations;

using KirisameY.Relinq.Extensions;

namespace KirisameY.CollectionViews.Combined;

public abstract class CombinedCollectionView
{
    [PublicAPI] public static CombinedCollectionView<T> Create<T>(params IReadOnlyCollection<IReadOnlyCollection<T>> collections) => new(collections);
    [PublicAPI] public static CombinedCollectionView<T> CreateFrozen<T>(IReadOnlyCollection<IReadOnlyCollection<T>> collections) => new([..collections]);
}

public class CombinedCollectionView<T>(IReadOnlyCollection<IReadOnlyCollection<T>> collections) : CombinedCollectionView, IReadOnlyCollection<T>
{
    public IEnumerator<T> GetEnumerator() => collections.Flatten().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => collections.Sum(c => c.Count);
}