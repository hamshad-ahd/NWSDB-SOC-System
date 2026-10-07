using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Bank.Api.Data;
using Bank.Api.Services;

// Configure default culture with Sri Lankan Rupee currency symbol
var defaultCulture = new CultureInfo("en-US");
defaultCulture.NumberFormat.CurrencySymbol = "Rs. ";
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bank Core Banking REST API (API 02)",
        Version = "v1",
        Description = "Core Banking REST API for card validation, balance checking, and payment authorization."
    });
});

// Configure EF Core SQLite
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BankDatabase")));

// Register Service Layer
builder.Services.AddScoped<IBankService, BankService>();

// Configure URLs
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://*:5001"); 
}

var app = builder.Build();

// Auto-migrate & Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    context.Database.Migrate();
    BankDbInitializer.Initialize(context);
    BankDbInitializer.ResetTestData(context);
}

// Enable Swagger in Development and Production for SOC assessment review
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

app.Run();
