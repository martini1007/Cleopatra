using Microsoft.EntityFrameworkCore;
using Cleopatra.Data;
using Cleopatra.Services;
using Hangfire;



var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ReminderService>();

var app = builder.Build();

var builder = WebApplication.CreateBuilder(args);

// Dodaj Hangfire do kontenera DI
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

// Dodaj Dashboard Hangfire
app.UseHangfireDashboard();

// Endpoint Dashboard (opcjonalnie chroniony)
app.UseEndpoints(endpoints =>
{
    endpoints.MapHangfireDashboard("/hangfire");
});

app.Run();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure AppDbContext to use SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=cleopatra.db"));

var app = builder.Build();

var reminderService = app.Services.GetService<ReminderService>();

RecurringJob.AddOrUpdate(
    "SendReminders",
    () => reminderService.SendRemindersAsync(),
    Cron.Daily); // Wysy³a przypomnienia codziennie

// Call the SeedManually method at startup (REMOVE after adding migrations)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        context.SeedManually();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();