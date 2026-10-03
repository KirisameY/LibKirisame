using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using KirisameY.BindingBridge.CollectionBinding;
using KirisameY.BindingBridge.CollectionBinding.Binding;
using KirisameY.BindingBridge.PropertyBinding;
using KirisameY.BindingBridge.PropertyBinding.Implements;
using KirisameY.BindingBridge.PropertyBinding.Resolver;
using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.Binder;

public abstract partial class DataBinderBase : IDataBinder
{
    #region Property

    public IBindHandle BindPropertyOneWay<TSource, TTarget, TValue>(
        TSource source, Expression<Func<TSource, TValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TValue>> targetProperty
    ) where TSource : notnull where TTarget : notnull
    {
        if (ResolveProperty(sourceProperty) is not IPropertyObservableEndpoint<TSource, TValue> fromEndpoint)
            throw new ArgumentException($"{sourceProperty} is not observable.");
        if (ResolveProperty(targetProperty) is not IPropertyWritableEndpoint<TTarget, TValue> toEndpoint)
            throw new ArgumentException($"{targetProperty} is not writable.");

        return fromEndpoint.OneWayBindTo(toEndpoint, source, target);
    }

    public IBindHandle BindPropertyOneWay<TSource, TTarget, TSourceValue, TTargetValue>(
        TSource source, Expression<Func<TSource, TSourceValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TTargetValue>> targetProperty,
        Func<TSourceValue, TTargetValue> converter
    ) where TSource : notnull where TTarget : notnull
    {
        if (ResolveProperty(sourceProperty) is not IPropertyObservableEndpoint<TSource, TSourceValue> fromEndpoint)
            throw new ArgumentException($"{sourceProperty} is not observable.");
        if (ResolveProperty(targetProperty) is not IPropertyWritableEndpoint<TTarget, TTargetValue> toEndpoint)
            throw new ArgumentException($"{targetProperty} is not writable.");

        return fromEndpoint.OneWayBindTo(toEndpoint, source, target, converter);
    }

    public IBindHandle BindPropertyTwoWay<TSource, TTarget, TValue>(
        TSource source, Expression<Func<TSource, TValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TValue>> targetProperty
    ) where TSource : notnull where TTarget : notnull
    {
        if (ResolveProperty(sourceProperty) is not IPropertyUniversalEndpoint<TSource, TValue> fromEndpoint)
            throw new ArgumentException($"{sourceProperty} is not writable or not observable.");
        if (ResolveProperty(targetProperty) is not IPropertyUniversalEndpoint<TTarget, TValue> toEndpoint)
            throw new ArgumentException($"{targetProperty} is not writable or not observable.");

        return fromEndpoint.TwoWayBindTo(toEndpoint, source, target);
    }

    public IBindHandle BindPropertyTwoWay<TSource, TTarget, TSourceValue, TTargetValue>(
        TSource source, Expression<Func<TSource, TSourceValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TTargetValue>> targetProperty,
        Func<TSourceValue, TTargetValue> converter, Func<TTargetValue, TSourceValue> reversedConverter
    ) where TSource : notnull where TTarget : notnull
    {
        if (ResolveProperty(sourceProperty) is not IPropertyUniversalEndpoint<TSource, TSourceValue> fromEndpoint)
            throw new ArgumentException($"{sourceProperty} is not writable or not observable.");
        if (ResolveProperty(targetProperty) is not IPropertyUniversalEndpoint<TTarget, TTargetValue> toEndpoint)
            throw new ArgumentException($"{targetProperty} is not writable or not observable.");

        return fromEndpoint.TwoWayBindTo(toEndpoint, source, target, converter, reversedConverter);
    }

    protected abstract PropertyUpdateNotifyProxy? ResolveProperty(Type type, MemberInfo? memberInfo);

    #endregion


    #region Collection

