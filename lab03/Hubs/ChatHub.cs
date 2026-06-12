using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace lab03.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _dbContext;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ChatDbContext dbContext, ILogger<ChatHub> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task JoinGroup(long groupId, long participantId)
    {
        if (!await ParticipantExistsAsync(participantId) || !await GroupExistsAsync(groupId))
        {
            throw new HubException("Unable to join the selected chat group.");
        }

        await EnsureGroupMembershipAsync(groupId, participantId);
        await Groups.AddToGroupAsync(Context.ConnectionId, GetSignalRGroupName(groupId));
    }

    public async Task SendMessage(long groupId, long participantId, string messageText)
    {
        var normalizedMessage = messageText.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            throw new HubException("Message text is required.");
        }

        var participant = await _dbContext.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.ParticipantId == participantId);

        if (participant is null || !await GroupExistsAsync(groupId))
        {
            throw new HubException("Unable to send this message.");
        }

        await EnsureGroupMembershipAsync(groupId, participantId);

        var message = new Message
        {
            GroupId = groupId,
            SenderId = participantId,
            MessageText = normalizedMessage,
            MessageType = "text",
            SentAt = DateTimeOffset.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Participant {ParticipantId} sent message {MessageId} to group {GroupId}.",
            participantId,
            message.MessageId,
            groupId);

        await Clients.Group(GetSignalRGroupName(groupId)).SendAsync("ReceiveMessage", new
        {
            messageId = message.MessageId,
            groupId,
            senderId = participant.ParticipantId,
            senderName = participant.DisplayName,
            text = normalizedMessage,
            messageType = message.MessageType,
            sentAt = message.SentAt
        });
    }

    private async Task<bool> ParticipantExistsAsync(long participantId)
    {
        return await _dbContext.Participants
            .AsNoTracking()
            .AnyAsync(participant => participant.ParticipantId == participantId);
    }

    private async Task<bool> GroupExistsAsync(long groupId)
    {
        return await _dbContext.ChatGroups
            .AsNoTracking()
            .AnyAsync(group => group.GroupId == groupId);
    }

    private async Task EnsureGroupMembershipAsync(long groupId, long participantId)
    {
        var isMember = await _dbContext.GroupMembers
            .AnyAsync(member => member.GroupId == groupId && member.ParticipantId == participantId);

        if (isMember)
        {
            return;
        }

        _dbContext.GroupMembers.Add(new GroupMember
        {
            GroupId = groupId,
            ParticipantId = participantId,
            JoinedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public static string GetSignalRGroupName(long groupId)
    {
        return $"chat-group-{groupId}";
    }
}
