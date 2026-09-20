using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews;

namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableCollectionExtensions
{
    extension<T>(IReadOnlyNotifiableCollection<T> source)
    {
        [PublicAPI]
        public IReadOnlyNotifiableCollection<T> AsReadOnlyNotifiableCollection() => new NotifiableCollectionReadonlyView<T>(source);

        // todo: as INotifyCollection&PropertyChanged
    }

    extension<TSource, TValue>(IReadOnlyNotifiableCollection<TSource> source)
    {
        [PublicAPI]
        public IReadOnlyNotifiableCollection<TValue> AsReadOnlyNotifiableCollection(Func<TSource, TValue> valueSelector) =>
            new NotifiableCollectionReadonlyView<TSource,TValue>(source, valueSelector);
    }

    extension<TSource, TValue>(IReadOnlyNotifiableCollection<TSource> source) where TSource : TValue
    {
        [PublicAPI]
        public IReadOnlyNotifiableCollection<TValue> AsReadOnlyNotifiableCollection(TypeA<TValue> type = default) =>
            new NotifiableCollectionReadonlyView<TSource,TValue>(source, static v => v);
    }
}