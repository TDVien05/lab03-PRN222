using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace lab03.Pages;

public class AdminLoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public AdminLoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public string? Identifier { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        ReturnUrl ??= _configuration["AdminAuthentication:DefaultReturnPage"];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var configuredUsername = GetRequiredConfig("AdminCredentials:Username");
        var configuredEmail = GetRequiredConfig("AdminCredentials:Email");
        var configuredPassword = GetRequiredConfig("AdminCredentials:Password");
        var adminRole = GetRequiredConfig("AdminAuthentication:Role");
        var authScheme = GetRequiredConfig("AdminAuthentication:Scheme");
        var defaultReturnPage = GetRequiredConfig("AdminAuthentication:DefaultReturnPage");

        var normalizedIdentifier = Identifier?.Trim();
        var identifierMatches =
            string.Equals(normalizedIdentifier, configuredUsername, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalizedIdentifier, configuredEmail, StringComparison.OrdinalIgnoreCase);

        var passwordMatches = string.Equals(Password, configuredPassword, StringComparison.Ordinal);

        if (!identifierMatches || !passwordMatches)
        {
            ErrorMessage = "Invalid admin credentials.";
            Password = null;
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, configuredUsername),
            new(ClaimTypes.Role, adminRole)
        };

        var identity = new ClaimsIdentity(claims, authScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(authScheme, principal);

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return LocalRedirect(defaultReturnPage);
    }

    private string GetRequiredConfig(string key)
    {
        return _configuration[key]
            ?? throw new InvalidOperationException($"Missing configuration: {key}");
    }
}
