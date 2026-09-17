using KirisameY.CollectionViews.Combined;
using KirisameY.Relinq.Extensions;

namespace KirisameY.Numeric.Implements;

internal class DoubleNumeric<TOrder>(double baseValue = 0) : IEditableNumeric<double, TOrder> where TOrder : Enum
{
    private bool _dirty = true;

    private void SetDirtyHandler(object? sender, EventArgs e) => _dirty = true;

    public double BaseValue { get; set; } = baseValue;
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

        modifier.Updated += SetDirtyHandler;
        _dirty           =  true;
    }

    public bool RemoveModifier(INumericModifier<TOrder> modifier)
    {
        if (!_modifiers.TryGetValue(modifier.Order, out var list)) return false;
        if (!list.Remove(modifier)) return false;

        modifier.Updated -= SetDirtyHandler;
        _dirty           =  true;
        return true;
    }
}