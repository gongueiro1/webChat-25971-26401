using Microsoft.AspNetCore.SignalR;

namespace webChat.Hubs;

public class ChatHub : Hub
{
    // Permite que o utilizador se junte a uma "sala" específica deste Post
    public async Task JoinPostGroup(int postId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Post_{postId}");
    }
}