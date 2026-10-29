using JetBrains.Annotations;

namespace KirisameY.Randomizer.RandomGenerators;

[PublicAPI]
public class XorShiftGenerator(ulong state) : SimpleRandomizer, ISavableRandomizer<XorShiftGenerator>
{
    //State
    public ulong State { get; private set; } = state;

    public static XorShiftGenerator Restore(ulong state) => new(state);


    //Random
    public override ulong NextU64()
    {
        var state = State;
        state ^= state << 13;
        state ^= state >> 7;
        state ^= state << 17;
        return State = state;
    }
}