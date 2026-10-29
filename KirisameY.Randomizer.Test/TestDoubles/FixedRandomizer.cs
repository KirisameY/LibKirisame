namespace KirisameY.Randomizer.Test.TestDoubles;

/// <summary>
///     A <see cref="SimpleRandomizer"/> whose <see cref="NextU64"/> result is fixed,
///     so the base-class conversions can be asserted exactly.
/// </summary>
internal sealed class FixedRandomizer(ulong value) : SimpleRandomizer
{
    public ulong Value { get; set; } = value;

    public int NextU64CallCount { get; private set; }

    public override ulong NextU64()
    {
        NextU64CallCount++;
        return Value;
    }
}
