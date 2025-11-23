using System.Collections;

using JetBrains.Annotations;

namespace KirisameLib.Collections;

public class DynamicCombinedCollectionView<T>(Func<IEnumerable<IReadOnlyCollection<T>>> collectionsGetter) : IReadOnlyCollection<T>, ICollection<T>
{
    internal DynamicCombinedCollectionView(IEnumerable<IReadOnlyCollection<T>> collections) : this(() => collections) { }

    [MustDisposeResource]
    public IEnumerator<T> GetEnumerator() => collectionsGetter.Invoke().SelectMany(c => c).GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => collectionsGetter.Invoke().Sum(collection => collection.Count);

    bool ICollection<T>.Contains(T item) => collectionsGetter.Invoke().Any(collection => collection.Contains(item));

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        if (arrayIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Param arrayIndex must greater than 0");

        using var enumerator = GetEnumerator();
        for (int i = arrayIndex; i < array.Length && enumerator.MoveNext(); i++)
        {
            array[i] = enumerator.Current;
        }
        if (enumerator.MoveNext())
            throw new ArgumentException("The number of elements in the source collection is greater than"
                                      + " the available space from arrayIndex to the end of the destination array {array}.");
    }

    // Readonly
    bool ICollection<T>.IsReadOnly => true;
    void ICollection<T>.Add(T item) => throw new NotSupportedException();
    void ICollection<T>.Clear() => throw new NotSupportedException();
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
}