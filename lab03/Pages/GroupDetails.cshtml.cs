using DataAccessLayer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace lab03.Pages;

public class GroupDetailsModel : PageModel
{
    private readonly ChatDbContext _dbContext;

    public GroupDetailsModel(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public GroupSummary? Group { get; private set; }

    public List<MemberSummary> Members { get; private set; } = [];

    public int MessageCount { get; private set; }

    public async Task OnGetAsync(long? groupId)
    {
        var selectedGroupId = groupId ?? await _dbContext.ChatGroups
            .AsNoTracking()
            .OrderByDescending(group => group.CreatedAt)
            .Select(group => (long?)group.GroupId)
            .FirstOrDefaultAsync();

        if (selectedGroupId is null)
        {
            return;
        }

        Group = await _dbContext.ChatGroups
            .AsNoTracking()
            .Where(group => group.GroupId == selectedGroupId)
            .Select(group => new GroupSummary(
                group.GroupId,
                group.GroupName,
                group.Description ?? string.Empty,
                group.CreatedAt))
            .FirstOrDefaultAsync();

        if (Group is null)
        {
            return;
        }

        Members = await _dbContext.GroupMembers
            .AsNoTracking()
            .Where(member => member.GroupId == Group.Id)
            .OrderBy(member => member.JoinedAt)
            .Select(member => new MemberSummary(
                member.Participant.DisplayName,
                member.JoinedAt))
            .ToListAsync();

        MessageCount = await _dbContext.Messages
            .AsNoTracking()
            .CountAsync(message => message.GroupId == Group.Id);
    }

    public sealed record GroupSummary(long Id, string Name, string Description, DateTimeOffset CreatedAt)
    {
        public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
    }

    public sealed record MemberSummary(string DisplayName, DateTimeOffset JoinedAt);
}
