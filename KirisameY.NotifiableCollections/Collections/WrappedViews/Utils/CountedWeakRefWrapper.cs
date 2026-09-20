using System.Runtime.CompilerServices;

namespace KirisameY.NotifiableCollections.Collections.WrappedViews.Utils;

internal class CountedWeakRefWrapper<TSource, TWrapped>(Func<TSource, TWrapped> wrapper)
    where TSource : class
    where TWrapped : class
{
    internal class CountedRecord(TWrapped value)
    {
        public uint Count = 0;
        public readonly TWrapped Value = value;
    }

    private readonly ConditionalWeakTable<TSource, CountedRecord> _table = [];

    public TWrapped? WrapNew(TSource source)
    {
        if (!_table.TryGetValue(source, out var record))
        {
            record = new(wrapper.Invoke(source));
            _table.Add(source, record);
        }

        record.Count++;
        return record.Value;
    }

    public TWrapped? RemoveWrapped(TSource source)
    {
        if (!_table.TryGetValue(source, out var record)) return null;

        record.Count--;
        if (record.Count <= 0) _table.Remove(source);
        return record.Value;
    }
}