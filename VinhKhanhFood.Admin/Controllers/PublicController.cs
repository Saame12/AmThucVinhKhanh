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
            var guestId = GetOrCreateWebGuestId();
            await EnsureWebGuestPresenceAsync(guestId);

            var pois = await _context.FoodLocations
                .Where(poi => poi.Status == "Approved")
                .OrderBy(poi => poi.Id)
                .ToListAsync();

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

            var guestId = GetOrCreateWebGuestId();
            await EnsureWebGuestPresenceAsync(guestId);

            var activeSubscription = await GetActiveSubscriptionAsync(guestId);
            var viewModel = new PublicPoiViewModel
            {
                PoiId = poi.Id,
                Name = poi.Name,
                Description = poi.Description,
                ImageUrl = poi.ImageUrl,
                Latitude = poi.Latitude,
                Longitude = poi.Longitude,
                HasDefaultAudio = !string.IsNullOrWhiteSpace(poi.Description),
                HasPaid = activeSubscription is not null,
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
            guestId = GetOrCreateWebGuestId();
        }

        await EnsureWebGuestPresenceAsync(guestId);

        var subscription = new Subscription
        {
            GuestId = guestId,
            PaymentCode = request.TransactionCode,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(5),
            Status = "Active",
            Amount = PublicUnlockPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        Response.Cookies.Append("GuestId", guestId, BuildPublicGuestCookieOptions());

        var redirectUrl = $"/public/poi/{id}";

        return Ok(new
        {
            success = true,
            redirectUrl,
            message = "Da mo khoa thanh cong. Ban co the su dung day du chuc nang tren web public."
        });
    }

    private string GetOrCreateWebGuestId()
    {
        var guestId = Request.Cookies["GuestId"];
        if (!string.IsNullOrWhiteSpace(guestId))
        {
            return guestId.Trim();
        }

        guestId = Guid.NewGuid().ToString("N");
        Response.Cookies.Append("GuestId", guestId, BuildPublicGuestCookieOptions());
        return guestId;
    }

    private async Task EnsureWebGuestPresenceAsync(string guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
        {
            return;
        }

        var normalizedGuestId = guestId.Trim();
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var displayName = BuildWebGuestDisplayName(remoteIp, normalizedGuestId);
        var user = await _context.Users.FirstOrDefaultAsync(item =>
            item.IsVirtual &&
            item.GuestId == normalizedGuestId &&
            item.Role == "WebGuest");

        if (user is null)
        {
            user = new VinhKhanhFood.API.Models.User
            {
                Username = displayName,
                Password = string.Empty,
                FullName = displayName,
                Role = "WebGuest",
                Status = "Active",
                IsVirtual = true,
                GuestId = normalizedGuestId,
                RemoteIp = remoteIp,
                LastSeenUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
        }
        else
        {
            user.Username = displayName;
            user.FullName = displayName;
            user.RemoteIp = remoteIp;
            user.LastSeenUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private CookieOptions BuildPublicGuestCookieOptions() =>
        new()
        {
            Expires = DateTimeOffset.UtcNow.AddYears(5),
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        };

    private static string BuildWebGuestDisplayName(string? remoteIp, string guestId)
    {
        var normalizedIp = string.IsNullOrWhiteSpace(remoteIp)
            ? "unknown-ip"
            : remoteIp.Replace(":", "-").Replace("%", "-").Trim();
        var suffix = guestId.Length > 6 ? guestId[^6..] : guestId;
        return $"web-guid-{normalizedIp}-{suffix}";
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

    private static bool IsValidPaymentCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return code.StartsWith("WEB-10K-", StringComparison.OrdinalIgnoreCase) ||
               code.Equals("DEMO-10K", StringComparison.OrdinalIgnoreCase);
    }

    private static string GeneratePaymentQrCode() =>
        $"WEB-10K-{DateTime.UtcNow:yyyyMMddHHmmss}";

    private static string GeneratePaymentQrImageUrl()
    {
        var paymentCode = GeneratePaymentQrCode();
        var paymentUri = $"vinhkhanhweb://unlock?code={paymentCode}&amount={PublicUnlockPrice:0}";
        return $"https://quickchart.io/qr?size=300&text={Uri.EscapeDataString(paymentUri)}";
    }
}
