using JetBrains.Annotations;

namespace KirisameY.Randomizer;

public interface IRandomizer
{
    [PublicAPI] uint NextU32();
    [PublicAPI] uint NextU32(uint max);
    [PublicAPI] int NextI32();

    [PublicAPI] ulong NextU64();
    [PublicAPI] ulong NextU64(ulong max);
    [PublicAPI] long NextI64();

    [PublicAPI] double NextDouble();
    [PublicAPI] float NextSingle();
}

public interface ISavableRandomizer : IRandomizer
{
    [PublicAPI] ulong State { get; }

    [PublicAPI] static abstract ISavableRandomizer Restore(ulong state);
}

public interface ISavableRandomizer<out TSelf> : ISavableRandomizer where TSelf : ISavableRandomizer<TSelf>
{
    [PublicAPI] new static abstract TSelf Restore(ulong state);

    [PublicAPI] static ISavableRandomizer ISavableRandomizer.Restore(ulong state) => TSelf.Restore(state);
}