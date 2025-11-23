using System.Diagnostics.CodeAnalysis;

namespace KirisameLib.FileSys;

public class RecordedDirectory(string name, IReadableDirectory? parent) : IReadableDirectory
{
    public bool Exists => true;
    public string Name { get; } = name;

    private readonly Dictionary<string, RecordedDirectory> _directories = [];
    private readonly Dictionary<string, RecordedFile> _files = [];

    public string FullName { get; } = parent is null ? name : $"{parent.FullName}/{name}";

    public IEnumerable<IReadableDirectory> Directories =>　_directories.Values;
    public IEnumerable<IReadableFile> Files => _files.Values;
    public IReadableDirectory? Parent { get; } = parent;

    public RecordedDirectory? CreateDirectory(string name)
    {
        var dir = new RecordedDirectory(name, this);
        return _directories.TryAdd(name, dir) ? dir : null;
    }

    public RecordedFile? CreateFile(string name, Func<Stream> streamGetter)
    {
        var file = new RecordedFile(name, this, streamGetter);
        return _files.TryAdd(name, file) ? null : file;
    }

    public bool TryGetDirectory(string name, [MaybeNullWhen(false)] out RecordedDirectory directory) =>
        _directories.TryGetValue(name, out directory);

    public bool TryGetFile(string name, [MaybeNullWhen(false)] out RecordedFile file) =>
        _files.TryGetValue(name, out file);
}