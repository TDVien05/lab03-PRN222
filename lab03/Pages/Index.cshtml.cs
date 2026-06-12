using DataAccessLayer;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace lab03.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ChatDbContext _dbContext;

        public IndexModel(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public string? DisplayName { get; set; }

        public string? ErrorMessage { get; private set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var normalizedDisplayName = DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedDisplayName))
            {
                ErrorMessage = "Please enter a display name first.";
                return Page();
            }

            if (normalizedDisplayName.Length > 50)
            {
                ErrorMessage = "Display name must be 50 characters or fewer.";
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

            HttpContext.Session.SetString("DisplayName", participant.DisplayName);
            HttpContext.Session.SetString("ParticipantId", participant.ParticipantId.ToString());

            return RedirectToPage("/DiscoverGroups");
        }
    }
}
