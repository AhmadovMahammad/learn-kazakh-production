using Microsoft.AspNetCore.SignalR;

namespace LearnKazakh.API;

public sealed class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    private readonly PresenceTracker _presenceTracker = presenceTracker;

    public override async Task OnConnectedAsync()
    {
        HttpContext? http = Context.GetHttpContext();
        string connectionId = http == null
            ? Context.ConnectionId
            : http.Request.Query["u"].ToString();

        if (_presenceTracker.UserConnected(connectionId))
        {
            await Clients.All.SendAsync("ActiveUsersChanged", _presenceTracker.GetOnlineUsersCount());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        HttpContext? http = Context.GetHttpContext();
        string connectionId = http == null
            ? Context.ConnectionId
            : http.Request.Query["u"].ToString();

        if (_presenceTracker.UserDisconnected(connectionId))
        {
            await Clients.All.SendAsync("ActiveUsersChanged", _presenceTracker.GetOnlineUsersCount());
        }

        await base.OnDisconnectedAsync(exception);
    }
}
