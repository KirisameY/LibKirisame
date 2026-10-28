using JetBrains.Annotations;

namespace KirisameY.Relinq.Extensions;

public static class EnumExtensions
{
    extension<TEnum>(TEnum) where TEnum : struct, Enum
    {
        [PublicAPI] public static TEnum[] GetValues() => Enum.GetValues<TEnum>();
    }
}