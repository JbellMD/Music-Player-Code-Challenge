using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Server.Hubs;
using music_manager_starter.Shared;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configure port
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("MusicManagerDb"));

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (!context.Playlists.Any())
        {
            // Add sample playlist
            var playlist = new Playlist
            {
                Name = "My First Playlist",
                Description = "Welcome to Music Manager!"
            };
            context.Playlists.Add(playlist);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Enable CORS
app.UseCors();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

// Map endpoints
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

try
{
    Log.Information("Starting web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
