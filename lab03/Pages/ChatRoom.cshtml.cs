using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using lab03.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace lab03.Pages;

public class ChatRoomModel : PageModel
{
    private const long MaxUploadSize = 1536L * 1024 * 1024;

    private readonly ChatDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatRoomModel(
        ChatDbContext dbContext,
        IWebHostEnvironment environment,
        IHubContext<ChatHub> hubContext)
    {
        _dbContext = dbContext;
        _environment = environment;
        _hubContext = hubContext;
    }

    public List<GroupSummary> Groups { get; private set; } = [];

    public GroupSummary? SelectedGroup { get; private set; }

    public List<MessageSummary> Messages { get; private set; } = [];

    public long? ParticipantId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(long? groupId)
    {
        if (!long.TryParse(HttpContext.Session.GetString("ParticipantId"), out var participantId))
        {
            return RedirectToPage("/DiscoverGroups");
        }

        var displayName = HttpContext.Session.GetString("DisplayName");
        var participantExists = await _dbContext.Participants
            .AsNoTracking()
            .AnyAsync(participant => participant.ParticipantId == participantId);

        if (string.IsNullOrWhiteSpace(displayName) || !participantExists)
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/DiscoverGroups");
        }

        ParticipantId = participantId;
        DisplayName = displayName;

        Groups = await _dbContext.ChatGroups
            .AsNoTracking()
            .OrderByDescending(group => group.CreatedAt)
            .Select(group => new GroupSummary(
                group.GroupId,
                group.GroupName,
                group.Members.Count))
            .ToListAsync();

        var selectedGroupId = groupId ?? Groups.FirstOrDefault()?.Id;
        SelectedGroup = Groups.FirstOrDefault(group => group.Id == selectedGroupId);

        if (SelectedGroup is null)
        {
            return Page();
        }

        await EnsureGroupMembershipAsync(SelectedGroup.Id, participantId);

        Groups = await _dbContext.ChatGroups
            .AsNoTracking()
            .OrderByDescending(group => group.CreatedAt)
            .Select(group => new GroupSummary(
                group.GroupId,
                group.GroupName,
                group.Members.Count))
            .ToListAsync();

        SelectedGroup = Groups.First(group => group.Id == selectedGroupId);

        Messages = await _dbContext.Messages
            .AsNoTracking()
            .Where(message => message.GroupId == SelectedGroup.Id)
            .OrderBy(message => message.SentAt)
            .Select(message => new MessageSummary(
                message.MessageId,
                message.SenderId,
                message.Sender.DisplayName,
                message.MessageText ?? string.Empty,
                message.MessageType,
                message.Attachments
                    .Select(attachment => new AttachmentSummary(
                        attachment.OriginalFileName,
                        attachment.FilePath,
                        attachment.FileType,
                        attachment.FileSize))
                    .FirstOrDefault(),
                message.SentAt))
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(long groupId, long participantId, IFormFile? file)
    {
        if (!long.TryParse(HttpContext.Session.GetString("ParticipantId"), out var sessionParticipantId) ||
            sessionParticipantId != participantId)
        {
            return BadRequest(new { error = "Your chat session is no longer valid." });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Choose a file before uploading." });
        }

        if (file.Length > MaxUploadSize)
        {
            return BadRequest(new { error = "File must be 1.5 GB or smaller." });
        }

        var participant = await _dbContext.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.ParticipantId == participantId);

        var groupExists = await _dbContext.ChatGroups
            .AsNoTracking()
            .AnyAsync(group => group.GroupId == groupId);

        if (participant is null || !groupExists)
        {
            return BadRequest(new { error = "Unable to upload to this group." });
        }

        await EnsureGroupMembershipAsync(groupId, participantId);

        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "chat-files", groupId.ToString());
        Directory.CreateDirectory(uploadRoot);

        var physicalPath = Path.Combine(uploadRoot, storedFileName);
        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
        var messageType = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            ? "image"
            : "file";
        var webPath = $"/uploads/chat-files/{groupId}/{storedFileName}";

        var message = new Message
        {
            GroupId = groupId,
            SenderId = participantId,
            MessageText = originalFileName,
            MessageType = messageType,
            SentAt = DateTimeOffset.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        var attachment = new Attachment
        {
            MessageId = message.MessageId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            FilePath = webPath,
            FileType = contentType,
            FileSize = file.Length,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Attachments.Add(attachment);
        await _dbContext.SaveChangesAsync();

        var payload = new
        {
            messageId = message.MessageId,
            groupId,
            senderId = participant.ParticipantId,
            senderName = participant.DisplayName,
            text = message.MessageText ?? string.Empty,
            messageType = message.MessageType,
            sentAt = message.SentAt,
            attachment = new
            {
                fileName = attachment.OriginalFileName,
                url = attachment.FilePath,
                contentType = attachment.FileType,
                fileSize = attachment.FileSize
            }
        };

        await _hubContext.Clients
            .Group(ChatHub.GetSignalRGroupName(groupId))
            .SendAsync("ReceiveMessage", payload);

        return new JsonResult(payload);
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

    public sealed record GroupSummary(long Id, string Name, int MemberCount)
    {
        public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
    }

    public sealed record MessageSummary(
        long Id,
        long SenderId,
        string SenderName,
        string Text,
        string MessageType,
        AttachmentSummary? Attachment,
        DateTimeOffset SentAt)
    {
        public string SenderInitials => string.Concat(SenderName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
    }

    public sealed record AttachmentSummary(string FileName, string Url, string ContentType, long FileSize);
}
