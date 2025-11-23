namespace KirisameLib.Event;

public class Variable<T>(T value)
{
    public T Value { get; set; } = value;
    public static implicit operator T(Variable<T> v) => v.Value;
    
    public static implicit operator Variable<T>(T v) => new (v);
}