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

    IBindHandle BindCollection<TSourceObj, TTargetObj, TSourceCollection, TTargetCollection, TElement>(
        TSourceObj sourceObj, Expression<Func<TSourceObj, TSourceCollection>> sourceCollection,
        TTargetObj targetObj, Expression<Func<TTargetObj, TTargetCollection>> targetCollection,
        TypeA<TElement> elementType = default)
        where TSourceObj : notnull
        where TTargetObj : notnull
        where TSourceCollection : class
        where TTargetCollection : class;
}

public interface IDataBinder : IPropertyDataBinder;