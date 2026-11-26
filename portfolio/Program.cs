using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;
using portfolio.Data;
using portfolio.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database: read the connection string from User Secrets.
var connectionString = builder.Configuration.GetConnectionString("Portfolio");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Portfolio' is missing. Add it in Manage User Secrets.");
}

var dataSource = NpgsqlDataSource.Create(connectionString);

builder.Services.AddSingleton(dataSource);
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddHttpClient<SupabaseStorageService>();

// Authentication: use cookies for the editor login.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";

        options.Cookie.Name = "Portfolio.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Limit repeated login attempts.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ipAddress,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Redirect(
            "/Account/Login?tooManyAttempts=true");

        await Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
