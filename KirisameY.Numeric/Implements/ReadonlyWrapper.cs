namespace KirisameY.Numeric.Implements;

internal class ReadonlyWrapper<TValue, TOrder>(IModifierEditableNumeric<TValue, TOrder> innerNumeric) : IModifierEditableNumeric<TValue, TOrder>
    where TOrder : Enum
{
    public TValue BaseValue => innerNumeric.BaseValue;
    public TValue Value => innerNumeric.Value;

    public event EventHandler Updated
    {
        add => innerNumeric.Updated += value;
        remove => innerNumeric.Updated -= value;
    }

    public IReadOnlyCollection<INumericModifier<TOrder>> Modifiers => innerNumeric.Modifiers;
    public void AddModifier(INumericModifier<TOrder> modifier) => innerNumeric.AddModifier(modifier);
    public bool RemoveModifier(INumericModifier<TOrder> modifier) => innerNumeric.RemoveModifier(modifier);
}