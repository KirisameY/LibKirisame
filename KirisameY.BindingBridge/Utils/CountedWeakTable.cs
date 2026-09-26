using System.Runtime.CompilerServices;

namespace KirisameY.BindingBridge.Utils;

internal class CountedWeakTable<TKey, TValue>
    where TKey : class
    where TValue : class
{
    internal class CountedRecord(TValue value)
    {
        public uint Count = 0;
        public readonly TValue Value = value;
    }

    private readonly ConditionalWeakTable<TKey, CountedRecord> _table = [];

    public void Add(TKey key, TValue value)
    {
        if (!_table.TryGetValue(key, out var record))
        {
            record = new(value);
            _table.Add(key, record);
        }

        record.Count++;
    }

    public TValue? Remove(TKey key)
    {
        if (!_table.TryGetValue(key, out var record)) return null;

        record.Count--;
        if (record.Count <= 0) _table.Remove(key);
        return record.Value;
    }
}