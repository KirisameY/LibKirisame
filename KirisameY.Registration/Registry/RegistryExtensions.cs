namespace KirisameY.Registration.Registry;

public static class RegistryExtensions
{
    extension(IRegistry reg)
    {
        // todo: 等待 .Net 11, 最好 11 真的能拿下
        // public object? this[RegKey id] => null;
    }

    extension<T>(IRegistry<T> reg)
    {
        // todo: 等待 .Net 11
        // public T this[RegKey id] => null;
    }
}