using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace KirisameY.NotifiableCollections.Test.ObservableTests;

/// <summary>
///     ObservableTests 这组测试的公共辅助方法。
/// </summary>
public static class ObservableTestUtils
{
    /// <summary>
    ///     订阅标准库的 <see cref="INotifyCollectionChanged.CollectionChanged"/>，按触发顺序收集事件参数。
    /// </summary>
    public static List<NotifyCollectionChangedEventArgs> RecordCollectionChanged(INotifyCollectionChanged source)
    {
        List<NotifyCollectionChangedEventArgs> events = [];
        source.CollectionChanged += (_, args) => events.Add(args);
        return events;
    }

    /// <summary>
    ///     订阅标准库的 <see cref="INotifyPropertyChanged.PropertyChanged"/>，按触发顺序收集属性名。
    /// </summary>
    public static List<string?> RecordPropertyChanged(INotifyPropertyChanged source)
    {
        List<string?> names = [];
        source.PropertyChanged += (_, args) => names.Add(args.PropertyName);
        return names;
    }

    /// <summary>
    ///     把 <see cref="NotifyCollectionChangedEventArgs"/> 上的项集合取成强类型序列，方便直接断言。
    /// </summary>
    /// <remarks>
    ///     <c>NewItems</c> / <c>OldItems</c> 的静态类型是非泛型的 <see cref="IList"/>
    ///     且可能为 <c>null</c>（如 Reset），所以统一在这里收口。
    /// </remarks>
    public static IEnumerable<T> Elements<T>(IEnumerable? source)
    {
        return source is null ? [] : source.Cast<T>();
    }

    /// <summary>
    ///     取出一串事件各自携带的新项并拉平，用于整体断言。
    /// </summary>
    public static IEnumerable<T> NewItemsOf<T>(IEnumerable<NotifyCollectionChangedEventArgs> events) =>
        events.SelectMany(e => Elements<T>(e.NewItems));

    /// <summary>
    ///     取出一串事件各自携带的旧项并拉平，用于整体断言。
    /// </summary>
    public static IEnumerable<T> OldItemsOf<T>(IEnumerable<NotifyCollectionChangedEventArgs> events) =>
        events.SelectMany(e => Elements<T>(e.OldItems));

    /// <summary>
    ///     只经过非泛型 <see cref="IEnumerable.GetEnumerator"/> 手工遍历一遍，返回沿途取到的元素。
    /// </summary>
    /// <remarks>
    ///     刻意不用 <c>Enumerable.Cast</c>：源自身实现了 <c>IEnumerable&lt;T&gt;</c> 时 Cast 会抄近路，
    ///     直接把泛型枚举器原样返回，那样就完全测不到非泛型这条路径了。
    /// </remarks>
    public static object?[] Enumerate(IEnumerable source)
    {
        List<object?> items = [];
        var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext()) items.Add(enumerator.Current);
        return [..items];
    }
}
