using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using KirisameY.BindingBridge.CollectionBinding;
using KirisameY.BindingBridge.CollectionBinding.Implements;
using KirisameY.BindingBridge.PropertyBinding.Implements;
using KirisameY.BindingBridge.PropertyBinding.Resolver;
using KirisameY.GenericUtils;
using KirisameY.Relinq.Extensions;

namespace KirisameY.BindingBridge.Binder;

public abstract partial class DataBinderBase
{
    private readonly Lock _cacheLock = new();
    private readonly Dictionary<(Type Type, MemberInfo? Member), PropertyUpdateNotifyProxy?> _propertyNotifyCache = [];
    private readonly Dictionary<(Type Type, string Exp), object> _propertyCache = [];
    private readonly Dictionary<(Type collectionType, Type ElementType, string Exp), object?> _collectionSourceCache = [];
    private readonly Dictionary<(Type collectionType, Type ElementType, string Exp), object?> _collectionTargetCache = [];

    private DelegatePropertyEndpoint<TObject, TProperty> ResolveProperty<TObject, TProperty>(Expression<Func<TObject, TProperty>> exp)
        where TObject : notnull
    {
        // find cache
        using var _ = _cacheLock.EnterScope();

        var cacheKey = (typeof(TObject), exp.ToString());
        if (_propertyCache.TryGetValue(cacheKey, out var value))
        {
            return (DelegatePropertyEndpoint<TObject, TProperty>)value;
        }

        // notify resolve
        var notify = PropertyResolveUtils.ResolveChainedNotify(exp, ResolveProperty, _propertyNotifyCache);

        // getter compile
        var getter = exp.Compile();

        // setter resolve
        var setter = PropertyResolveUtils.GetSetter(exp);

        // fin
        Action<TObject, Action>? subscriber = notify is null ? null : (obj, action) => notify.Value.SubscribeUpdate(obj, action);
        Action<TObject, Action>? unsubscriber = notify is null ? null : (obj, action) => notify.Value.UnsubscribeUpdate(obj, action);
        var result = (notify, setter) switch
        {
            (null, null)         => new DelegatePropertyEndpoint<TObject, TProperty>(getter),
            (not null, null)     => new DelegatePropertyObservableEndpoint<TObject, TProperty>(getter, subscriber!, unsubscriber!),
            (null, not null)     => new DelegatePropertyWritableEndpoint<TObject, TProperty>(getter, setter),
            (not null, not null) => new DelegatePropertyUniversalEndpoint<TObject, TProperty>(getter, setter, subscriber!, unsubscriber!)
        };
        _propertyCache.Add(cacheKey, result);
        return result;
    }

    private ICollectionObservableEndpointBase<TObj, TElement>? ResolveSimpleCollectionSource<TObj, TElement>() where TObj : class
    {
        using var _ = _cacheLock.EnterScope();

        var cacheKey = (typeof(TObj), typeof(TElement), "");
        if (_collectionSourceCache.TryGetValue(cacheKey, out var value))
        {
            return (ICollectionObservableEndpointBase<TObj, TElement>?)value;
        }

        var endpoint = ResolveCollectionSource<TObj, TElement>();
        _collectionSourceCache.Add(cacheKey, endpoint);
        return endpoint;
    }

    private ICollectionObserverEndpointBase<TObj, TElement>? ResolveSimpleCollectionTarget<TObj, TElement>() where TObj : class
    {
        using var _ = _cacheLock.EnterScope();

        var cacheKey = (typeof(TObj), typeof(TElement), "");
        if (_collectionTargetCache.TryGetValue(cacheKey, out var value))
        {
            return (ICollectionObserverEndpointBase<TObj, TElement>?)value;
        }

        var endpoint = ResolveCollectionTarget<TObj, TElement>();
        _collectionTargetCache.Add(cacheKey, endpoint);
        return endpoint;
    }

