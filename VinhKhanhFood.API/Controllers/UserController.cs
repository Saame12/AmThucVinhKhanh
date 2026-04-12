using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng này." });
        }

        return Ok(user);
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
            Status = "Active"
        };

        _context.Users.Add(user);
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

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Đã xóa thành công người dùng có ID: {id}" });
    }

    private static LoginResponse ToResponse(User user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Status = user.Status
        };
}
