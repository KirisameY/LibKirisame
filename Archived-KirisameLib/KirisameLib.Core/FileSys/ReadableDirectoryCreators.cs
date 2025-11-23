using System.IO.Compression;

namespace KirisameLib.FileSys;

public static class ReadableDirectoryCreators
{
    public static IReadableDirectory ToReadableDirectory(this DirectoryInfo directoryInfo) => new NativeDirectory(directoryInfo);


    public static IReadableDirectory ToReadableDirectory(this ZipArchive zip, string fullName) =>
        zip.ToReadableDirectory(Path.GetFileName(fullName), Path.GetDirectoryName(fullName));

    public static IReadableDirectory ToReadableDirectory(this ZipArchive zip, string rootName, string? rootPath)
    {
        var result = new RecordedDirectory(rootName, rootPath is null ? null : new NativeDirectory(rootPath));
        foreach (var entry in zip.Entries)
        {
            var dir = entry.FullName.Split("/").SkipLast(1).Aggregate(result, (d, path) => d.GetOrCreateDirectory(path));
            dir.CreateFile(entry.Name, () => entry.Open());
        }
        return result;
    }
}