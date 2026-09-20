using System.Collections;

using KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;
using KirisameY.NotifiableCollections.EventArgs;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews;

internal class NotifiableCollectionReadonlyView<T>(IReadOnlyNotifiableCollection<T> source) : IReadOnlyNotifiableCollection<T>
{
    public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;


    public event EventHandler<CollectionUpdateEventArgs<T>>? CollectionUpdated
    {
        add => source.CollectionUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => source.CollectionUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<CollectionUpdateEventArgs<T>>,
        EventHandler<CollectionUpdateEventArgs<T>>
    > HandlerCache => field ??= new(handler => (_, args) => handler.Invoke(this, args));
}

internal class NotifiableCollectionReadonlyView<TSource, TValue>(
    IReadOnlyNotifiableCollection<TSource> source, Func<TSource, TValue> valueSelector
) : IReadOnlyNotifiableCollection<TValue>
{
    public IEnumerator<TValue> GetEnumerator() => source.Select(valueSelector).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => source.Count;


    public event EventHandler<CollectionUpdateEventArgs<TValue>>? CollectionUpdated
    {
        add => source.CollectionUpdated += value is null ? null : HandlerCache.WrapNew(value);
        remove => source.CollectionUpdated -= value is null ? null : HandlerCache.RemoveWrapped(value);
    }

    private CountedWeakRefWrapper<
        EventHandler<CollectionUpdateEventArgs<TValue>>,
        EventHandler<CollectionUpdateEventArgs<TSource>>
    > HandlerCache => field ??= new(handler => (_, args) =>
    {
        CollectionUpdateEventArgs<TValue>? newArgs = args switch
        {
            ICollectionItemAddedEventArgs<TSource> added => new CollectionItemAddedEventArgs<TValue>(
                this, [..added.AddedItems.Select(valueSelector)]
            ),
            ICollectionItemClearedEventArgs<TSource> cleared => new CollectionItemClearedEventArgs<TValue>(
                this, [..cleared.RemovedItems.Select(valueSelector)]
            ),
            ICollectionItemRemovedEventArgs<TSource> removed => new CollectionItemRemovedEventArgs<TValue>(
                this, [..removed.RemovedItems.Select(valueSelector)]
            ),
            ICollectionItemReplacedEventArgs<TSource> replaced => new CollectionItemReplacedEventArgs<TValue>(
                this, [..replaced.OldItems.Select(valueSelector)], [..replaced.NewItems.Select(valueSelector)]
            ),

            // todo: 不属于这几个接口的类型会报错，这里可以置null然后不发解决，但是对于List直接套只读Collection包装器的情况它就不应该能发出来
            //       这么看来应该把元素类型转换和集合类型上提迁移到接口定义里才对
            _ => throw new InvalidDataException($"Unexpected type {args.GetType()} of args")
        };

        handler.Invoke(this, newArgs);
    });
}