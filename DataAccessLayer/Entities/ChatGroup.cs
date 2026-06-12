namespace DataAccessLayer.Entities;

public class ChatGroup
{
    public long GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Participant Creator { get; set; } = null!;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
