namespace KirisameLib.FileSys;

public static class RecordedDirectoryExtensions
{
    public static RecordedDirectory GetOrCreateDirectory(this RecordedDirectory dir, string name) =>
        dir.TryGetDirectory(name, out var result) ? result : dir.CreateDirectory(name)!;

    public static RecordedFile GetOrCreateFile(this RecordedDirectory dir, string name, Func<Stream> streamGetter) =>
        dir.TryGetFile(name, out var result) ? result : dir.CreateFile(name, streamGetter)!;
}