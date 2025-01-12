using Microsoft.EntityFrameworkCore;
using Cleopatra.Data;
using Cleopatra.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure Email Settings
// builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// builder.Services.AddTransient<IEmailService, EmailService>();
// builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<ReminderService>();

// Configure Hangfire
// builder.Services.AddHangfire(config => config.UseMemoryStorage());
// builder.Services.AddHangfireServer();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure Authentication Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/api/account/login";
    options.AccessDeniedPath = "/api/account/accessdenied";
    options.SlidingExpiration = true;
});

// Add Controllers with Views
builder.Services.AddControllersWithViews();

// Configure AppDbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=cleopatra.db"));

var app = builder.Build();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Ensure the database is created
        context.SeedManually(); // Seed data into the database
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

// Middleware Pipeline Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Authentication middleware
app.UseAuthorization();  // Authorization middleware

// Hangfire Dashboard
//app.UseHangfireDashboard();

// Hangfire Job Configuration
// var reminderService = app.Services.GetService<ReminderService>();
// RecurringJob.AddOrUpdate(
//     "SendReminders",
//     () => reminderService.SendRemindersAsync(),
//     Cron.Daily); // Schedule daily reminders

// Endpoint Mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
