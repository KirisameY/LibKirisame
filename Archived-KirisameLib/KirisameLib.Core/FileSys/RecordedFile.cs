namespace KirisameLib.FileSys;

public class RecordedFile(string name, IReadableDirectory? directory, Func<Stream> streamGetter) : IReadableFile
{
    public bool Exists => true;
    public string Name { get; } = name;
    public string FullName { get; } = directory is null ? name : $"{directory.FullName}/{name}";
    public IReadableDirectory? Directory { get; } = directory;

    public Stream OpenRead() => streamGetter.Invoke();
}