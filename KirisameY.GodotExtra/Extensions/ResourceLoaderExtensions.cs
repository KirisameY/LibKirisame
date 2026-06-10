using System.Diagnostics.CodeAnalysis;

using Godot;

using JetBrains.Annotations;

namespace KirisameY.GodotExtra.Extensions;

using static ResourceLoader;

public static class ResourceLoaderExtensions
{
    extension(ResourceLoader)
    {
        [PublicAPI]
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public static T? LoadOrDefault<T>(
            string path, string? typeHint = null, CacheMode cacheMode = CacheMode.Reuse, T? defaultValue = null
        ) where T : Resource => Exists(path, typeHint ?? "") ? Load<T>(path, typeHint, cacheMode) : defaultValue;

        [PublicAPI]
        public static bool TryLoad<T>(
            string path, [NotNullWhen(true)] out T? result, string? typeHint = null, CacheMode cacheMode = CacheMode.Reuse
        ) where T : Resource
        {
            result = null;
            if (!Exists(path, typeHint ?? "")) return false;
            result = Load<T>(path, typeHint, cacheMode);
            return true;
        }
    }
}