namespace KirisameLib.FileSys;

public class NativeDirectory(DirectoryInfo dir) : IReadableDirectory
{
    public NativeDirectory(string path) : this(new DirectoryInfo(path)) { }

    public bool Exists => dir.Exists;
    public string Name => dir.Name;
    public string FullName => dir.FullName;

    public IEnumerable<IReadableDirectory> Directories => dir.EnumerateDirectories().Select(x => (NativeDirectory)x);
    public IEnumerable<IReadableFile> Files => dir.EnumerateFiles().Select(x => (NativeFile)x);
    public IReadableDirectory? Parent => dir.Parent is null ? null : new NativeDirectory(dir.Parent);

    public static implicit operator NativeDirectory(DirectoryInfo dirInfo) => new NativeDirectory(dirInfo);
}