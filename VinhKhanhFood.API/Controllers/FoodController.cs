using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoodController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodLocation>>> GetLocations()
        {
            var locations = await _context.FoodLocations.ToListAsync();
            var baseUrl = "http://10.0.2.2:5020";

            foreach (var loc in locations)
            {
                if (!string.IsNullOrEmpty(loc.ImageUrl) && !loc.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    loc.ImageUrl = $"{baseUrl}/images/{loc.ImageUrl}";
                }
            }

            return Ok(locations);
        }

        [HttpPost]
        public async Task<ActionResult<FoodLocation>> PostFoodLocation(FoodLocation foodLocation)
        {
            if (string.IsNullOrEmpty(foodLocation.Status))
            {
                foodLocation.Status = "Pending";
            }

            _context.FoodLocations.Add(foodLocation);
            await _context.SaveChangesAsync();

            await AddUsageHistoryAsync(
                userId: foodLocation.OwnerId ?? 0,
                userName: "Owner",
                role: "Owner",
                action: "CREATE",
                poiId: foodLocation.Id,
                poiName: foodLocation.Name);

            return CreatedAtAction(nameof(GetLocations), new { id = foodLocation.Id }, foodLocation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodLocation(int id, FoodLocation foodLocation)
        {
            if (id != foodLocation.Id)
            {
                return BadRequest();
            }

            _context.Entry(foodLocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FoodLocations.Any(entity => entity.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            await AddUsageHistoryAsync(
                userId: foodLocation.OwnerId ?? 0,
                userName: "Owner",
                role: "Owner",
                action: "EDIT",
                poiId: foodLocation.Id,
                poiName: foodLocation.Name);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodLocation(int id)
        {
            var foodLocation = await _context.FoodLocations.FindAsync(id);
            if (foodLocation is null)
            {
                return NotFound();
            }

            await AddUsageHistoryAsync(
                userId: foodLocation.OwnerId ?? 0,
                userName: "Owner",
                role: "Owner",
                action: "DELETE",
                poiId: foodLocation.Id,
                poiName: foodLocation.Name);

            _context.FoodLocations.Remove(foodLocation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FoodLocation>> GetById(int id)
        {
            var food = await _context.FoodLocations.FindAsync(id);
            if (food is null)
            {
                return NotFound();
            }

            return Ok(food);
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var food = await _context.FoodLocations.FindAsync(id);
            if (food is null)
            {
                return NotFound();
            }

            food.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(food);
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var food = await _context.FoodLocations.FindAsync(id);
            if (food is null)
            {
                return NotFound();
            }

            food.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(food);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var data = await _context.UsageHistories
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("history/view")]
        public async Task<IActionResult> TrackView([FromBody] PoiViewHistoryRequest request)
        {
            var poi = await _context.FoodLocations.FindAsync(request.PoiId);
            if (poi is null)
            {
                return NotFound(new { message = "Không tìm thấy POI." });
            }

            var isGuestTraveler = string.Equals(request.Role?.Trim(), "TravelerGuest", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(request.Role?.Trim(), "Traveler", StringComparison.OrdinalIgnoreCase);
            var effectiveUserName = isGuestTraveler
                ? BuildGuestDisplayName(HttpContext.Connection.RemoteIpAddress?.ToString(), request.GuestId)
                : string.IsNullOrWhiteSpace(request.UserName) ? "Guest" : request.UserName.Trim();
            var effectiveRole = isGuestTraveler ? "TravelerGuest" : string.IsNullOrWhiteSpace(request.Role) ? "Traveler" : request.Role.Trim();

            await AddUsageHistoryAsync(
                userId: request.UserId,
                userName: effectiveUserName,
                role: effectiveRole,
                action: string.IsNullOrWhiteSpace(request.Action) ? "VIEW_DETAIL" : request.Action.Trim().ToUpperInvariant(),
                poiId: poi.Id,
                poiName: poi.Name);

            return Ok();
        }

        private async Task AddUsageHistoryAsync(int userId, string userName, string role, string action, int poiId, string poiName)
        {
            _context.UsageHistories.Add(new UsageHistory
            {
                UserId = userId,
                UserName = userName,
                Role = role,
                Action = action,
                PoiId = poiId,
                PoiName = poiName,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private static string BuildGuestDisplayName(string? remoteIp, string? guestId)
        {
            var normalizedIp = string.IsNullOrWhiteSpace(remoteIp)
                ? "unknown-ip"
                : remoteIp.Replace(":", "-").Replace("%", "-").Trim();
            var suffix = string.IsNullOrWhiteSpace(guestId)
                ? "guest"
                : (guestId.Trim().Length > 6 ? guestId.Trim()[^6..] : guestId.Trim());

            return $"guid-{normalizedIp}-{suffix}";
        }
    }

    public sealed class PoiViewHistoryRequest
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? GuestId { get; set; }
        public string? Action { get; set; }
        public int PoiId { get; set; }
    }
}
