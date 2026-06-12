namespace DataAccessLayer.Entities;

public class Message
{
    public long MessageId { get; set; }

    public long GroupId { get; set; }

    public long SenderId { get; set; }

    public string? MessageText { get; set; }

    public string MessageType { get; set; } = "text";

    public DateTimeOffset SentAt { get; set; }

    public ChatGroup Group { get; set; } = null!;

    public Participant Sender { get; set; } = null!;

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
