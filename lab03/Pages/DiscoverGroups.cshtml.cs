using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace lab03.Pages;

public class DiscoverGroupsModel : PageModel
{
    private readonly ChatDbContext _dbContext;

    public DiscoverGroupsModel(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<GroupSummary> Groups { get; private set; } = [];

    [BindProperty]
    public long GroupId { get; set; }

    [BindProperty]
    public string? DisplayName { get; set; }

    public long? ErrorGroupId { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        DisplayName = HttpContext.Session.GetString("DisplayName");
        await LoadGroupsAsync();
    }

    public async Task<IActionResult> OnPostJoinAsync()
    {
        var normalizedDisplayName = DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedDisplayName))
        {
            ErrorGroupId = GroupId;
            ErrorMessage = "Enter your name before joining this group.";
            await LoadGroupsAsync();
            return Page();
        }

        if (normalizedDisplayName.Length > 50)
        {
            ErrorGroupId = GroupId;
            ErrorMessage = "Name must be 50 characters or fewer.";
            await LoadGroupsAsync();
            return Page();
        }

        var groupExists = await _dbContext.ChatGroups
            .AsNoTracking()
            .AnyAsync(group => group.GroupId == GroupId);

        if (!groupExists)
        {
            ErrorGroupId = GroupId;
            ErrorMessage = "This group is no longer available.";
            await LoadGroupsAsync();
            return Page();
        }

        var participant = await _dbContext.Participants
            .FirstOrDefaultAsync(current => current.DisplayName == normalizedDisplayName);

        if (participant is null)
        {
            participant = new Participant
            {
                DisplayName = normalizedDisplayName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync();
        }

        var isMember = await _dbContext.GroupMembers
            .AnyAsync(member => member.GroupId == GroupId && member.ParticipantId == participant.ParticipantId);

        if (!isMember)
        {
            _dbContext.GroupMembers.Add(new GroupMember
            {
                GroupId = GroupId,
                ParticipantId = participant.ParticipantId,
                JoinedAt = DateTimeOffset.UtcNow
            });

            await _dbContext.SaveChangesAsync();
        }

        HttpContext.Session.SetString("DisplayName", participant.DisplayName);
        HttpContext.Session.SetString("ParticipantId", participant.ParticipantId.ToString());

        return RedirectToPage("/ChatRoom", new { groupId = GroupId });
    }

    private async Task LoadGroupsAsync()
    {
        Groups = await _dbContext.ChatGroups
            .AsNoTracking()
            .OrderByDescending(group => group.CreatedAt)
            .Select(group => new GroupSummary(
                group.GroupId,
                group.GroupName,
                group.Description ?? string.Empty,
                group.Members.Count))
            .ToListAsync();
    }

    public sealed record GroupSummary(long Id, string Name, string Description, int MemberCount);
}
