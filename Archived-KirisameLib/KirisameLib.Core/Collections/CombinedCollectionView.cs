using System.Collections.Immutable;

namespace KirisameLib.Collections;

public class CombinedCollectionView<T>(params IEnumerable<IReadOnlyCollection<T>> collections) : DynamicCombinedCollectionView<T>(collections.ToImmutableArray());