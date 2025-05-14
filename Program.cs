using Hangfire;
using Hangfire.SqlServer;
using WhatsAppNotifier.Services;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

// Register services
builder.Services.AddSingleton<WhatsAppService>();
builder.Services.AddSingleton<BackgroundMessagingService>();

var app = builder.Build();

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

// Configure Hangfire dashboard
app.UseHangfireDashboard();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Set up fixed recurring job - every 1 minute
using (var scope = app.Services.CreateScope())
{
    var backgroundService = scope.ServiceProvider.GetRequiredService<BackgroundMessagingService>();

    // Remove any existing jobs first
    RecurringJob.RemoveIfExists("send-sales-report");

    // Schedule the job to run every 1 minute
    RecurringJob.AddOrUpdate(
        "send-sales-report",
        () => backgroundService.SendSalesReportJob(),
        "*/1 * * * *", // Cron expression for every 1 minute
        TimeZoneInfo.Local);
}

app.Run();