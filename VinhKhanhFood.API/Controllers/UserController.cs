using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;
using VinhKhanhFood.API.Services;

namespace VinhKhanhFood.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private const double PoiRadiusMeters = 30d;
    private const double BetweenPoiMaxMeters = 45d;
    private const double BetweenPoiDeltaMeters = 8d;
    private readonly AppDbContext _context;
    private readonly UserPresenceService _presenceService;

    public UserController(AppDbContext context, UserPresenceService presenceService)
    {
        _context = context;
        _presenceService = presenceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoginResponse>>> GetUsers()
    {
        var users = await _context.Users
            .OrderByDescending(user => user.Id)
            .ToListAsync();

        var combinedUsers = users.Select(ToResponse)
            .OrderByDescending(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(user => user.Id)
            .ToList();

        return Ok(combinedUsers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoginResponse>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng này." });
        }

        return Ok(ToResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
        {
            return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống." });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginRequest.Username && !u.IsVirtual);

        if (user is null)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
        }

        if (user.Password != loginRequest.Password)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
        }

        if (user.Status == "Blocked")
        {
            return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa." });
        }

        return Ok(ToResponse(user));
    }

    [HttpPut("presence/{id:int}")]
    public async Task<IActionResult> UpdatePresence(int id, [FromQuery] bool isOnline)
    {
        var userExists = await _context.Users.AnyAsync(user => user.Id == id);
        if (!userExists)
        {
            return NotFound(new { message = "Không tìm thấy người dùng này." });
        }

        if (isOnline)
        {
            _presenceService.MarkOnline(id);
        }
        else
        {
            _presenceService.MarkOffline(id);
        }

        return Ok(new { id, onlineStatus = isOnline ? "Online" : "Offline" });
    }

    [HttpPut("guest-presence")]
    public async Task<IActionResult> UpdateGuestPresence([FromQuery] string guestId, [FromQuery] bool isOnline)
    {
        if (string.IsNullOrWhiteSpace(guestId))
        {
            return BadRequest(new { message = "GuestId is required." });
        }

        return await UpdateGuestPresenceInternalAsync(guestId.Trim(), isOnline);
    }

    [HttpGet("traveler-presence")]
    public async Task<IActionResult> GetTravelerPresenceSummary()
    {
        var users = await _context.Users.ToListAsync();
        var activeTravelerCount = _presenceService.GetActiveTravelerCount(users);

        return Ok(new { activeTravelerCount });
    }

    [HttpPut("visitor-location")]
    public async Task<IActionResult> UpdateVisitorLocation([FromBody] VisitorLocationRequest request)
    {
        if (request.UserId <= 0 && string.IsNullOrWhiteSpace(request.GuestId))
        {
            return BadRequest(new { message = "UserId or GuestId is required." });
        }

        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
        {
            return BadRequest(new { message = "Latitude/Longitude is invalid." });
        }

        var user = await ResolveTrackingUserAsync(request.UserId, request.GuestId);
        if (user is null)
        {
            return NotFound(new { message = "Visitor not found." });
        }

        var position = ResolvePoiPosition(request.Latitude, request.Longitude, await _context.FoodLocations.ToListAsync());

        user.LastLatitude = request.Latitude;
        user.LastLongitude = request.Longitude;
        user.CurrentPoiId = position.PrimaryPoi?.Id;
        user.CurrentPoiName = position.PrimaryPoi?.Name;
        user.SecondaryPoiId = position.SecondaryPoi?.Id;
        user.SecondaryPoiName = position.SecondaryPoi?.Name;
        user.LocationZoneStatus = position.ZoneStatus;
        user.LastSeenUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            zoneStatus = position.ZoneStatus,
            currentPoiId = user.CurrentPoiId,
            currentPoiName = user.CurrentPoiName,
            secondaryPoiId = user.SecondaryPoiId,
            secondaryPoiName = user.SecondaryPoiName
        });
    }

    [HttpPut("audio-heartbeat")]
    public async Task<IActionResult> UpdateAudioHeartbeat([FromBody] AudioHeartbeatRequest request)
    {
        if (request.PoiId <= 0)
        {
            return BadRequest(new { message = "PoiId is required." });
        }

        if (request.UserId <= 0 && string.IsNullOrWhiteSpace(request.GuestId))
        {
            return BadRequest(new { message = "UserId or GuestId is required." });
        }

        var user = await ResolveTrackingUserAsync(request.UserId, request.GuestId);
        if (user is null)
        {
            return NotFound(new { message = "Visitor not found." });
        }

        if (!request.IsPlaying)
        {
            user.LastAudioHeartbeatUtc = null;
            user.CurrentAudioPoiId = null;
            user.CurrentAudioPoiName = null;
            await _context.SaveChangesAsync();

            return Ok(new { isPlaying = false });
        }

        var poi = await _context.FoodLocations.FindAsync(request.PoiId);
        if (poi is null)
        {
            return NotFound(new { message = "POI not found." });
        }

        user.LastAudioHeartbeatUtc = DateTime.UtcNow;
        user.CurrentAudioPoiId = poi.Id;
        user.CurrentAudioPoiName = poi.Name;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            isPlaying = true,
            poiId = poi.Id,
            poiName = poi.Name
        });
    }

    [HttpPut("block/{id:int}")]
    public async Task<IActionResult> Block(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng này." });
        }

        user.Status = "Blocked";
        _presenceService.MarkOffline(id);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("unblock/{id:int}")]
    public async Task<IActionResult> Unblock(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng này." });
        }

        user.Status = "Active";
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest(new { message = "ID không khớp." });
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Users.AnyAsync(entity => entity.Id == id))
            {
                return NotFound(new { message = "Không tìm thấy người dùng này." });
            }

            throw;
        }

        return Ok(new { message = "Cập nhật thông tin thành công." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng này để xóa." });
        }

        if (string.Equals(user.Role?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Không thể xóa tài khoản Admin." });
        }

        _context.Users.Remove(user);
        _presenceService.MarkOffline(id);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Đã xóa thành công người dùng có ID: {id}" });
    }

    private LoginResponse ToResponse(User user) =>
        new()
        {
            Id = user.Id,
            DisplayId = user.Id.ToString(),
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Status = user.Status,
            OnlineStatus = _presenceService.GetStatus(user),
            IsVirtual = user.IsVirtual,
            LastSeenUtc = user.IsVirtual ? user.LastSeenUtc : null,
            LastLatitude = user.LastLatitude,
            LastLongitude = user.LastLongitude,
            CurrentPoiId = user.CurrentPoiId,
            CurrentPoiName = user.CurrentPoiName,
            SecondaryPoiId = user.SecondaryPoiId,
            SecondaryPoiName = user.SecondaryPoiName,
            LocationZoneStatus = user.LocationZoneStatus,
            LastAudioHeartbeatUtc = user.LastAudioHeartbeatUtc,
            CurrentAudioPoiId = user.CurrentAudioPoiId,
            CurrentAudioPoiName = user.CurrentAudioPoiName
        };

    private async Task<IActionResult> UpdateGuestPresenceInternalAsync(string guestId, bool isOnline)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var guestUser = await _context.Users.FirstOrDefaultAsync(user =>
            user.IsVirtual &&
            user.GuestId == guestId);

        if (guestUser is null)
        {
            guestUser = new User
            {
                Username = _presenceService.BuildGuestDisplayName(remoteIp, guestId),
                Password = string.Empty,
                FullName = _presenceService.BuildGuestDisplayName(remoteIp, guestId),
                Role = "TravelerGuest",
                Status = "Active",
                IsVirtual = true,
                GuestId = guestId,
                RemoteIp = remoteIp,
                LastSeenUtc = isOnline ? DateTime.UtcNow : null
            };

            _context.Users.Add(guestUser);
        }
        else
        {
            var displayName = _presenceService.BuildGuestDisplayName(remoteIp, guestId);
            guestUser.Username = displayName;
            guestUser.FullName = displayName;
            guestUser.RemoteIp = remoteIp;
            guestUser.LastSeenUtc = isOnline ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            guestId,
            onlineStatus = isOnline ? "Online" : "Offline",
            userId = guestUser.Id
        });
    }

    private async Task<User?> ResolveTrackingUserAsync(int? userId, string? guestId)
    {
        if (userId.HasValue && userId.Value > 0)
        {
            return await _context.Users.FindAsync(userId.Value);
        }

        if (string.IsNullOrWhiteSpace(guestId))
        {
            return null;
        }

        var guestUser = await _context.Users.FirstOrDefaultAsync(user => user.IsVirtual && user.GuestId == guestId);
        if (guestUser is not null)
        {
            return guestUser;
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        guestUser = new User
        {
            Username = _presenceService.BuildGuestDisplayName(remoteIp, guestId),
            Password = string.Empty,
            FullName = _presenceService.BuildGuestDisplayName(remoteIp, guestId),
            Role = "TravelerGuest",
            Status = "Active",
            IsVirtual = true,
            GuestId = guestId.Trim(),
            RemoteIp = remoteIp,
            LastSeenUtc = DateTime.UtcNow
        };

        _context.Users.Add(guestUser);
        await _context.SaveChangesAsync();
        return guestUser;
    }

    private static VisitorPoiPosition ResolvePoiPosition(double latitude, double longitude, List<FoodLocation> pois)
    {
        if (pois.Count == 0)
        {
            return new VisitorPoiPosition { ZoneStatus = "OUTSIDE_POI_ZONE" };
        }

        var nearest = pois
            .Select(poi => new
            {
                Poi = poi,
                DistanceMeters = CalculateDistanceMeters(latitude, longitude, poi.Latitude, poi.Longitude)
            })
            .OrderBy(item => item.DistanceMeters)
            .Take(2)
            .ToList();

        var primary = nearest.FirstOrDefault();
        var secondary = nearest.Skip(1).FirstOrDefault();

        if (primary is null)
        {
            return new VisitorPoiPosition { ZoneStatus = "OUTSIDE_POI_ZONE" };
        }

        if (secondary is not null &&
            primary.DistanceMeters <= BetweenPoiMaxMeters &&
            secondary.DistanceMeters <= BetweenPoiMaxMeters &&
            Math.Abs(primary.DistanceMeters - secondary.DistanceMeters) <= BetweenPoiDeltaMeters)
        {
            return new VisitorPoiPosition
            {
                ZoneStatus = "BETWEEN_POIS",
                PrimaryPoi = primary.Poi,
                SecondaryPoi = secondary.Poi
            };
        }

        if (primary.DistanceMeters <= PoiRadiusMeters)
        {
            return new VisitorPoiPosition
            {
                ZoneStatus = "AT_POI",
                PrimaryPoi = primary.Poi
            };
        }

        return new VisitorPoiPosition { ZoneStatus = "OUTSIDE_POI_ZONE" };
    }

    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000d;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var originLat = DegreesToRadians(lat1);
        var targetLat = DegreesToRadians(lat2);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(originLat) * Math.Cos(targetLat) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private sealed class VisitorPoiPosition
    {
        public string ZoneStatus { get; init; } = "OUTSIDE_POI_ZONE";
        public FoodLocation? PrimaryPoi { get; init; }
        public FoodLocation? SecondaryPoi { get; init; }
    }
}

public sealed class VisitorLocationRequest
{
    public int? UserId { get; set; }
    public string? GuestId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public sealed class AudioHeartbeatRequest
{
    public int? UserId { get; set; }
    public string? GuestId { get; set; }
    public int PoiId { get; set; }
    public bool IsPlaying { get; set; } = true;
}

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
