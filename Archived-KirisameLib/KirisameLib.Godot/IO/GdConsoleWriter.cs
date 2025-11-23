using System.Diagnostics.CodeAnalysis;
using System.Text;

using Godot;

namespace KirisameLib.Godot.IO;

public class GdConsoleWriter : TextWriter
{
    private MemoryStream Buffer { get; } = new();
    [field: AllowNull, MaybeNull]
    private StreamWriter BufferWriter => field ??= new(Buffer, Encoding.UTF8, -1, true);
    [field: AllowNull, MaybeNull]
    private StreamReader BufferReader => field ??= new(Buffer, Encoding.UTF8, false, -1, true);

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        BufferWriter.Write(value);
    }

    public override void Flush()
    {
        BufferWriter.Flush();
        Buffer.Position = 0;
        GD.Print(BufferReader.ReadToEnd());
        Buffer.SetLength(0);
        Buffer.Position = 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        BufferWriter.Dispose();
        BufferReader.Dispose();
        Buffer.Dispose();
    }
}