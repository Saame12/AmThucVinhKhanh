using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OwnerController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/owner/register
        [HttpPost("register")]
        public async Task<ActionResult<Owner>> Register([FromBody] RegisterOwnerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required");

            if (await _context.Owners.AnyAsync(o => o.Username == dto.Username))
                return BadRequest("Username already exists");

            if (await _context.Owners.AnyAsync(o => o.Email == dto.Email))
                return BadRequest("Email already exists");

            var owner = new Owner
            {
                Username = dto.Username,
                Password = dto.Password, // TODO: Hash password in production
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                BusinessName = dto.BusinessName,
                BusinessDescription = dto.BusinessDescription,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IdentificationNumber = dto.IdentificationNumber,
                TaxNumber = dto.TaxNumber,
                Status = "Pending",
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOwner), new { id = owner.Id }, owner);
        }

        // POST: api/owner/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginDto dto)
        {
            var owner = await _context.Owners
                .FirstOrDefaultAsync(o => o.Username == dto.Username && o.Password == dto.Password);

            if (owner == null)
                return Unauthorized("Invalid username or password");

            if (owner.Status != "Approved")
                return Unauthorized("Your account is not approved yet. Please wait for admin approval.");

            return Ok(new LoginResponse
            {
                Id = owner.Id,
                Username = owner.Username,
                FullName = owner.FullName,
                Email = owner.Email,
                Status = owner.Status
            });
        }

        // GET: api/owner/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Owner>> GetOwner(int id)
        {
            var owner = await _context.Owners
                .Include(o => o.FoodLocations)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (owner == null)
                return NotFound();

            return Ok(owner);
        }

        // GET: api/owner/{id}/foodlocations
        [HttpGet("{id}/foodlocations")]
        public async Task<ActionResult<List<FoodLocation>>> GetOwnerFoodLocations(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound("Owner not found");

            var foodLocations = await _context.FoodLocations
                .Where(f => f.OwnerId == id)
                .ToListAsync();

            return Ok(foodLocations);
        }

        // POST: api/owner/{id}/foodlocation
        [HttpPost("{id}/foodlocation")]
        public async Task<ActionResult<FoodLocation>> AddFoodLocation(int id, [FromBody] CreateFoodLocationDto dto)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound("Owner not found");

            var foodLocation = new FoodLocation
            {
                Name = dto.Name,
                Description = dto.Description,
                Name_EN = dto.Name_EN,
                Description_EN = dto.Description_EN,
                Name_ZH = dto.Name_ZH,
                Description_ZH = dto.Description_ZH,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImageUrl = dto.ImageUrl,
                AudioUrl = dto.AudioUrl,
                AudioUrl_EN = dto.AudioUrl_EN,
                AudioUrl_ZH = dto.AudioUrl_ZH,
                OwnerId = id,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.FoodLocations.Add(foodLocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFoodLocation", new { id = foodLocation.Id }, foodLocation);
        }

        // PUT: api/owner/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOwner(int id, [FromBody] UpdateOwnerDto dto)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound();

            owner.Email = dto.Email ?? owner.Email;
            owner.PhoneNumber = dto.PhoneNumber ?? owner.PhoneNumber;
            owner.FullName = dto.FullName ?? owner.FullName;
            owner.BusinessName = dto.BusinessName ?? owner.BusinessName;
            owner.BusinessDescription = dto.BusinessDescription ?? owner.BusinessDescription;
            owner.Address = dto.Address ?? owner.Address;
            owner.Latitude = dto.Latitude ?? owner.Latitude;
            owner.Longitude = dto.Longitude ?? owner.Longitude;

            await _context.SaveChangesAsync();
            return Ok(owner);
        }
    }

    // DTOs
    public class RegisterOwnerDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string BusinessName { get; set; }
        public string? BusinessDescription { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? TaxNumber { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }

    public class CreateFoodLocationDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }
        public string? Name_EN { get; set; }
        public string? Description_EN { get; set; }
        public string? AudioUrl_EN { get; set; }
        public string? Name_ZH { get; set; }
        public string? Description_ZH { get; set; }
        public string? AudioUrl_ZH { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateOwnerDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessDescription { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
