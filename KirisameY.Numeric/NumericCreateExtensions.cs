using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.Numeric.Implements;

namespace KirisameY.Numeric;

public static class NumericCreateExtensions
{
    extension(INumeric)
    {
        [PublicAPI]
        public static IEditableNumeric<TValue, TOrder> Create<TValue, TOrder>(
            TValue value,
            [SuppressMessage("ReSharper", "UnusedParameter.Global")]
            TypeA<TOrder> orderType = default,
            ValueConvertMode convertMode = ValueConvertMode.Checked
        )
            where TValue : INumberBase<TValue>
            where TOrder : Enum
        {
            var valueInDouble = double.CreateChecked(value);

            var doubleNumeric = new DoubleNumeric<TOrder>(valueInDouble);
            if (typeof(TValue) == typeof(double)) return (IEditableNumeric<TValue, TOrder>)doubleNumeric;

            return new NumericWrapper<TValue, TOrder>(doubleNumeric, convertMode);
        }

        [PublicAPI]
        public static IModifierEditableNumeric<TValue, TOrder> CreateReadonly<TValue, TOrder>(
            TValue value,
            [SuppressMessage("ReSharper", "UnusedParameter.Global")]
            TypeA<TOrder> orderType = default,
            ValueConvertMode convertMode = ValueConvertMode.Checked
        )
            where TValue : INumberBase<TValue>
            where TOrder : Enum =>
            new ReadonlyWrapper<TValue, TOrder>(Create(value, orderType, convertMode));
    }
}