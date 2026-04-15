using System.Collections.Concurrent;

namespace VinhKhanhFood.API.Services;

public sealed class UserPresenceService
{
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);
    private readonly ConcurrentDictionary<int, DateTimeOffset> _onlineUsers = new();

    public void MarkOnline(int userId)
    {
        _onlineUsers[userId] = DateTimeOffset.UtcNow;
    }

    public void MarkOffline(int userId)
    {
        _onlineUsers.TryRemove(userId, out _);
    }

    public string GetStatus(int userId)
    {
        if (!_onlineUsers.TryGetValue(userId, out var lastSeen))
        {
            return "Offline";
        }

        if (DateTimeOffset.UtcNow - lastSeen <= ActiveWindow)
        {
            return "Online";
        }

        _onlineUsers.TryRemove(userId, out _);
        return "Offline";
    }
}
