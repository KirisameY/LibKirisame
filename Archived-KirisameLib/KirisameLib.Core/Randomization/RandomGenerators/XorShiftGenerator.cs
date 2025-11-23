namespace KirisameLib.Randomization.RandomGenerators;

public class XorShiftGenerator(uint state) : SimpleRandomGenerator
{
    //State
    public uint State { get; private set; } = state;


    //Random
    public override uint NextUint32()
    {
        State ^= State << 13;
        State ^= State >> 17;
        State ^= State << 5;
        return State;
    }
}