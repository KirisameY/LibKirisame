namespace KirisameY.Numeric;

public interface INumeric
{
    event EventHandler Updated;
}

public interface INumeric<out T> : INumeric
{
    T BaseValue { get; }
    T Value { get; }
}

public interface IBaseEditableNumeric<T> : INumeric<T>
{
    new T BaseValue { get; set; }
}

public interface IModifierEditableNumeric<TOrder> : INumeric
    where TOrder : Enum
{
    IReadOnlyCollection<INumericModifier<TOrder>> Modifiers { get; }

    void AddModifier(INumericModifier<TOrder> modifier);
    bool RemoveModifier(INumericModifier<TOrder> modifier);
}

public interface IModifierEditableNumeric<out T, TOrder> : INumeric<T>, IModifierEditableNumeric<TOrder>
    where TOrder : Enum;

public interface IEditableNumeric<T, TOrder> : IBaseEditableNumeric<T>, IModifierEditableNumeric<T, TOrder>
    where TOrder : Enum;