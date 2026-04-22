using System.Collections.Concurrent;
using VinhKhanhFood.API.Models;

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

    public string GetStatus(User user)
    {
        if (user.IsVirtual)
        {
            return GetVirtualStatus(user);
        }

        return GetRegisteredStatus(user.Id);
    }

    public int GetActiveTravelerCount(IEnumerable<User> users)
    {
        var onlineTravelerCount = users.Count(user =>
            !string.Equals(user.Role?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(user.Role?.Trim(), "Owner", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(GetStatus(user), "Online", StringComparison.OrdinalIgnoreCase));
        return onlineTravelerCount;

    }

    public string BuildGuestDisplayName(string? remoteIp, string guestId)
    {
        var normalizedIp = string.IsNullOrWhiteSpace(remoteIp)
            ? "unknown-ip"
            : remoteIp.Replace(":", "-").Replace("%", "-").Trim();
        var suffix = guestId.Length > 6 ? guestId[^6..] : guestId;
        return $"guid-{normalizedIp}-{suffix}";
    }

    private string GetRegisteredStatus(int userId)
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

    private string GetVirtualStatus(User user)
    {
        if (!user.LastSeenUtc.HasValue)
        {
            return "Offline";
        }

        return DateTime.UtcNow - user.LastSeenUtc.Value <= ActiveWindow
            ? "Online"
            : "Offline";
    }
}
