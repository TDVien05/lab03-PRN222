using DataAccessLayer;
using lab03.Hubs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
const long maxUploadSize = 1536L * 1024 * 1024;
var adminAuthScheme = builder.Configuration["AdminAuthentication:Scheme"]
    ?? throw new InvalidOperationException("Missing configuration: AdminAuthentication:Scheme");
var adminLoginPath = builder.Configuration["AdminAuthentication:LoginPath"]
    ?? throw new InvalidOperationException("Missing configuration: AdminAuthentication:LoginPath");
var adminAccessDeniedPath = builder.Configuration["AdminAuthentication:AccessDeniedPath"]
    ?? throw new InvalidOperationException("Missing configuration: AdminAuthentication:AccessDeniedPath");
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing configuration: ConnectionStrings:DefaultConnection");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadSize;
});

// Add services to the container.
builder.Services
    .AddAuthentication(adminAuthScheme)
    .AddCookie(adminAuthScheme, options =>
    {
        options.LoginPath = adminLoginPath;
        options.AccessDeniedPath = adminAccessDeniedPath;
    });

builder.Services.AddAuthorization();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(defaultConnection));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadSize;
});
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<ChatHub>("/chatHub");

app.Run();
