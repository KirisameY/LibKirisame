using JetBrains.Annotations;

namespace KirisameY.GenericUtils;

public static class TypeA
{
    [PublicAPI]
    public static TypeA<T> Of<T>() => new();
}

public struct TypeA<[UsedImplicitly] T>;