    public IBindHandle BindCollection<TSource, TTarget, TElement>(TSource source, TTarget target, TypeA<TElement> elementType = default)
        where TSource : class where TTarget : class
    {
        var sourceEndpoint = ResolveSimpleCollectionSource<TSource, TElement>();
        var targetEndpoint = ResolveSimpleCollectionTarget<TTarget, TElement>();

        if (sourceEndpoint is not (ICollectionObservableEndpoint<TSource, TElement> or IListObservableEndpoint<TSource, TElement>))
            throw new ArgumentException($"{typeof(TSource)} is not a observable collection of {typeof(TElement)}");
        if (targetEndpoint is not (ICollectionObserverEndpoint<TTarget, TElement> or IListObserverEndpoint<TTarget, TElement>))
            throw new ArgumentException($"{typeof(TTarget)} is not a observer for {typeof(TElement)} collection");

        return (sourceEndpoint, targetEndpoint) switch
        {
            (ICollectionObservableEndpoint<TSource, TElement> cse, ICollectionObserverEndpoint<TTarget, TElement> cte) =>
                cse.CollectionBindTo(cte, source, target),
            (IListObservableEndpoint<TSource, TElement> lse, IListObserverEndpoint<TTarget, TElement> lte) =>
                lse.ListBindTo(lte, source, target),
            (ICollectionObservableEndpoint<TSource, TElement> cse, IListObserverEndpoint<TTarget, TElement> lte) =>
                cse.CollectionBindToList(lte, source, target),
            (IListObservableEndpoint<TSource, TElement> lse, ICollectionObserverEndpoint<TTarget, TElement> cte) =>
                lse.ListBindToCollection(cte, source, target),

            _ => throw new Exception("this exception should never be thrown")
        };
    }

    public IBindHandle BindCollection<TSourceObj, TTargetObj, TSourceCollection, TTargetCollection, TElement>(
        TSourceObj sourceObj, Expression<Func<TSourceObj, TSourceCollection>> sourceCollection,
        TTargetObj targetObj, Expression<Func<TTargetObj, TTargetCollection>> targetCollection,
        TypeA<TElement> elementType = default)
        where TSourceObj : class
        where TTargetObj : class
        where TSourceCollection : class
        where TTargetCollection : class
    {
        var sourceEndpoint = ResolveChainedCollectionSource(sourceCollection, elementType);
        var targetEndpoint = ResolveChainedCollectionTarget(targetCollection, elementType);

        if (sourceEndpoint is not (ICollectionObservableEndpoint<TSourceObj, TElement> or IListObservableEndpoint<TSourceObj, TElement>))
            throw new ArgumentException($"{typeof(TSourceCollection)} is not a observable collection of {typeof(TElement)}");
        if (targetEndpoint is not (ICollectionObserverEndpoint<TTargetObj, TElement> or IListObserverEndpoint<TTargetObj, TElement>))
            throw new ArgumentException($"{typeof(TTargetCollection)} is not a observer for {typeof(TElement)} collection");

        return (sourceEndpoint, targetEndpoint) switch
        {
            (ICollectionObservableEndpoint<TSourceObj, TElement> cse, ICollectionObserverEndpoint<TTargetObj, TElement> cte) =>
                cse.CollectionBindTo(cte, sourceObj, targetObj),
            (IListObservableEndpoint<TSourceObj, TElement> lse, IListObserverEndpoint<TTargetObj, TElement> lte) =>
                lse.ListBindTo(lte, sourceObj, targetObj),
            (ICollectionObservableEndpoint<TSourceObj, TElement> cse, IListObserverEndpoint<TTargetObj, TElement> lte) =>
                cse.CollectionBindToList(lte, sourceObj, targetObj),
            (IListObservableEndpoint<TSourceObj, TElement> lse, ICollectionObserverEndpoint<TTargetObj, TElement> cte) =>
                lse.ListBindToCollection(cte, sourceObj, targetObj),

            _ => throw new Exception("this exception should never be thrown")
        };
    }

    protected abstract ICollectionObservableEndpointBase<TObj, TElement>? ResolveCollectionSource<TObj, TElement>() where TObj : class;
    protected abstract ICollectionObserverEndpointBase<TObj, TElement>? ResolveCollectionTarget<TObj, TElement>() where TObj : class;

    #endregion
}