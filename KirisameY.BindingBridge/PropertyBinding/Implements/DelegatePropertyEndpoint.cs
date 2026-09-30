namespace KirisameY.BindingBridge.PropertyBinding.Implements;

internal class DelegatePropertyEndpoint<TObject, TProperty>(Func<TObject, TProperty> getter) : IPropertyEndpoint<TObject, TProperty>
{
    public TProperty GetValue(TObject obj) => getter.Invoke(obj);
}

internal class DelegatePropertyObservableEndpoint<TObject, TProperty>(
    Func<TObject, TProperty> getter, Action<TObject, Action> subscriber, Action<TObject, Action> unsubscriber
) : DelegatePropertyEndpoint<TObject, TProperty>(getter), IPropertyObservableEndpoint<TObject, TProperty>
{
    public void SubscribeUpdate(TObject obj, Action handler) => subscriber.Invoke(obj, handler);
    public void UnsubscribeUpdate(TObject obj, Action handler) => unsubscriber.Invoke(obj, handler);
}

internal class DelegatePropertyWritableEndpoint<TObject, TProperty>(
    Func<TObject, TProperty> getter, Action<TObject, TProperty> setter
) : DelegatePropertyEndpoint<TObject, TProperty>(getter), IPropertyWritableEndpoint<TObject, TProperty>
{
    public void SetValue(TObject obj, TProperty value) => setter.Invoke(obj, value);
}

internal class DelegatePropertyUniversalEndpoint<TObject, TProperty>(
    Func<TObject, TProperty> getter, Action<TObject, TProperty> setter,
    Action<TObject, Action> subscriber, Action<TObject, Action> unsubscriber
) : DelegatePropertyEndpoint<TObject, TProperty>(getter), IPropertyUniversalEndpoint<TObject, TProperty>
{
    public void SetValue(TObject obj, TProperty value) => setter.Invoke(obj, value);

    public void SubscribeUpdate(TObject obj, Action handler) => subscriber.Invoke(obj, handler);
    public void UnsubscribeUpdate(TObject obj, Action handler) => unsubscriber.Invoke(obj, handler);
}