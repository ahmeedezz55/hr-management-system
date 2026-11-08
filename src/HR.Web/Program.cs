using System.Globalization;
using HR.Infrastructure;
using HR.Infrastructure.Data;
using HR.Infrastructure.Identity;
using HR.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------
// Database provider decision:
//   1) Try the configured SQL Server (with light retries for a cold DB).
//   2) Fall back to a local SQLite file when SQL Server is unavailable
//      (e.g. GitHub Codespaces started without the database sidecar).
// ------------------------------------------------------------------
using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddConsole());
var sqlServerLogger = bootstrapLoggerFactory.CreateLogger("DatabaseStartup");
var useSqlite = !await DatabaseStartup.SqlServerReachableAsync(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerLogger);

if (useSqlite)
{
    sqlServerLogger.LogWarning(
        "سيتم استخدام قاعدة بيانات SQLite محلية (HRSystem.db) بدلاً من SQL Server.");
}

builder.Services.AddInfrastructure(builder.Configuration, useSqlite);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddHttpClient("Groq", client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supported = new[] { new CultureInfo("ar-SA"), new CultureInfo("en-US") };
    options.DefaultRequestCulture = new RequestCulture("ar-SA");
    options.SupportedCultures = supported;
    options.SupportedUICultures = supported;
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(db, users, roles);
}

app.Run();
