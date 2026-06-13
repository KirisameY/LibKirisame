using JetBrains.Annotations;

using KirisameY.GenericUtils;

namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableDictionaryExtensions
{
    extension<TKey, TValue>(INotifiableDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnly() =>
            new NotifiableDictionaryReadOnlyView<TKey, TValue>(dictionary);

        // todo: as INotifyCollection&PropertyChanged
    }

    extension<TKey, TSourceValue, TValue>(IReadOnlyNotifiableDictionary<TKey, TSourceValue> dictionary)
        where TKey : notnull
        where TSourceValue : TValue
    {
        [PublicAPI]
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsType(TypeA<TValue> type = default) =>
            new NotifiableDictionaryReadOnlyView<TKey, TSourceValue, TValue>(dictionary);
    }
}