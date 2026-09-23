namespace KirisameY.BindingBridge.PropertyBinding;

public interface IPropertyEndpoint<in TObject, out TProperty>
{
    TProperty GetValue(TObject? obj);
}

public interface IObservablePropertyEndpoint<in TObject, out TProperty> : IPropertyEndpoint<TObject, TProperty>
{
    void SubscribeUpdate(TObject? obj, Action<TProperty> handler);
    void UnsubscribeUpdate(TObject? obj, Action<TProperty> handler);
}

public interface IWritablePropertyEndpoint<in TObject, TProperty> : IPropertyEndpoint<TObject, TProperty>
{
    void SetValue(TObject? obj, TProperty value);
}

public interface IUniversalPropertyEndpoint<in TObject, TProperty> : IObservablePropertyEndpoint<TObject, TProperty>,
                                                                     IWritablePropertyEndpoint<TObject, TProperty>;