using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context) { _context = context; }

        // 1. Lấy TOÀN BỘ danh sách User (Để hiện lên bảng ở trang Admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // 2. Lấy thông tin CHI TIẾT của 1 User theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng này!");
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] User loginInfo)
        {
            // Sử dụng ToLower() để so sánh không phân biệt hoa thường cho Username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == loginInfo.Username.ToLower()
                                       && u.Password == loginInfo.Password);

            if (user == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu");

            // Ép kiểu về ToLower khi so sánh Status để chắc chắn "Active" hay "active" đều đúng
            if (user.Status?.Trim().ToLower() == "blocked")
                return BadRequest("Tài khoản đã bị khóa");

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            // 🔥 AUTO ACTIVE
            user.Status = "Active";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // 🔥 BLOCK
        [HttpPut("block/{id}")]
        public async Task<IActionResult> Block(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = "Blocked";
            await _context.SaveChangesAsync();

            return Ok();
        }

        // 🔥 UNBLOCK
        [HttpPut("unblock/{id}")]
        public async Task<IActionResult> Unblock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = "Active";
            await _context.SaveChangesAsync();

            return Ok();
        }

        // 3. Cập nhật thông tin User
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id) return BadRequest("ID không khớp!");

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return Ok("Cập nhật thông tin thành công!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng này để xóa!");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok($"Đã xóa thành công người dùng có ID: {id}");
        }
    }
}