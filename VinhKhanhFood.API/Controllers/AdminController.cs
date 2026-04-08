using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/pending-owners
        [HttpGet("pending-owners")]
        public async Task<ActionResult<List<Owner>>> GetPendingOwners()
        {
            var pendingOwners = await _context.Owners
                .Where(o => o.Status == "Pending")
                .Include(o => o.FoodLocations)
                .ToListAsync();

            return Ok(pendingOwners);
        }

        // GET: api/admin/all-owners
        [HttpGet("all-owners")]
        public async Task<ActionResult<List<Owner>>> GetAllOwners()
        {
            var owners = await _context.Owners
                .Include(o => o.FoodLocations)
                .ToListAsync();

            return Ok(owners);
        }

        // GET: api/admin/owners/{status}
        [HttpGet("owners/{status}")]
        public async Task<ActionResult<List<Owner>>> GetOwnersByStatus(string status)
        {
            var owners = await _context.Owners
                .Where(o => o.Status == status)
                .Include(o => o.FoodLocations)
                .ToListAsync();

            return Ok(owners);
        }

        // POST: api/admin/approve-owner/{id}
        [HttpPost("approve-owner/{id}")]
        public async Task<IActionResult> ApproveOwner(int id)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound("Owner not found");

            owner.Status = "Approved";
            owner.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Owner approved successfully", owner });
        }

        // POST: api/admin/reject-owner/{id}
        [HttpPost("reject-owner/{id}")]
        public async Task<IActionResult> RejectOwner(int id, [FromBody] RejectDto dto)
        {
            var owner = await _context.Owners.FindAsync(id);
            if (owner == null)
                return NotFound("Owner not found");

            owner.Status = "Rejected";
            owner.RejectionReason = dto.Reason;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Owner rejected successfully", owner });
        }

        // GET: api/admin/pending-foodlocations
        [HttpGet("pending-foodlocations")]
        public async Task<ActionResult<List<FoodLocation>>> GetPendingFoodLocations()
        {
            var pendingLocations = await _context.FoodLocations
                .Where(f => f.Status == "Pending")
                .Include(f => f.Owner)
                .ToListAsync();

            return Ok(pendingLocations);
        }

        // GET: api/admin/foodlocations/{status}
        [HttpGet("foodlocations/{status}")]
        public async Task<ActionResult<List<FoodLocation>>> GetFoodLocationsByStatus(string status)
        {
            var locations = await _context.FoodLocations
                .Where(f => f.Status == status)
                .Include(f => f.Owner)
                .ToListAsync();

            return Ok(locations);
        }

        // POST: api/admin/approve-foodlocation/{id}
        [HttpPost("approve-foodlocation/{id}")]
        public async Task<IActionResult> ApproveFoodLocation(int id)
        {
            var foodLocation = await _context.FoodLocations.FindAsync(id);
            if (foodLocation == null)
                return NotFound("Food location not found");

            foodLocation.Status = "Approved";
            foodLocation.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Food location approved successfully", foodLocation });
        }

        // POST: api/admin/reject-foodlocation/{id}
        [HttpPost("reject-foodlocation/{id}")]
        public async Task<IActionResult> RejectFoodLocation(int id, [FromBody] RejectDto dto)
        {
            var foodLocation = await _context.FoodLocations.FindAsync(id);
            if (foodLocation == null)
                return NotFound("Food location not found");

            foodLocation.Status = "Rejected";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Food location rejected successfully", foodLocation });
        }

        // GET: api/admin/owner/{id}/foodlocations
        [HttpGet("owner/{id}/foodlocations")]
        public async Task<ActionResult<List<FoodLocation>>> GetOwnerFoodLocationsForApproval(int id)
        {
            var foodLocations = await _context.FoodLocations
                .Where(f => f.OwnerId == id)
                .Include(f => f.Owner)
                .ToListAsync();

            if (!foodLocations.Any())
                return NotFound("No food locations found for this owner");

            return Ok(foodLocations);
        }

        // GET: api/admin/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<AdminStatistics>> GetStatistics()
        {
            var totalOwners = await _context.Owners.CountAsync();
            var pendingOwners = await _context.Owners.CountAsync(o => o.Status == "Pending");
            var approvedOwners = await _context.Owners.CountAsync(o => o.Status == "Approved");
            var rejectedOwners = await _context.Owners.CountAsync(o => o.Status == "Rejected");

            var totalFoodLocations = await _context.FoodLocations.CountAsync();
            var pendingFoodLocations = await _context.FoodLocations.CountAsync(f => f.Status == "Pending");
            var approvedFoodLocations = await _context.FoodLocations.CountAsync(f => f.Status == "Approved");
            var rejectedFoodLocations = await _context.FoodLocations.CountAsync(f => f.Status == "Rejected");

            return Ok(new AdminStatistics
            {
                TotalOwners = totalOwners,
                PendingOwners = pendingOwners,
                ApprovedOwners = approvedOwners,
                RejectedOwners = rejectedOwners,
                TotalFoodLocations = totalFoodLocations,
                PendingFoodLocations = pendingFoodLocations,
                ApprovedFoodLocations = approvedFoodLocations,
                RejectedFoodLocations = rejectedFoodLocations
            });
        }
    }

    // DTOs
    public class RejectDto
    {
        public string Reason { get; set; }
    }

    public class AdminStatistics
    {
        public int TotalOwners { get; set; }
        public int PendingOwners { get; set; }
        public int ApprovedOwners { get; set; }
        public int RejectedOwners { get; set; }
        public int TotalFoodLocations { get; set; }
        public int PendingFoodLocations { get; set; }
        public int ApprovedFoodLocations { get; set; }
        public int RejectedFoodLocations { get; set; }
    }
}
