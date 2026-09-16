namespace KirisameY.Numeric;

public interface INumeric<out T>
{
    T BaseValue { get; }
    T Value { get; }
}

public interface IBaseEditableNumeric<T> : INumeric<T>
{
    new T BaseValue { get; set; }
}

public interface IModifierEditableNumeric<out T, TOrder> : INumeric<T>
    where TOrder : Enum
{
    IReadOnlyCollection<INumericModifier<TOrder>> Modifiers { get; }

    void AddModifier(INumericModifier<TOrder> modifier);
    bool RemoveModifier(INumericModifier<TOrder> modifier);
}

public interface IEditableNumeric<T, TOrder> : IBaseEditableNumeric<T>, IModifierEditableNumeric<T, TOrder>
    where TOrder : Enum;