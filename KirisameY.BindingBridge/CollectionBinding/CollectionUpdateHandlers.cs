namespace KirisameY.BindingBridge.CollectionBinding;

// collection
public delegate void CollectionItemAddedHandler<in TElement>(IEnumerable<TElement> addedElements);

public delegate void CollectionItemRemovedHandler<in TElement>(IEnumerable<TElement> removedElements);

public delegate void CollectionItemReplacedHandler<in TElement>(IEnumerable<TElement> oldElements, IEnumerable<TElement> newElements);

public delegate void CollectionResetHandler<in TElement>(IReadOnlyCollection<TElement> collectionView);

// list
public delegate void ListItemAddedHandler<in TItem>(IEnumerable<TItem> addedItems, IEnumerable<int>? indexes);

public delegate void ListItemRemovedHandler<in TItem>(IEnumerable<TItem> removedItems, IEnumerable<int>? indexes);

public delegate void ListItemReplacedHandler<in TItem>(IEnumerable<TItem> oldItems, IEnumerable<TItem> newItems, IEnumerable<int>? indexes);

public delegate void ListItemMovedHandler<in TItem>(IEnumerable<TItem> items, IEnumerable<int> oldIndexes, IEnumerable<int> newIndexes);

public delegate void ListResetHandler<in TItem>(IReadOnlyList<TItem> listView);