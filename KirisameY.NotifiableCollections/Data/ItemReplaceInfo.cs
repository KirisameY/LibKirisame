namespace KirisameY.NotifiableCollections.Data;

public interface IItemReplaceInfo<out T>
{
    public T Old { get; }
    public T New { get; }
}

public interface IListItemReplaceInfo<out T> : IItemReplaceInfo<T>
{
    public int Index { get; }
}

public static class ItemReplaceInfo
{
    public static IItemReplaceInfo<T> From<T>(T old, T @new) => new ItemReplaceInfo<T>(old, @new);
    public static IListItemReplaceInfo<T> From<T>(int index, T old, T @new) => new ListItemReplaceInfo<T>(index, old, @new);
}

public class ItemReplaceInfo<T>(T old, T @new) : IItemReplaceInfo<T>
{
    public T Old { get; } = old;
    public T New { get; } = @new;
}

public class ListItemReplaceInfo<T>(int index, T old, T @new) : ItemReplaceInfo<T>(old, @new), IListItemReplaceInfo<T>
{
    public int Index { get; } = index;
}