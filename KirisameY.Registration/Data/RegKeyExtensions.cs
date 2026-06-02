using JetBrains.Annotations;

namespace KirisameY.Registration.Data;

public static class RegKeyExtensions
{
    extension(string self)
    {
        [PublicAPI]
        public RegKey? WithDefaultNameSpace(string defaultNameSpace, string separator = ":") => self.Split(separator) switch
        {
            [var nameSpace, var key] => RegKey.From(nameSpace, key),
            [var key]                => RegKey.From(defaultNameSpace, key),
            _                        => null
        };
    }
}