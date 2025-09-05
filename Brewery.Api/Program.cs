using Brewery.Api.Middleware;
using Brewery.Application.Interfaces;
using Brewery.Persistence.Data;
using Brewery.Persistence.Integrations;
using Brewery.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});


builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient for OpenBrewery
builder.Services.AddHttpClient<IOpenBreweryClient, OpenBreweryClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration.GetValue<string>("OpenBrewery:BaseUrl")
        ?? "https://api.openbrewerydb.org/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTransientHttpErrorPolicy(p =>
    p.WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry)))
);

// EF Core - SQLite
var useSqlite = builder.Configuration.GetValue<bool>("Persistence:UseSqlite", true);
if (useSqlite)
{
    var conn = builder.Configuration.GetValue<string>(
        "Persistence:SqliteConnection",
        "Data Source=breweries.db");

    builder.Services.AddDbContext<BreweryDbContext>(opts => opts.UseSqlite(conn));
}

// Register repositories & services
builder.Services.AddScoped<IBreweryRepository, EfBreweryRepository>();
builder.Services.AddScoped<IBreweryService, Brewery.Application.Services.BreweryService>();

// Register DataSeeder if using sqlite
if (useSqlite)
{
    builder.Services.AddScoped<DataSeeder>();
}

// API Key (optional)
var enableApiKey = builder.Configuration.GetValue<bool>("Security:EnableApiKey", false);

// Build app
var app = builder.Build();

// Run migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        if (useSqlite)
        {
            var db = services.GetRequiredService<BreweryDbContext>();
            db.Database.Migrate(); // apply migrations

            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Error during DB migration/seed");
        throw;
    }
}

// API Key middleware (if enabled)
if (enableApiKey)
{
    app.Use(async (ctx, next) =>
    {
        if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var providedKey))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { error = "Missing API Key" });
            return;
        }

        var expected = builder.Configuration.GetValue<string>("Security:ApiKey");
        if (!string.Equals(providedKey, expected, StringComparison.InvariantCulture))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
            return;
        }

        await next();
    });
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Brewery API v1");
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
