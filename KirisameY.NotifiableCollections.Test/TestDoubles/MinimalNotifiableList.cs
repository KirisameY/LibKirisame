using KirisameY.NotifiableCollections.Collections;

namespace KirisameY.NotifiableCollections.Test.TestDoubles;

/// <summary>
///     只实现 <see cref="NotifiableListSimpleBase{T}"/> 的四个抽象成员，其余操作全部交给基类的默认实现。
///     <br/>
///     A list that implements only the four abstract members of <see cref="NotifiableListSimpleBase{T}"/>,
///     leaving every other operation to the base defaults.
/// </summary>
/// <remarks>
///     <see cref="NotifiableList{T}"/> 重写了所有 protected virtual 钩子，所以这些钩子的默认实现
///     只能通过这样一个最小子类来单独验证。
/// </remarks>
public class MinimalNotifiableList<T> : NotifiableListSimpleBase<T>
{
    private readonly List<T> _items = [];

    protected override IReadOnlyList<T> View => _items;

    protected override void SetItem(int index, T item) => _items[index] = item;

    protected override void InsertItem(int index, T item) => _items.Insert(index, item);

    protected override void RemoveItem(int index) => _items.RemoveAt(index);
}
