namespace KirisameLib.FileSys;

public interface IReadableDirectory
{
    bool Exists { get; }
    string Name { get; }
    string FullName { get; }
    IEnumerable<IReadableDirectory> Directories { get; }
    IEnumerable<IReadableFile> Files { get; }
    IReadableDirectory? Parent { get; }
}