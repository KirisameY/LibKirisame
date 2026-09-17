using System.Numerics;

using KirisameY.GenericUtils;

namespace KirisameY.Numeric.Test;

public enum TestOrder
{
    Early,
    Middle,
    Late
}

public class TestModifier(TestOrder order, Func<double, double> transform) : INumericModifier<TestOrder>
{
    private EventHandler? _updated;

    public TestOrder Order { get; } = order;

    public Func<double, double> Transform { get; set; } = transform;

    public int ModifyCallCount { get; private set; }

    public event EventHandler Updated
    {
        add => _updated += value;
        remove => _updated -= value;
    }

    public void ModifyValue(ref double value)
    {
        ModifyCallCount++;
        value = Transform(value);
    }

    public void RaiseUpdated() => _updated?.Invoke(this, EventArgs.Empty);
}

public static class NumericTestUtils
{
    public static IEditableNumeric<TValue, TestOrder> Create<TValue>(
        TValue value,
        ValueConvertMode convertMode = ValueConvertMode.Checked
    )
        where TValue : INumberBase<TValue> =>
        // todo: 泛型推断失败，暂时显式指定类型参数，待修复后移除
        INumeric.Create<TValue, TestOrder>(value, TypeA.Of<TestOrder>(), convertMode);

    public static IModifierEditableNumeric<TValue, TestOrder> CreateReadonly<TValue>(
        TValue value,
        ValueConvertMode convertMode = ValueConvertMode.Checked
    )
        where TValue : INumberBase<TValue> =>
        // todo: 泛型推断失败，暂时显式指定类型参数，待修复后移除
        INumeric.CreateReadonly<TValue, TestOrder>(value, TypeA.Of<TestOrder>(), convertMode);
}
