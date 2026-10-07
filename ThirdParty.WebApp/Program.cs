using System.Globalization;
using ThirdParty.WebApp.Services;

// Configure default culture with Sri Lankan Rupee currency symbol
var defaultCulture = new CultureInfo("en-US");
defaultCulture.NumberFormat.CurrencySymbol = "Rs. ";
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register NwsdbApiClient for REST API communication
builder.Services.AddHttpClient<INwsdbApiClient, NwsdbApiClient>(client =>
{
    var nwsdbApiUrl = builder.Configuration["NwsdbApi:BaseUrl"]
        ?? builder.Configuration["NwsdbApiUrl"]
        ?? "http://localhost:5002";

    if (!nwsdbApiUrl.EndsWith("/"))
    {
        nwsdbApiUrl += "/";
    }

    client.BaseAddress = new Uri(nwsdbApiUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Configure URL
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://*:5004");
}

var app = builder.Build();

// Configure HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Collection}/{action=Lookup}/{id?}");

app.Run();
