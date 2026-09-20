using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections.WrappedViews;

namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableDictionaryExtensions
{
    extension<TKey, TValue>(IReadOnlyNotifiableDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnlyNotifiableDictionary() =>
            new NotifiableDictionaryReadonlyView<TKey, TValue>(dictionary);

        // todo: as INotifyCollection&PropertyChanged
    }

    extension<TKey, TSourceValue, TValue>(IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary)
        where TKey : notnull
    {
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnlyNotifiableDictionary(Func<TSourceValue, TValue> valueSelector) =>
            new NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(dictionary, valueSelector);
    }

    extension<TKey, TSourceValue, TValue>(IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary)
        where TKey : notnull
        where TSourceValue : TValue
    {
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnlyNotifiableDictionary(TypeA<TValue> type = default) =>
            new NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(dictionary, static s => s);
    }
}