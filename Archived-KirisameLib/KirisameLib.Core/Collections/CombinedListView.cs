using System.Collections.Immutable;

namespace KirisameLib.Collections;

public class CombinedListView<T>(params IEnumerable<IReadOnlyList<T>> lists) : DynamicCombinedListView<T>(lists.ToImmutableArray());