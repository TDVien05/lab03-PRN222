using System.ComponentModel.DataAnnotations;
using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace lab03.Pages;

[Authorize]
public class CreateGroupModel : PageModel
{
    private readonly ChatDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateGroupModel> _logger;

    public CreateGroupModel(
        ChatDbContext dbContext,
        IConfiguration configuration,
        ILogger<CreateGroupModel> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Group name is required.")]
    [StringLength(100, ErrorMessage = "Group name must be 100 characters or fewer.")]
    public string? GroupName { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public string Visibility { get; set; } = string.Empty;

    public string? StatusMessage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string PublicVisibilityValue => GetRequiredConfig("CreateGroup:PublicVisibilityValue");

    public string PrivateVisibilityValue => GetRequiredConfig("CreateGroup:PrivateVisibilityValue");

    public string PublicVisibilityLabel => GetRequiredConfig("CreateGroup:PublicVisibilityLabel");

    public string PrivateVisibilityLabel => GetRequiredConfig("CreateGroup:PrivateVisibilityLabel");

    public string VisibilityLabel =>
        string.Equals(Visibility, PrivateVisibilityValue, StringComparison.OrdinalIgnoreCase)
            ? PrivateVisibilityLabel
            : PublicVisibilityLabel;

    public void OnGet()
    {
        Visibility = GetRequiredConfig("CreateGroup:DefaultVisibility");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var adminDisplayName = GetRequiredConfig("AdminCredentials:DisplayName");

            var adminParticipant = await _dbContext.Participants
                .FirstOrDefaultAsync(participant => participant.DisplayName == adminDisplayName);

            if (adminParticipant is null)
            {
                adminParticipant = new Participant
                {
                    DisplayName = adminDisplayName
                };

                _dbContext.Participants.Add(adminParticipant);
                await _dbContext.SaveChangesAsync();
            }

            var description = Description?.Trim();
            if (!string.IsNullOrWhiteSpace(description))
            {
                description = string.Format(
                    GetRequiredConfig("CreateGroup:DescriptionVisibilityFormat"),
                    VisibilityLabel,
                    description);
            }
            else
            {
                description = string.Format(
                    GetRequiredConfig("CreateGroup:EmptyDescriptionVisibilityFormat"),
                    VisibilityLabel);
            }

            var group = new ChatGroup
            {
                GroupName = GroupName!.Trim(),
                Description = description,
                CreatedBy = adminParticipant.ParticipantId
            };

            _dbContext.ChatGroups.Add(group);
            await _dbContext.SaveChangesAsync();

            _dbContext.GroupMembers.Add(new GroupMember
            {
                GroupId = group.GroupId,
                ParticipantId = adminParticipant.ParticipantId
            });

            await _dbContext.SaveChangesAsync();

            StatusMessage = string.Format(
                GetRequiredConfig("CreateGroup:SuccessMessageFormat"),
                group.GroupName);
            GroupName = string.Empty;
            Description = string.Empty;
            Visibility = GetRequiredConfig("CreateGroup:DefaultVisibility");

            return Page();
        }
        catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException)
        {
            _logger.LogError(exception, "Failed to create chat group.");
            ErrorMessage = GetRequiredConfig("CreateGroup:DatabaseErrorMessage");
            return Page();
        }
    }

    private string GetRequiredConfig(string key)
    {
        return _configuration[key]
            ?? throw new InvalidOperationException($"Missing configuration: {key}");
    }
}