    // ReSharper disable once UnusedParameter.Local
    private ICollectionObservableEndpointBase<TObj, TElement>? ResolveChainedCollectionSource<TObj, TCollection, TElement>(
        Expression<Func<TObj, TCollection>> exp, TypeA<TElement> elementType
    ) where TObj : class where TCollection : class
    {
        using var _ = _cacheLock.EnterScope();

        var cacheKey = (typeof(TObj), typeof(TElement), exp.ToString());
        if (_collectionSourceCache.TryGetValue(cacheKey, out var value))
        {
            return (ICollectionObservableEndpointBase<TObj, TElement>?)value;
        }

        var collectionSource = ResolveCollectionSource<TCollection, TElement>();
        var notify = PropertyResolveUtils.ResolveChainedNotify(exp, ResolveProperty, _propertyNotifyCache);
        ICollectionObservableEndpointBase<TObj, TElement>? endpoint = collectionSource switch
        {
            null => null,
            IListObservableEndpoint<TCollection, TElement> ep =>
                new ChainedListObservableEndpoint<TObj, TCollection, TElement>(ep, exp.Compile(), notify),
            ICollectionObservableEndpoint<TCollection, TElement> ep =>
                new ChainedCollectionObservableEndpoint<TObj, TCollection, TElement>(ep, exp.Compile(), notify),
            _ => throw new Exception($"Unexpected collection source endpoint type {collectionSource.GetType()}")
        };

        _collectionSourceCache.Add(cacheKey, endpoint);
        return endpoint;
    }

    // ReSharper disable once UnusedParameter.Local
    private ICollectionObserverEndpointBase<TObj, TElement>? ResolveChainedCollectionTarget<TObj, TCollection, TElement>(
        Expression<Func<TObj, TCollection>> exp, TypeA<TElement> elementType
    ) where TObj : class where TCollection : class
    {
        using var _ = _cacheLock.EnterScope();

        var cacheKey = (typeof(TObj), typeof(TElement), exp.ToString());
        if (_collectionTargetCache.TryGetValue(cacheKey, out var value))
        {
            return (ICollectionObserverEndpointBase<TObj, TElement>?)value;
        }

        var collectionTarget = ResolveCollectionTarget<TCollection, TElement>();
        ICollectionObserverEndpointBase<TObj, TElement>? endpoint = collectionTarget switch
        {
            null => null,
            IListObserverEndpoint<TCollection, TElement> ep =>
                new ChainedListObserverEndpoint<TObj, TCollection, TElement>(ep, exp.Compile()),
            ICollectionObserverEndpoint<TCollection, TElement> ep =>
                new ChainedCollectionObserverEndpoint<TObj, TCollection, TElement>(ep, exp.Compile()),
            _ => throw new Exception($"Unexpected collection target endpoint type {collectionTarget.GetType()}")
        };

        _collectionTargetCache.Add(cacheKey, endpoint);
        return endpoint;
    }
}

