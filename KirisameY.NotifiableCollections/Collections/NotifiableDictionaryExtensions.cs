namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableDictionaryExtensions
{
    extension<TKey, TValue>(INotifiableDictionary<TKey, TValue> dictionary)
    {
        public IReadOnlyNotifiableDictionary<TKey, TValue> AsReadOnly() =>
            new NotifiableDictionaryReadOnlyView<TKey, TValue>(dictionary);

        // todo: as INotifyCollection&PropertyChanged
    }
}
