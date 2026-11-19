using System.Buffers.Binary;
using System.IO.Compression;

namespace Backend.Services;

public sealed class DocumentFormatValidator
{
    public const int MaxDocumentBytes = 10 * 1024 * 1024;
    public const int MaxAudioBytes = 25 * 1024 * 1024;

    private static readonly List<string> AllowedExtensions =
        [".pdf", ".docx", ".jpg", ".jpeg", ".png", ".mp3", ".wav", ".m4a"];
    private static readonly List<string> ArchiveExtensions = [".pdf", ".docx"];

    public static string? FileName(string name) => name.Replace('\\', '/').Split('/').LastOrDefault();
    public bool IsAllowed(string extension) => AllowedExtensions.Contains(extension);
    public bool IsAllowedInArchive(string extension) => ArchiveExtensions.Contains(extension);
    public int MaxBytes(string extension) =>
        extension is ".pdf" or ".docx" or ".jpg" or ".jpeg" or ".png" ? MaxDocumentBytes : MaxAudioBytes;

    public string ContentType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".mp3" => "audio/mpeg",
        ".wav" => "audio/wav",
        _ => "audio/mp4"
    };

    public bool HasExpectedFormat(MemoryStream buffer, string extension)
    {
        var bytes = buffer.GetBuffer().AsSpan(0, (int)buffer.Length);
        if (extension == ".pdf")
            return bytes.StartsWith("%PDF-"u8);

        if (extension is ".jpg" or ".jpeg")
            return bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8 &&
                   bytes[^2] == 0xFF && bytes[^1] == 0xD9;

        if (extension == ".png")
            return bytes.Length >= 45 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) &&
                   BinaryPrimitives.ReadUInt32BigEndian(bytes[8..12]) == 13 &&
                   bytes[12..].StartsWith("IHDR"u8) &&
                   BinaryPrimitives.ReadUInt32BigEndian(bytes[16..20]) > 0 &&
                   BinaryPrimitives.ReadUInt32BigEndian(bytes[20..24]) > 0 &&
                   bytes[^8..].StartsWith("IEND"u8);

        if (extension == ".mp3")
            return bytes.Length >= 10 && bytes.StartsWith("ID3"u8) ||
                   bytes.Length >= 4 && bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0;

        if (extension == ".wav")
            return bytes.Length >= 44 && bytes.StartsWith("RIFF"u8) &&
                   bytes[8..].StartsWith("WAVE"u8);

        if (extension == ".m4a")
            return bytes.Length >= 12 && bytes[4..].StartsWith("ftyp"u8) &&
                   (bytes[8..].StartsWith("M4A "u8) || bytes[8..].StartsWith("isom"u8) ||
                    bytes[8..].StartsWith("mp41"u8) || bytes[8..].StartsWith("mp42"u8));

        if (!bytes.StartsWith("PK\x03\x04"u8)) return false;
        try
        {
            buffer.Position = 0;
            using var archive = new ZipArchive(buffer, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count > 1000) return false;
            return archive.GetEntry("[Content_Types].xml") is not null &&
                   archive.GetEntry("word/document.xml") is not null;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}
