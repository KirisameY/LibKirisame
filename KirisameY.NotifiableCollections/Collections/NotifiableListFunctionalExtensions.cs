using JetBrains.Annotations;

using KirisameY.NotifiableCollections.Data;
using KirisameY.Relinq.Extensions;

namespace KirisameY.NotifiableCollections.Collections;

/// <summary>
///     <see cref="IReadOnlyNotifiableList{T}"/> 的功能性拓展方法：
///     <br/>
///     Functional extension methods for <see cref="IReadOnlyNotifiableList{T}"/>.
/// </summary>
public static class NotifiableListFunctionalExtensions
{
    extension<T>(INotifiableList<T> list)
    {
        /// <inheritdoc cref="INotifiableList{T}.AddRange"/>
        [PublicAPI] public void AddRange(IEnumerable<T> items) => list.AddRange([..items]);

        /// <inheritdoc cref="INotifiableList{T}.InsertRange"/>
        [PublicAPI] public void InsertRange(int index, IEnumerable<T> items) => list.InsertRange(index, [..items]);

        /// <summary>
        ///     将一组指定索引的项插入列表中。
        ///     <br/>
        ///     Inserts a batch of items with given index into the list.
        /// </summary>
        /// <param name="indexes">
        ///     要插入到的目标索引的集合，应当被升序排列且没有重复索引项。
        ///     <br/>
        ///     A collection of target indexes to add to, should be ascending ordered and have no duplicated indexes.
        /// </param>
        /// <param name="items">
        ///     要添加的项的集合，应当与 <paramref name="indexes"/> 逐位对齐。
        ///     <br/>
        ///     A collection of items to add, should be aligned in the same order with <paramref name="indexes"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="indexes"/> 中存在非法或重复的的索引项或未按照索引被升序排列，或长度与 <paramref name="items"/> 不匹配。
        ///     <br/>
        ///     <paramref name="indexes"/> containes invalid or duplicated index or is not ascending ordered by indexes,
        ///     or length mismatched with <paramref name="items"/>.
        /// </exception>
        [PublicAPI] public void MultiInsert(IEnumerable<int> indexes, IEnumerable<T> items) =>
            list.MultiInsert(indexes.ZipChecked(items, ItemWithIndex.From));

        /// <summary>
        ///     将一组指定索引的项替换至列表中。
        ///     <br/>
        ///     Replace list items with a batch of items with given index.
        /// </summary>
        /// <param name="indexes">
        ///     要替换的项所在索引的集合，应当被升序排列且没有重复索引项。
        ///     <br/>
        ///     A collection of indexes of items to replace, should be ascending ordered and have no duplicated indexes.
        /// </param>
        /// <param name="items">
        ///     要替换的项的集合，应当与 <paramref name="indexes"/> 逐位对齐。
        ///     <br/>
        ///     A collection of items to replace, should be aligned in the same order with <paramref name="indexes"/>.
        /// </param>
        /// <returns>
        ///     被替换的项的列表，与 <paramref name="items"/> 逐位对齐。
        ///     <br/>
        ///     A list of replaced items, aligned in the same order with <paramref name="items"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="indexes"/> 中存在非法或重复的的索引项或未按照索引被升序排列，或长度与 <paramref name="items"/> 不匹配。
        ///     <br/>
        ///     <paramref name="indexes"/> containes invalid or duplicated index or is not ascending ordered by indexes,
        ///     or length mismatched with <paramref name="items"/>.
        /// </exception>
        [PublicAPI] public IReadOnlyList<T> MultiReplace(IEnumerable<int> indexes, IEnumerable<T> items) =>
            list.MultiReplace(indexes.ZipChecked(items, ItemWithIndex.From));

        /// <summary>
        ///     就地排序。
        ///     <br/>
        ///     Sorts in place.
        /// </summary>
        [PublicAPI] public void Sort() => list.Sort(Comparer<T>.Default);

        /// <summary>
        ///     按给定比较方式就地排序。
        ///     <br/>
        ///     Sorts in place using the given comparison.
        /// </summary>
        [PublicAPI] public void Sort(Comparison<T> comparison) => list.Sort(Comparer<T>.Create(comparison));

        /// <summary>
        ///     对指定范围就地排序。
        ///     <br/>
        ///     Sorts in place.
        /// </summary>
        [PublicAPI] public void Sort(int index, int count) => list.Sort(index, count, Comparer<T>.Default);

        /// <summary>
        ///     按给定比较方式对指定范围就地排序。
        ///     <br/>
        ///     Sorts in place using the given comparison.
        /// </summary>
        [PublicAPI] public void Sort(int index, int count, Comparison<T> comparison) =>
            list.Sort(index, count, Comparer<T>.Create(comparison));
    }
}