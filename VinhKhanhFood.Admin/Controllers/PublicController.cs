using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.Admin.Models;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;
using ApiFoodLocation = VinhKhanhFood.API.Models.FoodLocation;

namespace VinhKhanhFood.Admin.Controllers;

public class PublicController : Controller
{
    private const decimal PublicUnlockPrice = 10000m;
    private readonly AppDbContext _context;

    public PublicController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/public")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var pois = await _context.FoodLocations
                .Where(poi => poi.Status == "Approved")
                .OrderBy(poi => poi.Id)
                .ToListAsync();

            var guestId = Request.Cookies["GuestId"];
            var activeSubscription = await GetActiveSubscriptionAsync(guestId);

            ViewBag.HasSubscription = activeSubscription is not null;
            ViewBag.SubscriptionEndDate = activeSubscription?.EndDate;

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
            var poi = await _context.FoodLocations.FindAsync(id);
            if (poi is null)
            {
                return NotFound("POI not found");
            }

            var guestId = Request.Cookies["GuestId"];
            var activeSubscription = await GetActiveSubscriptionAsync(guestId);
            var claimToken = ResolveClaimToken(activeSubscription, payment);

            var viewModel = new PublicPoiViewModel
            {
                PoiId = poi.Id,
                Name = poi.Name,
                Description = poi.Description,
                ImageUrl = poi.ImageUrl,
                Latitude = poi.Latitude,
                Longitude = poi.Longitude,
                HasPaid = activeSubscription is not null,
                HasClaimToken = !string.IsNullOrWhiteSpace(claimToken),
                ClaimToken = claimToken ?? string.Empty,
                OpenInAppUrl = !string.IsNullOrWhiteSpace(claimToken)
                    ? $"vinhkhanhfood://unlock?token={Uri.EscapeDataString(claimToken)}&poiId={poi.Id}"
                    : string.Empty,
                Amount = PublicUnlockPrice,
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

        if (!IsValidPaymentCode(request.TransactionCode))
        {
            return BadRequest(new { success = false, message = "Invalid payment code" });
        }

        var guestId = Request.Cookies["GuestId"];
        if (string.IsNullOrWhiteSpace(guestId))
        {
            guestId = Guid.NewGuid().ToString("N");
        }

        var claimToken = Guid.NewGuid().ToString("N");
        var subscription = new Subscription
        {
            GuestId = guestId,
            PaymentCode = request.TransactionCode,
            ClaimToken = claimToken,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(5),
            Status = "Active",
            Amount = PublicUnlockPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        Response.Cookies.Append("GuestId", guestId, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(5),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });

        var redirectUrl = $"/public/poi/{id}?payment={Uri.EscapeDataString(claimToken)}";

        return Ok(new
        {
            success = true,
            redirectUrl,
            claimToken,
            message = "Da mo khoa thanh cong. Ban co the tiep tuc tren web hoac mo trong app."
        });
    }

    private async Task<Subscription?> GetActiveSubscriptionAsync(string? guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
        {
            return null;
        }

        return await _context.Subscriptions
            .Where(subscription =>
                subscription.GuestId == guestId &&
                subscription.Status == "Active" &&
                subscription.EndDate > DateTime.UtcNow)
            .OrderByDescending(subscription => subscription.EndDate)
            .FirstOrDefaultAsync();
    }

    private static string? ResolveClaimToken(Subscription? activeSubscription, string? claimTokenQuery)
    {
        if (!string.IsNullOrWhiteSpace(claimTokenQuery))
        {
            return claimTokenQuery.Trim();
        }

        if (activeSubscription is not null && !string.IsNullOrWhiteSpace(activeSubscription.ClaimToken))
        {
            return activeSubscription.ClaimToken.Trim();
        }

        return null;
    }

    private static bool IsValidPaymentCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return code.StartsWith("PAY-10K-", StringComparison.OrdinalIgnoreCase) ||
               code.Equals("DEMO-10K", StringComparison.OrdinalIgnoreCase);
    }

    private static string GeneratePaymentQrCode() =>
        $"PAY-10K-{DateTime.UtcNow:yyyyMMddHHmmss}";

    private static string GeneratePaymentQrImageUrl()
    {
        var paymentCode = GeneratePaymentQrCode();
        var paymentUri = $"vinhkhanhpay://unlock?code={paymentCode}&amount={PublicUnlockPrice:0}";
        return $"https://quickchart.io/qr?size=300&text={Uri.EscapeDataString(paymentUri)}";
    }
}
