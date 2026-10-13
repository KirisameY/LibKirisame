using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace KirisameY.GenericUtils;

public static class Optional
{
    [PublicAPI] public static Optional<T> From<T>([DisallowNull] T value) => new Optional<T>(value);
    [PublicAPI] public static Optional<T> Empty<T>() => new Optional<T>();

    [PublicAPI]
    public static Optional<T> FromNullable<T>(T? value) => value switch
    {
        null     => new Optional<T>(),
        not null => new Optional<T>(value)
    };
}

public readonly struct Optional<T> : IEquatable<Optional<T>>
{
    [PublicAPI, MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }

    [PublicAPI]
    public T? Value { get; }

    public Optional()
    {
        HasValue = false;
        Value    = default;
    }

    internal Optional(T value)
    {
        HasValue = true;
        Value    = value;
    }


    public bool Equals(Optional<T> other) => (this, other) switch
    {
        ({ HasValue: false }, { HasValue: false }) => true,
        ({ HasValue: true }, { HasValue: true }) =>
            EqualityComparer<T>.Default.Equals(Value, other.Value),
        _ => false
    };

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(HasValue, Value);

    public static bool operator ==(Optional<T> a, Optional<T> b) => a.Equals(b);

    public static bool operator !=(Optional<T> a, Optional<T> b) => !a.Equals(b);
};