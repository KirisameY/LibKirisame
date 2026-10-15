using KirisameY.BindingBridge.CollectionBinding.SimpleCollectionObservers;

namespace KirisameY.BindingBridge.Test.TestDoubles;

/// <summary>
///     自定义列表观察者：内部真的拿一份 <see cref="List{T}"/> 兜住，每个方法都照做。
/// </summary>
/// <remarks>
///     于是"把收到的调用依序照做一遍"的结果就是 <see cref="Items"/> 本身，
///     可以直接拿来跟源比对——用不着去数端点在中间拆了几步、按什么顺序拆。
/// </remarks>
public class SimpleListObserverTarget : ISimpleListObserver<int>
{
    /// <summary>观察者眼里的列表内容。</summary>
    public List<int> Items { get; } = [];

    /// <summary>按顺序记下收到的每一次调用，用来观察"被问了什么"。</summary>
    public List<string> Calls { get; } = [];

    public int Find(int item)
    {
        Calls.Add($"Find({item})");
        return Items.IndexOf(item);
    }

    public void Add(int item)
    {
        Calls.Add($"Add({item})");
        Items.Add(item);
    }

    public void Insert(int index, int item)
    {
        Calls.Add($"Insert({index},{item})");
        Items.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        Calls.Add($"RemoveAt({index})");
        Items.RemoveAt(index);
    }

    public void Replace(int index, int newItem)
    {
        Calls.Add($"Replace({index},{newItem})");
        Items[index] = newItem;
    }

    public void Move(int oldIndex, int newIndex)
    {
        Calls.Add($"Move({oldIndex},{newIndex})");
        var item = Items[oldIndex];
        Items.RemoveAt(oldIndex);
        Items.Insert(newIndex, item);
    }

    public void Clear()
    {
        Calls.Add("Clear");
        Items.Clear();
    }
}

/// <summary>
///     一个"谁都认不出来"的列表观察者：<see cref="Find"/> 一律回 <c>-1</c>，其余方法只记不做。
/// </summary>
/// <remarks>
///     用来钉"找不到就别动"：源里有、观察者这边没有的项，
///     端点不该拿着 <c>-1</c> 去 <see cref="RemoveAt"/> 或者去改 <c>[-1]</c>。
/// </remarks>
public class BlindListObserver : ISimpleListObserver<int>
{
    /// <summary>按顺序记下收到的每一次调用。</summary>
    public List<string> Calls { get; } = [];

    public int Find(int item)
    {
        Calls.Add($"Find({item})");
        return -1;
    }

    public void Add(int item) => Calls.Add($"Add({item})");

    public void Insert(int index, int item) => Calls.Add($"Insert({index},{item})");

    public void RemoveAt(int index) => Calls.Add($"RemoveAt({index})");

    public void Replace(int index, int newItem) => Calls.Add($"Replace({index},{newItem})");

    public void Move(int oldIndex, int newIndex) => Calls.Add($"Move({oldIndex},{newIndex})");

    public void Clear() => Calls.Add("Clear");
}

/// <summary>
///     自定义集合观察者：内部拿一份 <see cref="List{T}"/> 兜住，每个方法都照做。
/// </summary>
/// <remarks>
///     接口本身不承诺顺序，装它的容器自然也不该有顺序；这里用列表只是为了断言时省掉排序。
/// </remarks>
public class SimpleCollectionObserverTarget : ISimpleCollectionObserver<int>
{
    /// <summary>观察者眼里的集合内容。</summary>
    public List<int> Items { get; } = [];

    /// <summary>按顺序记下收到的每一次调用，用来观察"被问了什么"。</summary>
    public List<string> Calls { get; } = [];

    public void Add(int element)
    {
        Calls.Add($"Add({element})");
        Items.Add(element);
    }

    public void Remove(int element)
    {
        Calls.Add($"Remove({element})");
        Items.Remove(element);
    }

    public void Replace(int oldElement, int newElement)
    {
        Calls.Add($"Replace({oldElement},{newElement})");
        var index = Items.IndexOf(oldElement);
        if (index >= 0) Items[index] = newElement;
    }

    public void Clear()
    {
        Calls.Add("Clear");
        Items.Clear();
    }
}
