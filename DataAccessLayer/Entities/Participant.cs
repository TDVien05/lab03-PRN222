namespace DataAccessLayer.Entities;

public class Participant
{
    public long ParticipantId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ChatGroup> CreatedGroups { get; set; } = new List<ChatGroup>();

    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
