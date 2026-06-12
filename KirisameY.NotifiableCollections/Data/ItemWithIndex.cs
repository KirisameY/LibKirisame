namespace KirisameY.NotifiableCollections.Data;

public interface IItemWithIndex<out T>
{
    public int Index { get; }
    public T Item { get; }
}

public static class ItemWithIndex
{
    public static IItemWithIndex<T> From<T>(int index, T item) => new ItemWithIndex<T>(index, item);
    public static IItemWithIndex<T> WithIndex<T>(this T item, int index) => From(index, item);
}

public record ItemWithIndex<T>(int Index, T Item) : IItemWithIndex<T>;