using System.Linq.Expressions;
using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding;
using KirisameY.BindingBridge.PropertyBinding.Implements;
using KirisameY.BindingBridge.PropertyBinding.Resolver;

namespace KirisameY.BindingBridge.Binder;

public abstract class DataBinderBase : IDataBinder
{
    #region Property

    public IBindHandle BindPropertyOneWay<TSource, TTarget, TValue>(TSource source, Expression<Func<TSource, TValue>> sourceProperty,
                                                                    TTarget target, Expression<Func<TTarget, TValue>> targetProperty)
    {
        if (ResolveProperty(sourceProperty) is not IObservablePropertyEndpoint<TSource, TValue> fromEndpoint)
            throw new ArgumentException($"{nameof(sourceProperty)} is not observable.");
        if (ResolveProperty(targetProperty) is not IWritablePropertyEndpoint<TTarget, TValue> toEndpoint)
            throw new ArgumentException($"{nameof(targetProperty)} is not writable.");

        return fromEndpoint.OneWayBindTo(toEndpoint, source, target);
    }

    public IBindHandle BindPropertyOneWay<TSource, TTarget, TSourceValue, TTargetValue>(TSource source, Expression<Func<TSource, TSourceValue>> sourceProperty,
                                                                                        TTarget target, Expression<Func<TTarget, TTargetValue>> targetProperty,
                                                                                        Func<TSourceValue, TTargetValue> converter)
    {
        if (ResolveProperty(sourceProperty) is not IObservablePropertyEndpoint<TSource, TSourceValue> fromEndpoint)
            throw new ArgumentException($"{nameof(sourceProperty)} is not observable.");
        if (ResolveProperty(targetProperty) is not IWritablePropertyEndpoint<TTarget, TTargetValue> toEndpoint)
            throw new ArgumentException($"{nameof(targetProperty)} is not writable.");

        return fromEndpoint.OneWayBindTo(toEndpoint, source, target, converter);
    }

    public IBindHandle BindPropertyTwoWay<TSource, TTarget, TValue>(TSource source, Expression<Func<TSource, TValue>> sourceProperty,
                                                                    TTarget target, Expression<Func<TTarget, TValue>> targetProperty)
    {
        if (ResolveProperty(sourceProperty) is not IUniversalPropertyEndpoint<TSource, TValue> fromEndpoint)
            throw new ArgumentException($"{nameof(sourceProperty)} is not writable or not observable.");
        if (ResolveProperty(targetProperty) is not IUniversalPropertyEndpoint<TTarget, TValue> toEndpoint)
            throw new ArgumentException($"{nameof(targetProperty)} is not writable or not observable.");

        return fromEndpoint.TwoWayBindTo(toEndpoint, source, target);
    }

    public IBindHandle BindPropertyTwoWay<TSource, TTarget, TSourceValue, TTargetValue>(TSource source, Expression<Func<TSource, TSourceValue>> sourceProperty,
                                                                                        TTarget target, Expression<Func<TTarget, TTargetValue>> targetProperty,
                                                                                        Func<TSourceValue, TTargetValue> converter, Func<TTargetValue, TSourceValue> reversedConverter)
    {
        if (ResolveProperty(sourceProperty) is not IUniversalPropertyEndpoint<TSource, TSourceValue> fromEndpoint)
            throw new ArgumentException($"{nameof(sourceProperty)} is not writable or not observable.");
        if (ResolveProperty(targetProperty) is not IUniversalPropertyEndpoint<TTarget, TTargetValue> toEndpoint)
            throw new ArgumentException($"{nameof(targetProperty)} is not writable or not observable.");

        return fromEndpoint.TwoWayBindTo(toEndpoint, source, target, converter, reversedConverter);
    }


    private DelegatePropertyEndpoint<TObject, TProperty> ResolveProperty<TObject, TProperty>(Expression<Func<TObject, TProperty>> exp)
    {
        var objParam = exp.Parameters[0];
        var expBody = exp.Body;
        var (parent, member) = expBody switch
        {
            MemberExpression memberExp => (memberExp.Expression, memberExp.Member),
            IndexExpression indexExp   => (indexExp.Object, indexExp.Indexer),

            _ => throw new ArgumentException("Expression is neither a property, field, nor indexer.")
        };
        // todo: 链式适配
        if (parent?.Type.IsAssignableTo(typeof(TObject)) is not true)
            throw new ArgumentException($"Expression is not from an instance of type <{typeof(TObject)}>.");

        // getter compile
        var getter = exp.Compile();

        // setter resolve
        Action<TObject, TProperty>? setter;
        try
        {
            var valueParam = Expression.Parameter(typeof(TProperty));
            var assign = Expression.Assign(expBody, valueParam);
            var lambda = Expression.Lambda<Action<TObject, TProperty>>(assign, objParam, valueParam);
            setter = lambda.Compile();
        }
        catch (ArgumentException)
        {
            setter = null;
        }

        // notify resolve
        // todo: 链式适配
        var updatedNotify = ResolveProperty(typeof(TObject), member);
        var subscriber = (TObject obj, Action update) => { updatedNotify?.SubscribeUpdate.Invoke(obj, update); };
        var unsubscriber = (TObject obj, Action update) => { updatedNotify?.UnsubscribeUpdate.Invoke(obj, update); };

        return (updatedNotify, setter) switch
        {
            (null, null)         => new DelegatePropertyEndpoint<TObject, TProperty>(getter),
            (not null, null)     => new DelegateObservablePropertyEndpoint<TObject, TProperty>(getter, subscriber, unsubscriber),
            (null, not null)     => new DelegateWritablePropertyEndpoint<TObject, TProperty>(getter, setter),
            (not null, not null) => new DelegateUniversalPropertyEndpoint<TObject, TProperty>(getter, setter, subscriber, unsubscriber)
        };
    }

    protected abstract PropertyUpdateNotifyProxy? ResolveProperty(Type type, MemberInfo? memberInfo);

    #endregion
}