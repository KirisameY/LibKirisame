using System.Collections.Generic;

namespace KirisameLib.GeneratorTools.Extensions;

public static class KeyValuePairExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value) =>
        (key, value) = (pair.Key, pair.Value);
}