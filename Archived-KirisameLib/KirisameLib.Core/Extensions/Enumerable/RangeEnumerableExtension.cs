namespace KirisameLib.Extensions.Enumerable;

public static class RangeEnumerableExtension
{
    //foreach
    public static RangeEnumerator GetEnumerator(this Range range) => new(range);

    public struct RangeEnumerator
    {
        public RangeEnumerator(Range range)
        {
            var start = (range.Start.IsFromEnd ? -1 : 1) * range.Start.Value;
            _end = (range.End.IsFromEnd ? -1 : 1) * range.End.Value;
            _dir = start <= _end ? (sbyte)1 : (sbyte)-1;
            Current = start - _dir;
        }

        private readonly int _end;
        private readonly sbyte _dir;
        public int Current { get; private set; }

        public bool MoveNext()
        {
            Current += _dir;
            return Current != _end;
        }
    }
}