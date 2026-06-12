using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Data;

public static class NotifiableCollectionsDataExtensions
{
    extension<T>(IItemReplaceInfo<T> info)
    {
        [PublicAPI]
        public void Deconstruct(out T old, out T @new)
        {
            old  = info.Old;
            @new = info.New;
        }
    }

    extension<T>(IListItemReplaceInfo<T> info)
    {
        [PublicAPI]
        public void Deconstruct(out int index, out T old, out T @new)
        {
            index = info.Index;
            old   = info.Old;
            @new  = info.New;
        }
    }

    extension<T>(ItemWithIndex<T> info)
    {
        [PublicAPI]
        public void Deconstruct(out int index, out T item)
        {
            index = info.Index;
            item  = info.Item;
        }
    }
}