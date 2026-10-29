using JetBrains.Annotations;

namespace KirisameY.Randomizer.RandomGenerators;

[PublicAPI]
public class XorShiftStarGenerator(ulong state) : SimpleRandomizer, ISavableRandomizer<XorShiftStarGenerator>
{
    //State
    public ulong State { get; private set; } = state;

    public static XorShiftStarGenerator Restore(ulong state) => new(state);


    //Random
    public override ulong NextU64()
    {
        var state = State;
        state ^= state >> 12;
        state ^= state << 25;
        state ^= state >> 27;
        unchecked { state *= 0x2545F4914F6CDD1DUL; }
        return State = state;
    }
}