file static class PropertyResolveUtils
{
    public static Action<TObject, TProperty>? GetSetter<TObject, TProperty>(Expression<Func<TObject, TProperty>> exp)
    {
        var objParam = exp.Parameters[0];
        var expBody = exp.Body;

        var targetExp = expBody switch
        {
            MemberExpression memberExp => (Expression)memberExp,
            IndexExpression indexExp   => indexExp,
            BinaryExpression
            {
                NodeType: ExpressionType.ArrayIndex,
                Left: var array,
                Right: var i
            } => Expression.MakeIndex(array, null, [i]),
            MethodCallExpression
            {
                Method.IsSpecialName: true,
                Method.Name: var methodName,
                Object: { Type: var type } obj,
                Arguments: var arguments
            } when (
                methodName.Split('_') is ["get", var propName] &&
                type.GetProperty(propName) is { } property &&
                arguments.All(e => e is ConstantExpression)
            ) => property.GetIndexParameters() is []
                ? Expression.Property(obj, property)
                : Expression.MakeIndex(obj, property, arguments),

            _ => throw new ArgumentException("Expression is neither a property, field, nor indexer with constant index.")
        };

        Action<TObject, TProperty>? setter;
        try
        {
            var valueParam = Expression.Parameter(typeof(TProperty));
            var assign = Expression.Assign(targetExp, valueParam);
            var lambda = Expression.Lambda<Action<TObject, TProperty>>(assign, objParam, valueParam);
            setter = lambda.Compile();
        }
        catch (ArgumentException)
        {
            setter = null;
        }

        return setter;
    }

    public static PropertyUpdateNotifyProxy? ResolveChainedNotify(
        LambdaExpression sourceExp,
        Func<Type, MemberInfo?, PropertyUpdateNotifyProxy?> propertyResolver,
        Dictionary<(Type Type, MemberInfo? Member), PropertyUpdateNotifyProxy?> cacheDict
    )
    {
        var objParam = sourceExp.Parameters[0];
        var expBody = sourceExp.Body;

        var exp = expBody;

        // resolve property chain
        List<(Func<object, object> ParentGetter, PropertyUpdateNotifyProxy? Notify)> notifies = [];
        while (true)
        {
            if (exp == objParam) break;
            var (parent, member) = expBody switch
            {
                MemberExpression { Expression: { } e, Member: var m }                 => (e, m),
                IndexExpression { Object: { } o, Indexer: var i }                     => (o, i),
                BinaryExpression { NodeType: ExpressionType.ArrayIndex, Left: { } l } => (l, null),
                MethodCallExpression
                {
                    Method.IsSpecialName: true,
                    Method.Name: var methodName,
                    Object: { Type: var type } obj,
                    Arguments: var arguments
                } when (
                    methodName.Split('_') is ["get", var propName] &&
                    type.GetProperty(propName) is { } property &&
                    arguments.All(e => e is ConstantExpression)
                ) => (obj, property),
                _ => throw new ArgumentException($"Expression {sourceExp} is neither a instance property, field, "
                                               + $"indexer with constant index, nor the source instance.")
            };

            var cacheKey = (parent.Type, member);
            if (!cacheDict.TryGetValue(cacheKey, out var notify))
            {
                notify = cacheDict[cacheKey] = propertyResolver.Invoke(parent.Type, member);
            }

            // wrap param and return value into object
            var newParam = Expression.Parameter(typeof(object));
            var parentGetter = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Lambda(parent, objParam),
                        Expression.Convert(newParam, objParam.Type)
                    ),
                    typeof(object)
                ), newParam
            ).Compile();

            notifies.Add((parentGetter, notify));

            exp = parent;
        }
        notifies.Reverse(); // before reverse, root is at the last

        return notifies switch
        {
            [(_, { } n)]        => n,
            [.., (_, not null)] => GetChainedNotify(notifies),
            _                   => null
        };
    }

    private static PropertyUpdateNotifyProxy GetChainedNotify(List<(Func<object, object> ParentGetter, PropertyUpdateNotifyProxy? Notify)> list)
    {
        var chainLength = list.Count;
        ConditionalWeakTable<object, ChainedNotifyInfoUnit> chainedInfo = [];

        return new((obj, handler) =>
        {
            if (!chainedInfo.TryGetValue(obj, out var unit))
            {
                unit = new(chainLength);
                chainedInfo.Add(obj, unit);
            }
            var (sourceHandlers, parentObjs, targetHandlers) = unit;

            if (targetHandlers.Count == 0)
            {
                // subscribe source handlers
                list.ForEach((i, t) =>
                {
                    if (t is not (var parentGetter, { } notify)) return;

                    var parentObj = parentGetter.Invoke(obj);

                    // get cache or create source handler
                    var sourceHandler = sourceHandlers[i] ??= () =>
                    {
                        foreach (var j in Enumerable.Range(0, chainLength).Skip(i + 1).Reverse())
                        {
                            if (list[j] is not (var g, { } n)) continue;
                            if (
                                (parentObjs[j], sourceHandlers[j]) is not
                                ({ } oldOp /*我超，op！*/, { } sh)
                            ) throw new Exception("this exception should not be thrown");

                            n.UnsubscribeUpdate(oldOp, sh);
                            var newOp = g.Invoke(obj);
                            n.SubscribeUpdate(newOp, sh);
                            parentObjs[j] = newOp;
                        }
                        targetHandlers.ForEach(a => a.Invoke());
                    };
                    notify.SubscribeUpdate(parentObj, sourceHandler);

                    // record subscribed object
                    parentObjs[i] = parentObj;
                });
            }

            targetHandlers.Add(handler);
        }, (obj, handler) =>
        {
            if (!chainedInfo.TryGetValue(obj, out var unit)) return;
            var (sourceHandlers, parentObjs, targetHandlers) = unit;

            targetHandlers.Remove(handler);
            if (targetHandlers.Count == 0)
            {
                list.ForEach((i, t) =>
                {
                    if (t is not (_, { } notify)) return;
                    if (
                        (parentObjs[i], sourceHandlers[i]) is not
                        ({ } oldOp /*我超，op！*/, { } sh)
                    ) throw new Exception("this exception should not be thrown");
                    notify.UnsubscribeUpdate(oldOp, sh);
                    parentObjs[i] = null;
                });
            }
        });
    }

    private class ChainedNotifyInfoUnit(int length)
    {
        public readonly Action?[] SourceHandlers = new Action?[length];
        public readonly object?[] ParentObjects = new object?[length];
        public readonly List<Action> TargetHandlers = [];

        public void Deconstruct(out Action?[] sourceHandlers, out object?[] parentObjects, out List<Action> targetHandlers)
        {
            sourceHandlers = SourceHandlers;
            parentObjects  = ParentObjects;
            targetHandlers = TargetHandlers;
        }
    }
}