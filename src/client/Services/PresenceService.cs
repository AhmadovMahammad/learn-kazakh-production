using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace ClientSolution.Services;
public class PresenceService(IJSRuntime jSRuntime, IHttpClientFactory httpClientFactory) : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime = jSRuntime;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private HubConnection? _hubConnection;

    public event Action<int>? ActiveCountChanged;
    public int ActiveCount { get; private set; }

    public async Task StartAsync()
    {
        string userKey = await GetOrCreateUserKeyAsync();
        HttpClient httpClient = _httpClientFactory.CreateClient("LearnKazakhApi");

        Uri hubUri = new Uri(httpClient.BaseAddress ?? new Uri("https://localhost:7248/api/"), $"/hubs/presence?u={Uri.EscapeDataString(userKey)}");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUri)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<int>("ActiveUsersChanged", count =>
        {
            ActiveCount = count;
            ActiveCountChanged?.Invoke(count);
        });

        await _hubConnection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
        }
    }

    private async Task<string> GetOrCreateUserKeyAsync()
    {
        string userKey = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userKey");
        if (string.IsNullOrEmpty(userKey))
        {
            userKey = Guid.NewGuid().ToString();
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userKey", userKey);
        }

        return userKey;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
