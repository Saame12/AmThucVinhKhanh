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

        // 1. LẤY DANH SÁCH (GET)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodLocation>>> GetLocations()
        {
            var locations = await _context.FoodLocations.ToListAsync();//lỗi đoạn này 
            // THAY SỐ THÀNH SỐ CỔNG HTTP (trong file launchSettings.json)
            var baseUrl = "http://10.0.2.2:5020";
            foreach (var loc in locations)
            {

                if (!string.IsNullOrEmpty(loc.ImageUrl))
                {
                    loc.ImageUrl = $"{baseUrl}/images/{loc.ImageUrl}";

                }
            }
            return Ok(locations);
        }

        // 2. THÊM MỚI (POST)
        [HttpPost]
        public async Task<ActionResult<FoodLocation>> PostFoodLocation(FoodLocation foodLocation)
        {
            if (string.IsNullOrEmpty(foodLocation.Status))
            {
                foodLocation.Status = "Pending";
            }

            _context.FoodLocations.Add(foodLocation);
            await _context.SaveChangesAsync();

            // 🔥 NEW: LOG USAGE HISTORY (CREATE)
            var history = new UsageHistory
            {
                UserId = foodLocation.OwnerId ?? 0,
                UserName = "Owner",
                Role = "Owner", // 🔥 THÊM DÒNG NÀY
                Action = "CREATE",
                PoiId = foodLocation.Id,
                PoiName = foodLocation.Name,
                CreatedAt = DateTime.Now
            };

            _context.UsageHistories.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocations), new { id = foodLocation.Id }, foodLocation);
        }

        // 3. CẬP NHẬT (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodLocation(int id, FoodLocation foodLocation)
        {
            if (id != foodLocation.Id) return BadRequest();
            _context.Entry(foodLocation).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FoodLocations.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            // 🔥 NEW: LOG EDIT
            _context.UsageHistories.Add(new UsageHistory
            {
                UserId = foodLocation.OwnerId ?? 0,
                UserName = "Owner",
                Role = "Owner",
                Action = "EDIT",
                PoiId = foodLocation.Id,
                PoiName = foodLocation.Name,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 4. XÓA (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodLocation(int id)
        {
            //var foodLocation = await _context.FoodLocations.FindAsync(id);
            //if (foodLocation == null) return NotFound();
            //_context.FoodLocations.Remove(foodLocation);
            //await _context.SaveChangesAsync();
            var foodLocation = await _context.FoodLocations.FindAsync(id);
            if (foodLocation == null) return NotFound();

            // 🔥 NEW: LOG DELETE
            _context.UsageHistories.Add(new UsageHistory
            {
                UserId = foodLocation.OwnerId ?? 0,
                UserName = "Owner",
                Role = "Owner",
                Action = "DELETE",
                PoiId = foodLocation.Id,
                PoiName = foodLocation.Name,
                CreatedAt = DateTime.Now
            });

            _context.FoodLocations.Remove(foodLocation);
            await _context.SaveChangesAsync();
            return NoContent();

        }
        // 🔥 NEW: GET BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult<FoodLocation>> GetById(int id)
        {
            var food = await _context.FoodLocations.FindAsync(id);

            if (food == null) return NotFound();

            return Ok(food);
        }
        // 🔥 NEW: APPROVE POI
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var food = await _context.FoodLocations.FindAsync(id);
            if (food == null) return NotFound();

            food.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(food);
        }

        // 🔥 NEW: REJECT POI
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var food = await _context.FoodLocations.FindAsync(id);
            if (food == null) return NotFound();

            food.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(food);
        }
        //HISTORY
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var data = await _context.UsageHistories //lỗi ở đoạn này 
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(data);
        }

    }
}