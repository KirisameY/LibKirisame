using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace KirisameY.Registration.Data;

public sealed class RegKey
{
    #region Factory

    private RegKey(string nameSpace, string key)
    {
        NameSpace = nameSpace;
        Key       = key;
    }

    // ReSharper disable once InconsistentNaming
    private static readonly ConcurrentDictionary<(string nameSpace, string key), RegKey> _cache = new();

    [PublicAPI]
    public static RegKey From(string nameSpace, string key)
    {
        return _cache.GetOrAdd((nameSpace, key), static t => new(t.nameSpace, t.key));
    }

    [PublicAPI]
    public static RegKey? FromFull(string fullKey, string separator = ":")
    {
        if (fullKey.Split(separator) is not [var nameSpace, var key]) return null;

        return From(nameSpace, key);
    }

    #endregion


    #region Fields & Properties

    public string NameSpace { get; }
    public string Key { get; }

    [PublicAPI]
    [field: AllowNull, MaybeNull]
    public string FullName => field ??= $"{NameSpace}:{Key}";

    #endregion


    #region Equality

    public static bool operator ==(RegKey a, RegKey b) => Equals(a, b);

    public static bool operator !=(RegKey a, RegKey b) => !Equals(a, b);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    #endregion


    #region Convert

    public override string ToString() => FullName;

    [PublicAPI]
    public void Deconstruct(out string nameSpace, out string key) => (nameSpace, key) = (NameSpace, Key);

    public static implicit operator string(RegKey regKey) => regKey.FullName;

    public static implicit operator RegKey((string nameSpace, string key) regKey) => From(regKey.nameSpace, regKey.key);

    #endregion


    // Default
    public static readonly RegKey Default = From("", "");
}