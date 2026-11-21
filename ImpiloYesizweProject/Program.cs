using ImpiloYesizweProject.Data;
using ImpiloYesizweProject.Models;
using Microsoft.EntityFrameworkCore;
using ImpiloYesizweProject.Services;
using Microsoft.AspNetCore.DataProtection;
using ImpiloYesizweProject.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Validate database configuration
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The DefaultConnection connection string is missing.");
}

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// MVC
builder.Services.AddControllersWithViews();

// Administrator cookie authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ImpiloYesizwe.AdminAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = false;

        options.LoginPath = "/Admin/Login";
    });

builder.Services.AddAuthorization();

//Configure email settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Email verification security settings
builder.Services
    .AddOptions<EmailVerificationSettings>()
    .Bind(builder.Configuration.GetSection("EmailVerificationSettings"))
    .Validate(settings =>
    {
        if (string.IsNullOrWhiteSpace(settings.HashKey))
            return false;

        try
        {
            return Convert.FromBase64String(settings.HashKey).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }, "The verification HashKey must be a Base64-encoded 32-byte key.")
    .Validate(settings =>
        settings.CodeLifetimeMinutes is >= 1 and <= 15,
        "Verification code lifetime must be between 1 and 15 minutes.")
    .Validate(settings =>
        settings.MaxFailedAttempts is >= 1 and <= 10,
        "Failed attempts must be between 1 and 10.")
    .Validate(settings =>
        settings.ResendCooldownSeconds is >= 30 and <= 600,
        "Resend cooldown must be between 30 and 600 seconds.")
    .Validate(settings =>
        settings.MaxResends is >= 0 and <= 10,
        "Maximum resends must be between 0 and 10.")
    .ValidateOnStart();

// Application Data Protection for ASP.NET Core security features
builder.Services.AddDataProtection()
    .SetApplicationName("ImpiloYesizweProject");

// Audit service
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuditService>();

// Email service
builder.Services.AddScoped<EmailService>();

// Email verification service
builder.Services.AddScoped<AdminEmailVerificationService>();

// Session support
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);

    options.Cookie.Name = ".ImpiloYesizwe.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});

var app = builder.Build();



// HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

// Authenticate incoming administrator cookies.
app.UseAuthentication();

// Enforce authorization rules.
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
