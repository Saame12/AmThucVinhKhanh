namespace VinhKhanhFood.App.Models;

public sealed class UsageActorIdentity
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? GuestId { get; set; }
}
