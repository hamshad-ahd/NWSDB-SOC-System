using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.Services;

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
        Title = "NWSDB Water Services Core REST API (API 01)",
        Version = "v1",
        Description = "Core NWSDB REST API for Customer Verification, Bill Statement Inquiry, Water Usage, Payment Authorization, and Third-Party Collection Transfers."
    });
});

// EF Core SQLite
builder.Services.AddDbContext<NwsdbDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("NwsdbDatabase")));

// Register Service Layer (Dependency Injection)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IUsageService, UsageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IThirdPartyPaymentService, ThirdPartyPaymentService>();

// HttpClient for Bank.Api integration
builder.Services.AddHttpClient<IBankApiService, BankApiService>(client =>
{
    var bankApiUrl = builder.Configuration["BankApi:BaseUrl"]
        ?? builder.Configuration["BankApiUrl"]
        ?? "http://localhost:5001";

    if (!bankApiUrl.EndsWith("/"))
    {
        bankApiUrl += "/";
    }

    client.BaseAddress = new Uri(bankApiUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Configure URL
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://*:5002");
}

var app = builder.Build();

// Auto-migrate & Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NwsdbDbContext>();
    context.Database.Migrate();
    DbInitializer.Initialize(context);
    DbInitializer.ResetTestData(context);
}

// Enable Swagger in Development and Production for SOC assessment review
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NWSDB API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

app.Run();
