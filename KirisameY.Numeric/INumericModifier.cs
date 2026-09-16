namespace KirisameY.Numeric;

public interface INumericModifier
{
    void ModifyValue(ref double value);
}

public interface INumericModifier<out TOrder> : INumericModifier where TOrder : Enum
{
    TOrder Order { get; }
}