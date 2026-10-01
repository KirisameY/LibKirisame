using System.Linq.Expressions;
using System.Reflection;

using KirisameY.BindingBridge.PropertyBinding.Implements;

namespace KirisameY.BindingBridge.Binder;

public abstract partial class DataBinderBase
{
    private readonly Dictionary<(Type Type, string Exp), object> _propertyCache = [];
    private readonly Dictionary<(Type Type, MemberInfo? Member), (object Subscriber, object Unsubscriber)> _propertyNotifyCache = [];
    private readonly Lock _cacheLock = new();

    private DelegatePropertyEndpoint<TObject, TProperty> ResolveProperty<TObject, TProperty>(Expression<Func<TObject, TProperty>> exp)
        where TObject : notnull
    {
        var objParam = exp.Parameters[0];
        var expBody = exp.Body;
        var (parent, member) = expBody switch
        {
            MemberExpression memberExp => (memberExp.Expression, memberExp.Member),
            IndexExpression indexExp   => (indexExp.Object, indexExp.Indexer),
            BinaryExpression { NodeType: ExpressionType.ArrayIndex } arrayIndexExp => (
                arrayIndexExp.Left, null
            ),
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
            _ => throw new ArgumentException("Expression is neither a property, field, nor indexer with constant index.")
        };

        // find cache
        using var _ = _cacheLock.EnterScope();

        var cacheKey = (typeof(TObject), exp.ToString());
        if (_propertyCache.TryGetValue(cacheKey, out var value))
        {
            return (DelegatePropertyEndpoint<TObject, TProperty>)value;
        }

        // todo: 链式适配
        // type check
        if (parent?.Type.IsAssignableTo(typeof(TObject)) is not true)
            throw new ArgumentException($"Expression is not from an instance of type <{typeof(TObject)}>.");

        // getter compile
        var getter = exp.Compile();

        // setter resolve
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
                methodName.Split('_') is ["get", var propName] && type.GetProperty(propName) is { } property
            ) => property.GetIndexParameters() is []
                ? Expression.Property(obj, property)
                : Expression.MakeIndex(obj, property, arguments),

            _ => throw new ArgumentException("Expression is neither a property, field, nor indexer.")
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

        // notify resolve
        // todo: 链式适配
        var notCacheKey = (typeof(TObject), member);
        var notifiable = true;
        if (!_propertyNotifyCache.TryGetValue(notCacheKey, out var n))
        {
            if (ResolveProperty(typeof(TObject), member) is { } updatedNotify)
            {
                _propertyNotifyCache[notCacheKey] = n = (
                    (TObject obj, Action update) => updatedNotify.SubscribeUpdate.Invoke(obj, update),
                    (TObject obj, Action update) => updatedNotify.UnsubscribeUpdate.Invoke(obj, update)
                );
            }
            else notifiable = false;
        }

        // fin
        var subscriber = n.Subscriber as Action<TObject, Action>;
        var unsubscriber = n.Unsubscriber as Action<TObject, Action>;
        var result = (notifiable, setter) switch
        {
            (false, null)     => new DelegatePropertyEndpoint<TObject, TProperty>(getter),
            (true, null)      => new DelegatePropertyObservableEndpoint<TObject, TProperty>(getter, subscriber!, unsubscriber!),
            (false, not null) => new DelegatePropertyWritableEndpoint<TObject, TProperty>(getter, setter),
            (true, not null)  => new DelegatePropertyUniversalEndpoint<TObject, TProperty>(getter, setter, subscriber!, unsubscriber!)
        };
        _propertyCache.Add(cacheKey, result);
        return result;
    }
}