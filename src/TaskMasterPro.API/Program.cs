using DotNetEnv;
using Serilog;
using TaskMasterPro.API.Extensions;
using TaskMasterPro.API.Middlewares;
using TaskMasterPro.Application;
using TaskMasterPro.Infrastructure;
using TaskMasterPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// Load environment variables from .env if present
Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

// Ensure JWT configuration gets populated from environment variables
builder.Configuration.AddEnvironmentVariables();
// We also can explicitly map them for clarity in case of nested objects in appsettings
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtExpiry = Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS") ?? "1";
if (!string.IsNullOrEmpty(jwtSecret))
{
    builder.Configuration["Jwt:Secret"] = jwtSecret;
}

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

// Zero-Installation Automation: Automatic Migrations with Retry Mechanism
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterProDbContext>();
    var retries = 5;
    
    while (retries > 0)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning(ex, "Failed to apply migrations. Retrying... {Retries} attempts left.", retries);
            if (retries == 0) throw;
            Thread.Sleep(3000); // Wait before retrying
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// Configure CORS using environment variable
var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
if (!string.IsNullOrEmpty(corsOrigins))
{
    app.UseCors(policy => 
        policy.WithOrigins(corsOrigins.Split(';'))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
}
else 
{
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
}

app.UseAuthentication();
app.UseAuthorization();

// Serve frontend from wwwroot (SPA approach)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback for SPA
app.MapFallbackToFile("index.html");

app.Run();
