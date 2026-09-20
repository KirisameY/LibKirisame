using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews;

namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableListExtensions
{
    extension<T>(IReadOnlyNotifiableList<T> list)
    {
        [PublicAPI]
        public IReadOnlyNotifiableList<T> AsReadOnlyNotifiableList() => new NotifiableListReadonlyView<T>(list);

        // todo: as INotifyCollection&PropertyChanged
    }

    extension<TSource, TValue>(IReadOnlyNotifiableList<TSource> list)
    {
        [PublicAPI]
        public IReadOnlyNotifiableList<TValue> AsReadOnlyNotifiableList(Func<TSource, TValue> valueSelector) =>
            new NotifiableListReadonlyView<TSource, TValue>(list, valueSelector);
    }

    extension<TSource, TValue>(IReadOnlyNotifiableList<TSource> list) where TSource : TValue
    {
        [PublicAPI]
        public IReadOnlyNotifiableList<TValue> AsReadOnlyNotifiableList(TypeA<TValue> type = default) =>
            new NotifiableListReadonlyView<TSource, TValue>(list, static v => v);
    }
}