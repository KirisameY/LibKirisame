namespace KirisameY.Numeric;

public interface INumericModifier
{
    void ModifyValue(ref double value);

    event EventHandler Updated;
}

public interface INumericModifier<out TOrder> : INumericModifier where TOrder : Enum
{
    TOrder Order { get; }
}