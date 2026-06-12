namespace KirisameY.NotifiableCollections.Collections;

public static class NotifiableListExtensions
{
    extension<T>(INotifiableList<T> list)
    {
        public IReadOnlyNotifiableList<T> AsReadOnly() => new NotifiableListReadOnlyView<T>(list);

        // todo: as INotifyCollection&PropertyChanged
    }
}