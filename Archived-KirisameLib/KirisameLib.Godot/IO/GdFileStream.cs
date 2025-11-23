using File = Godot.FileAccess;

namespace KirisameLib.Godot.IO;

public class GdFileStream : Stream
{
    #region Constructors

    private GdFileStream(File file, File.ModeFlags mode)
    {
        File = file;
        Mode = mode;
    }

    public static GdFileStream Open(string path, File.ModeFlags mode) =>
        new(File.Open(path, mode), mode);

    public static GdFileStream OpenCompressed(string path, File.ModeFlags mode, File.CompressionMode compressionMode) =>
        new(File.OpenCompressed(path, mode, compressionMode), mode);

    public static GdFileStream OpenEncrypted(string path, File.ModeFlags mode, byte[] key) =>
        new(File.OpenEncrypted(path, mode, key), mode);

    public static GdFileStream OpenEncryptedPw(string path, File.ModeFlags mode, string password) =>
        new(File.OpenEncryptedWithPass(path, mode, password), mode);

    #endregion


    #region Fields

    private File File { get; }
    private bool Closed { get; set; } = false;

    private File.ModeFlags Mode { get; }

    #endregion


    #region Stream Methods

    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(Closed, this);
        File.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(Closed, this);
        if (offset + count > buffer.Length)
            throw new ArgumentException($"The sum of {nameof(offset)} and {nameof(count)} is larger than the buffer length.");
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Param offset must greater than 0.");
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count),   count,  "Param count must greater than 0.");
        if (!CanRead) throw new NotSupportedException("The stream does not support reading.");

        var length = Math.Min(count, Length - Position);
        File.GetBuffer(length).CopyTo(buffer, offset);
        return (int)length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        ObjectDisposedException.ThrowIf(Closed, this);
        var target = offset + origin switch
        {
            SeekOrigin.Begin => 0,
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Param origin must be Begin, Current or End.")
        };
        File.Seek((ulong)target);
        return Position;
    }

    public override void SetLength(long value)
    {
        ObjectDisposedException.ThrowIf(Closed, this);
        if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");

        File.Resize(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(Closed, this);
        if (offset + count > buffer.Length)
            throw new ArgumentException($"The sum of {nameof(offset)} and {nameof(count)} is larger than the buffer length.");
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Param offset must greater than 0.");
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count),   count,  "Param count must greater than 0.");
        if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");

        var writeBuffer = (offset, count) == (0, buffer.Length)
            ? buffer : new ArraySegment<byte>(buffer, offset, count).ToArray();
        File.StoreBuffer(writeBuffer);
    }

    public override void Close()
    {
        if (Closed) return;
        Closed = true;
        File.Close();
        base.Close();
    }

    #endregion

    #region Stream Properties

    public override bool CanRead => Mode.HasFlag(File.ModeFlags.Read);
    public override bool CanSeek => true;
    public override bool CanWrite => Mode.HasFlag(File.ModeFlags.Write);
    public override long Length
    {
        get
        {
            ObjectDisposedException.ThrowIf(Closed, this);
            return (long)File.GetLength();
        }
    }
    public override long Position
    {
        get
        {
            ObjectDisposedException.ThrowIf(Closed, this);
            return (long)File.GetPosition();
        }
        set
        {
            ObjectDisposedException.ThrowIf(Closed, this);
            File.Resize(value);
        }
    }

    #endregion
}