namespace KirisameLib.Randomization;

public interface IRandomGenerator
{
    uint NextUint32();
    int NextInt32();

    ulong NextUint64();
    long NextInt64();

    double NextDouble();
    float NextSingle();
}