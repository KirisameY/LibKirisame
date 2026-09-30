namespace KirisameY.BindingBridge.PropertyBinding;

public interface IPropertyEndpoint<in TObject, out TProperty>
{
    TProperty GetValue(TObject obj);
}

public interface IPropertyObservableEndpoint<in TObject, out TProperty> : IPropertyEndpoint<TObject, TProperty>
{
    void SubscribeUpdate(TObject obj, Action handler);
    void UnsubscribeUpdate(TObject obj, Action handler);
}

public interface IPropertyWritableEndpoint<in TObject, TProperty> : IPropertyEndpoint<TObject, TProperty>
{
    void SetValue(TObject obj, TProperty value);
}

public interface IPropertyUniversalEndpoint<in TObject, TProperty> : IPropertyObservableEndpoint<TObject, TProperty>,
                                                                     IPropertyWritableEndpoint<TObject, TProperty>;