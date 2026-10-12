using System.Runtime.CompilerServices;

namespace KirisameY.NotifiableCollections.Data;

/// <summary>
///     <see cref="IItemWithIndex{T}"/>> 及相关类型使用的工具方法
///     <br/>
///     Extension util methods for <see cref="IItemWithIndex{T}"/>
/// </summary>
public static class ItemWithIndexExtensions
{
    private static class Cache<T>
    {
        public static readonly ConditionalWeakTable<IReadOnlyList<IItemWithIndex<T>>, IReadOnlyList<int>> IndexesCache = [];
        public static readonly ConditionalWeakTable<IReadOnlyList<IItemWithIndex<T>>, IReadOnlyList<T>> ValuesCache = [];
    }

    extension<T>(IReadOnlyList<IItemWithIndex<T>> source)
    {
        /// <summary>
        ///     获取一个 <see cref="IItemWithIndex{T}"/> 列表的索引项的 <see cref="IReadOnlyList{T}"/> 视图
        ///     <br/>
        ///     Get an indexes <see cref="IReadOnlyList{T}"/> view of an <see cref="IItemWithIndex{T}"/> list
        /// </summary>
        public IReadOnlyList<int> IndexesView => Cache<T>.IndexesCache.GetOrAdd(source, s => new ItemWithIndexIndexesView<T>(s));

        /// <summary>
        ///     获取一个 <see cref="IItemWithIndex{T}"/> 列表的内容项的 <see cref="IReadOnlyList{T}"/> 视图
        ///     <br/>
        ///     Get values <see cref="IReadOnlyList{T}"/> view of an <see cref="IItemWithIndex{T}"/> list
        /// </summary>
        public IReadOnlyList<T> ItemsView => Cache<T>.ValuesCache.GetOrAdd(source, s => new ItemWithIndexItemsView<T>(s));
    }
}