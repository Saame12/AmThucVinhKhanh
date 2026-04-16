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
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginInfo)
    {
        if (string.IsNullOrWhiteSpace(loginInfo.Username) || string.IsNullOrWhiteSpace(loginInfo.Password))
        {
            return BadRequest(new { message = "Thiếu tên đăng nhập hoặc mật khẩu." });
        }

        var normalizedUsername = loginInfo.Username.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower() == normalizedUsername &&
            u.Password == loginInfo.Password);

        if (user is null)
        {
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
        }

        if (string.Equals(user.Status?.Trim(), "Blocked", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Tài khoản đã bị khóa." });
        }

        _presenceService.MarkOnline(user.Id);
        return Ok(ToResponse(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin." });
        }

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername);
        if (usernameExists)
        {
            return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            Password = request.Password,
            FullName = request.FullName.Trim(),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim(),
            Status = "Active",
            IsVip = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _presenceService.MarkOnline(user.Id);
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

    [HttpPost("vip/purchase/{id:int}")]
    public async Task<ActionResult<LoginResponse>> PurchaseVip(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng nÃ y." });
        }

        if (string.Equals(user.Status?.Trim(), "Blocked", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "TÃ i khoáº£n Ä‘Ã£ bá»‹ khÃ³a." });
        }

        user.IsVip = true;
        await _context.SaveChangesAsync();

        return Ok(ToResponse(user));
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
            IsVip = user.IsVip,
            IsVirtual = user.IsVirtual
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
                IsVip = false,
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
}
