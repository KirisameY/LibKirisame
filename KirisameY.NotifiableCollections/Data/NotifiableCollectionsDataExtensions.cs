using JetBrains.Annotations;

namespace KirisameY.NotifiableCollections.Data;

/// <summary>
///     让变更信息类型支持解构（<c>var (a, b) = info;</c>）的扩展方法。
///     <br/>
///     Extension methods that let change-info types be deconstructed (<c>var (a, b) = info;</c>).
/// </summary>
public static class NotifiableCollectionsDataExtensions
{
    extension<T>(IItemReplaceInfo<T> info)
    {
        /// <summary>
        ///     解构出替换前后的值。
        ///     <br/>
        ///     Deconstructs the values before and after the replacement.
        /// </summary>
        /// <param name="old">
        ///     替换前的旧值。
        ///     <br/>
        ///     The value before the replacement.
        /// </param>
        /// <param name="new">
        ///     替换后的新值。
        ///     <br/>
        ///     The value after the replacement.
        /// </param>
        [PublicAPI]
        public void Deconstruct(out T old, out T @new)
        {
            old  = info.Old;
            @new = info.New;
        }
    }

    extension<T>(IListItemReplaceInfo<T> info)
    {
        /// <summary>
        ///     解构出发生替换的索引与替换前后的值。
        ///     <br/>
        ///     Deconstructs the index at which the replacement happened together with the values before and after it.
        /// </summary>
        /// <param name="index">
        ///     发生替换的索引。
        ///     <br/>
        ///     The index at which the replacement happened.
        /// </param>
        /// <param name="old">
        ///     替换前的旧值。
        ///     <br/>
        ///     The value before the replacement.
        /// </param>
        /// <param name="new">
        ///     替换后的新值。
        ///     <br/>
        ///     The value after the replacement.
        /// </param>
        [PublicAPI]
        public void Deconstruct(out int index, out T old, out T @new)
        {
            index = info.Index;
            old   = info.Old;
            @new  = info.New;
        }
    }

    extension<T>(IItemWithIndex<T> info)
    {
        /// <summary>
        ///     解构出索引与对应元素。
        ///     <br/>
        ///     Deconstructs the index together with the element at that index.
        /// </summary>
        /// <param name="index">
        ///     元素所在的索引。
        ///     <br/>
        ///     The index the element is at.
        /// </param>
        /// <param name="item">
        ///     该索引上的元素。
        ///     <br/>
        ///     The element at that index.
        /// </param>
        [PublicAPI]
        public void Deconstruct(out int index, out T item)
        {
            index = info.Index;
            item  = info.Item;
        }
    }
}
