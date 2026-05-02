using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.Admin.Models;
using VinhKhanhFood.API.Data;
using ApiFoodLocation = VinhKhanhFood.API.Models.FoodLocation;

namespace VinhKhanhFood.Admin.Controllers;

public class PublicController : Controller
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public PublicController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("/public")]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Read directly from database - no API needed
            var pois = await _context.FoodLocations
                .Where(p => p.Status == "Approved")
                .OrderBy(p => p.Id)
                .ToListAsync();

            // Check if user has active subscription
            var guestId = Request.Cookies["GuestId"];
            var hasSubscription = false;
            DateTime? subscriptionEndDate = null;

            if (!string.IsNullOrWhiteSpace(guestId))
            {
                var subscription = await _context.Subscriptions
                    .Where(s => s.GuestId == guestId && s.Status == "Active")
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (subscription != null && subscription.EndDate > DateTime.UtcNow)
                {
                    hasSubscription = true;
                    subscriptionEndDate = subscription.EndDate;
                }
            }

            ViewBag.HasSubscription = hasSubscription;
            ViewBag.SubscriptionEndDate = subscriptionEndDate;

            return View(pois);
        }
        catch
        {
            return View(new List<ApiFoodLocation>());
        }
    }

    [HttpGet("/public/poi/{id}")]
    public async Task<IActionResult> Poi(int id, string? payment)
    {
        try
        {
            // Read directly from database - no API needed
            var poi = await _context.FoodLocations.FindAsync(id);
            if (poi == null)
            {
                return NotFound("POI not found");
            }

            // Check if user has active subscription (from cookie)
            var guestId = Request.Cookies["GuestId"];
            var hasPaid = false;

            if (!string.IsNullOrWhiteSpace(guestId))
            {
                // Check if subscription exists and is valid
                var subscription = await _context.Subscriptions
                    .Where(s => s.GuestId == guestId && s.Status == "Active")
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (subscription != null && subscription.EndDate > DateTime.UtcNow)
                {
                    hasPaid = true;
                }
            }

            // If payment code provided, verify it
            if (!hasPaid && !string.IsNullOrWhiteSpace(payment))
            {
                hasPaid = IsValidPaymentCode(payment);
            }

            var viewModel = new PublicPoiViewModel
            {
                PoiId = poi.Id,
                Name = poi.Name,
                Description = poi.Description,
                ImageUrl = poi.ImageUrl,
                Latitude = poi.Latitude,
                Longitude = poi.Longitude,
                HasPaid = hasPaid,
                Amount = 50000m,
                PaymentQrCode = GeneratePaymentQrCode(),
                PaymentQrImageUrl = GeneratePaymentQrImageUrl()
            };

            return View(viewModel);
        }
        catch
        {
            return NotFound("POI not found");
        }
    }

    [HttpPost("/public/poi/{id}/verify-payment")]
    public async Task<IActionResult> VerifyPayment(int id, [FromBody] PaymentVerificationRequest request)
    {
        if (request.PoiId != id)
        {
            return BadRequest(new { success = false, message = "Invalid POI ID" });
        }

        // Verify payment code
        if (IsValidPaymentCode(request.TransactionCode))
        {
            // Generate or get GuestId
            var guestId = Request.Cookies["GuestId"];
            if (string.IsNullOrWhiteSpace(guestId))
            {
                guestId = Guid.NewGuid().ToString();
            }

            // Create 5-year subscription
            var subscription = new VinhKhanhFood.API.Models.Subscription
            {
                GuestId = guestId,
                PaymentCode = request.TransactionCode,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(5),
                Status = "Active",
                Amount = 50000m,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Set cookie for 5 years
            Response.Cookies.Append("GuestId", guestId, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(5),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            var redirectUrl = $"/public/poi/{id}";
            return Ok(new { success = true, redirectUrl, message = "Đã kích hoạt gói 5 năm! Bạn có thể truy cập tất cả POIs." });
        }

        return BadRequest(new { success = false, message = "Invalid payment code" });
    }

    private bool IsValidPaymentCode(string code)
    {
        // Demo mode: Accept codes in format "PAY-XXXX" or "DEMO-5YEAR"
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return code.StartsWith("PAY-", StringComparison.OrdinalIgnoreCase) ||
               code.Equals("DEMO-5YEAR", StringComparison.OrdinalIgnoreCase);
    }

    private string GeneratePaymentQrCode()
    {
        // Generate a demo payment code for 5-year subscription
        return $"PAY-5YEAR-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private string GeneratePaymentQrImageUrl()
    {
        var paymentCode = GeneratePaymentQrCode();
        var paymentUri = $"vinhkhanhpay://subscribe?code={paymentCode}";
        return $"https://quickchart.io/qr?size=300&text={Uri.EscapeDataString(paymentUri)}";
    }
}
