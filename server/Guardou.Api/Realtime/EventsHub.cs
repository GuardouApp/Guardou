using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Guardou.Api.Realtime;

[Authorize]
internal sealed class EventsHub : Hub<IEventsClient>
{
    public const string Path = "/api/events";

    public static string UserGroup(string userId)
    {
        return $"user:{userId}";
    }

    public override async Task OnConnectedAsync()
    {
        string userId = Context.UserIdentifier ?? throw new HubException("The signed in user has no id.");
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        await base.OnConnectedAsync();
    }
}
