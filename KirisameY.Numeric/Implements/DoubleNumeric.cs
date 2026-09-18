using KirisameY.CollectionViews.Combined;
using KirisameY.Relinq.Extensions;

namespace KirisameY.Numeric.Implements;

internal class DoubleNumeric<TOrder>(double baseValue = 0) : IEditableNumeric<double, TOrder> where TOrder : Enum
{
    private bool _dirty = true;

    private EventHandler? _updated;

    public event EventHandler Updated
    {
        add => _updated += value;
        remove => _updated -= value;
    }

    private void RaiseUpdated() => _updated?.Invoke(this, EventArgs.Empty);

    private void ModifierUpdatedHandler(object? sender, EventArgs e)
    {
        _dirty = true;
        RaiseUpdated();
    }

    public double BaseValue
    {
        get;
        set
        {
            field  = value;
            _dirty = true;
            RaiseUpdated();
        }
    } = baseValue;
    public double Value
    {
        get
        {
            if (!_dirty) return field;

            var v = BaseValue;
            Modifiers.ForEach(m => m.ModifyValue(ref v));
            field  = v;
            _dirty = false;
            return field;
        }
    }

    private readonly SortedList<TOrder, List<INumericModifier<TOrder>>> _modifiers = [];
    public IReadOnlyCollection<INumericModifier<TOrder>> Modifiers => field ??= CombinedCollectionView<INumericModifier<TOrder>>.Create(_modifiers.Values.AsReadOnly());

    public void AddModifier(INumericModifier<TOrder> modifier)
    {
        var order = modifier.Order;
        if (!_modifiers.TryGetValue(order, out var list))
        {
            list = _modifiers[order] = [];
        }
        list.Add(modifier);

        modifier.Updated += ModifierUpdatedHandler;
        _dirty           =  true;
        RaiseUpdated();
    }

    public bool RemoveModifier(INumericModifier<TOrder> modifier)
    {
        if (!_modifiers.TryGetValue(modifier.Order, out var list)) return false;
        if (!list.Remove(modifier)) return false;

        modifier.Updated -= ModifierUpdatedHandler;
        _dirty           =  true;
        RaiseUpdated();
        return true;
    }
}