using System.Linq.Expressions;

using KirisameY.GenericUtils;

namespace KirisameY.BindingBridge.Binder;

public interface IPropertyDataBinder
{
    IBindHandle BindPropertyOneWay<TSource, TTarget, TValue>(
        TSource source, Expression<Func<TSource, TValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TValue>> targetProperty
    ) where TSource : notnull where TTarget : notnull;

    IBindHandle BindPropertyOneWay<TSource, TTarget, TSourceValue, TTargetValue>(
        TSource source, Expression<Func<TSource, TSourceValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TTargetValue>> targetProperty,
        Func<TSourceValue, TTargetValue> converter
    ) where TSource : notnull where TTarget : notnull;

    IBindHandle BindPropertyTwoWay<TSource, TTarget, TValue>(
        TSource source, Expression<Func<TSource, TValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TValue>> targetProperty
    ) where TSource : notnull where TTarget : notnull;

    IBindHandle BindPropertyTwoWay<TSource, TTarget, TSourceValue, TTargetValue>(
        TSource source, Expression<Func<TSource, TSourceValue>> sourceProperty,
        TTarget target, Expression<Func<TTarget, TTargetValue>> targetProperty,
        Func<TSourceValue, TTargetValue> converter, Func<TTargetValue, TSourceValue> reversedConverter
    ) where TSource : notnull where TTarget : notnull;

    // collections
    IBindHandle BindCollection<TSource, TTarget, TElement>(TSource source, TTarget target, TypeA<TElement> elementType = default)
        where TSource : class where TTarget : class;

    IBindHandle BindCollection<TSourceRoot, TTargetRoot, TSourceCollection, TTargetCollection, TElement>(
        TSourceRoot sourceObj, Expression<Func<TSourceRoot, TSourceCollection>> sourceCollection,
        TTargetRoot targetObj, Expression<Func<TTargetRoot, TTargetCollection>> targetCollection,
        TypeA<TElement> elementType = default)
        where TSourceRoot : class
        where TTargetRoot : class
        where TSourceCollection : class
        where TTargetCollection : class;

    // events
    IBindHandle BindEvent<TSource, TTarget, TDelegate>(
        TSource source, string eventName,
        TTarget target, Expression<Func<TTarget, TDelegate>> handler
    ) where TSource : class where TDelegate : Delegate;

    IBindHandle BindEvent<TSource, TTarget, TSourceDelegate, TTargetDelegate>(
        TSource source, string eventName,
        TTarget target, Expression<Func<TTarget, TTargetDelegate>> handler,
        Func<TTargetDelegate, TSourceDelegate> converter
    ) where TSource : class where TSourceDelegate : Delegate where TTargetDelegate : Delegate;

    IBindHandle BindEvent<TSourceRoot, TSourceNotifier, TTarget, TDelegate>(
        TSourceRoot source, Expression<Func<TSourceRoot, TSourceNotifier>> sourceNotifier, string eventName,
        TTarget target, Expression<Func<TTarget, TDelegate>> handler
    ) where TSourceRoot : class where TSourceNotifier : class where TDelegate : Delegate;

    IBindHandle BindEvent<TSourceRoot, TSourceNotifier, TTarget, TSourceDelegate, TTargetDelegate>(
        TSourceRoot source, Expression<Func<TSourceRoot, TSourceNotifier>> sourceNotifier, string eventName,
        TTarget target, Expression<Func<TTarget, TTargetDelegate>> handler,
        Func<TTargetDelegate, TSourceDelegate> converter
    ) where TSourceRoot : class where TSourceNotifier : class where TSourceDelegate : Delegate where TTargetDelegate : Delegate;
}

public interface IDataBinder : IPropertyDataBinder;