namespace KirisameLib.FileSys;

public class NativeFile(FileInfo file) : IReadableFile
{
    public NativeFile(string path) : this(new FileInfo(path)) { }

    public bool Exists => file.Exists;
    public string Name => file.Name;
    public string FullName => file.FullName;
    public IReadableDirectory? Directory => file.Directory is null ? null : new NativeDirectory(file.Directory);

    public Stream OpenRead() => file.OpenRead();

    public static implicit operator NativeFile(FileInfo file) => new(file);
}