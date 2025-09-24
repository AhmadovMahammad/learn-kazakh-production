using Blazored.LocalStorage;
using ClientSolution;
using ClientSolution.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Default client: for static files of client application
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Named clients for API calls
string? apiAddress = builder.Configuration.GetValue<string>("BaseUrl:ApiAddress") ?? "https://localhost:5001";
builder.Services.AddHttpClient("LearnKazakhApi", client =>
{
    client.BaseAddress = new Uri(apiAddress);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<PresenceService>();
builder.Services.AddScoped<ScrollService>();
builder.Services.AddBlazoredLocalStorage();

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddMudMarkdownServices();

builder.Logging.SetMinimumLevel(LogLevel.None);
await builder.Build().RunAsync();
