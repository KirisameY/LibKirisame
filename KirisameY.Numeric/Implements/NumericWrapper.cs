using System.Numerics;

namespace KirisameY.Numeric.Implements;

internal class NumericWrapper<TValue, TOrder>(DoubleNumeric<TOrder> innerNumeric, ValueConvertMode convertMode = ValueConvertMode.Checked) : IEditableNumeric<TValue, TOrder>
    where TValue : INumberBase<TValue>
    where TOrder : Enum
{
    private double ToDouble(TValue value) => convertMode switch
    {
        ValueConvertMode.Checked    => double.CreateChecked(value),
        ValueConvertMode.Saturating => double.CreateSaturating(value),
        ValueConvertMode.Truncating => double.CreateTruncating(value),
        _                           => throw new InvalidOperationException($"Unknown Convert Mode {convertMode}") // todo: union发布后重构
    };

    private TValue ToValue(double value) => convertMode switch
    {
        ValueConvertMode.Checked    => TValue.CreateChecked(value),
        ValueConvertMode.Saturating => TValue.CreateSaturating(value),
        ValueConvertMode.Truncating => TValue.CreateTruncating(value),
        _                           => throw new InvalidOperationException($"Unknown Convert Mode {convertMode}") // todo: union发布后重构
    };

    public TValue BaseValue
    {
        get => ToValue(innerNumeric.BaseValue);
        set => innerNumeric.BaseValue = ToDouble(value);
    }
    public TValue Value => ToValue(innerNumeric.Value);


    public IReadOnlyCollection<INumericModifier<TOrder>> Modifiers => innerNumeric.Modifiers;

    public void AddModifier(INumericModifier<TOrder> modifier)
    {
        innerNumeric.AddModifier(modifier);
    }

    public bool RemoveModifier(INumericModifier<TOrder> modifier)
    {
        return innerNumeric.RemoveModifier(modifier);
    }
}