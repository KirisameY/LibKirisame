using System.Linq.Expressions;

namespace KirisameY.BindingBridge;

public interface IPropertyDataBinder
{
    IBindHandle BindPropertyOneWay<TSource, TTarget, TValue>(
        Expression<Func<TSource, TValue>> sourceProperty, Expression<Func<TTarget, TValue>> targetProperty
    );

    IBindHandle BindPropertyOneWay<TSource, TTarget, TSourceValue, TTargetValue>(
        Expression<Func<TSource, TSourceValue>> sourceProperty, Expression<Func<TTarget, TTargetValue>> targetProperty,
        Func<TSourceValue, TTargetValue> converter
    );

    IBindHandle BindPropertyTwoWay<TSource, TTarget, TValue>(
        Expression<Func<TSource, TValue>> sourceProperty, Expression<Func<TTarget, TValue>> targetProperty
    );

    IBindHandle BindPropertyTwoWay<TSource, TTarget, TSourceValue, TTargetValue>(
        Expression<Func<TSource, TSourceValue>> sourceProperty, Expression<Func<TTarget, TTargetValue>> targetProperty,
        Func<TSourceValue, TTargetValue> converter, Func<TTargetValue, TSourceValue> reversedConverter
    );
}

public interface IDataBinder : IPropertyDataBinder;