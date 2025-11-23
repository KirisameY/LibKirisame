namespace KirisameLib.FileSys;

public interface IReadableFile
{
    bool Exists { get; }
    string Name { get; }
    string FullName { get; }
    IReadableDirectory? Directory { get; }
    Stream OpenRead();
}