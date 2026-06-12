namespace DataAccessLayer.Entities;

public class GroupMember
{
    public long GroupMemberId { get; set; }

    public long GroupId { get; set; }

    public long ParticipantId { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public ChatGroup Group { get; set; } = null!;

    public Participant Participant { get; set; } = null!;
}
