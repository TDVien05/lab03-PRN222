namespace DataAccessLayer.Entities;

public class Attachment
{
    public long AttachmentId { get; set; }

    public long MessageId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public Message Message { get; set; } = null!;
}
