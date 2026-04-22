using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? poiId, [FromQuery] string? status)
    {
        var query = FilterTransactions(startDate, endDate, poiId, status);
        var rows = await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return Ok(rows);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? poiId, [FromQuery] string? status)
    {
        var query = FilterTransactions(startDate, endDate, poiId, status);
        var transactions = await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        var paidTransactions = transactions
            .Where(item => string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var dashboard = new
        {
            totalTransactions = transactions.Count,
            paidTransactions = paidTransactions.Count,
            pendingTransactions = transactions.Count(item => string.Equals(item.Status, "Pending", StringComparison.OrdinalIgnoreCase)),
            failedTransactions = transactions.Count(item => string.Equals(item.Status, "Failed", StringComparison.OrdinalIgnoreCase)),
            reconciledTransactions = transactions.Count(item => item.ReconciledAt.HasValue),
            totalRevenue = paidTransactions.Sum(item => item.Amount),
            averageOrderValue = paidTransactions.Count == 0 ? 0 : decimal.Round(paidTransactions.Average(item => item.Amount), 0),
            unlockedPurchases = paidTransactions.Count(item => IsUnlockPurchase(item)),
            topPois = paidTransactions
                .GroupBy(item => new { item.PoiId, item.PoiName })
                .Select(group => new
                {
                    poiId = group.Key.PoiId,
                    poiName = group.Key.PoiName,
                    revenue = group.Sum(item => item.Amount),
                    transactionCount = group.Count()
                })
                .OrderByDescending(item => item.revenue)
                .ThenBy(item => item.poiName)
                .Take(10)
                .ToList(),
            providerBreakdown = transactions
                .GroupBy(item => item.Provider)
                .Select(group => new
                {
                    provider = group.Key,
                    transactionCount = group.Count(),
                    paidCount = group.Count(item => string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase)),
                    revenue = group.Where(item => string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount)
                })
                .OrderByDescending(item => item.revenue)
                .ThenBy(item => item.provider)
                .ToList(),
            transactions
        };

        return Ok(dashboard);
    }

    [HttpGet("access")]
    public async Task<IActionResult> GetProfessionalAccess([FromQuery] int poiId, [FromQuery] int? userId, [FromQuery] string? guestId)
    {
        if (poiId <= 0)
        {
            return BadRequest(new { message = "PoiId is required." });
        }

        var poi = await _context.FoodLocations.FindAsync(poiId);
        if (poi is null)
        {
            return NotFound(new { message = "POI not found." });
        }

        var hasVip = false;
        if (userId.HasValue && userId.Value > 0)
        {
            hasVip = await _context.Users
                .Where(user => user.Id == userId.Value)
                .Select(user => user.IsVip)
                .FirstOrDefaultAsync();
        }

        var hasPoiUnlock = await FindExistingUnlockAsync(poiId, userId, guestId) is not null;

        return Ok(new
        {
            poiId,
            hasProfessionalAudio = poi.HasProfessionalAudio,
            hasAccess = hasVip || hasPoiUnlock,
            accessType = hasVip ? "VIP" : hasPoiUnlock ? "POI_UNLOCK" : "NONE"
        });
    }

    [HttpPost("mock-checkout")]
    public async Task<IActionResult> MockCheckout([FromBody] MockCheckoutRequest request)
    {
        if (request.PoiId <= 0)
        {
            return BadRequest(new { message = "PoiId is required." });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero." });
        }

        var poi = await _context.FoodLocations.FindAsync(request.PoiId);
        if (poi is null)
        {
            return NotFound(new { message = "POI not found." });
        }

        var purchaserDisplayName = await ResolvePurchaserDisplayNameAsync(request.UserId, request.GuestId);

        var transaction = new PaymentTransaction
        {
            TransactionCode = $"QR-{DateTime.Now:yyyyMMddHHmmss}-{request.PoiId:D4}",
            PoiId = poi.Id,
            PoiName = poi.Name,
            UserId = request.UserId > 0 ? request.UserId : null,
            GuestId = request.GuestId?.Trim() ?? string.Empty,
            PurchaserDisplayName = purchaserDisplayName,
            Amount = decimal.Round(request.Amount, 0),
            Currency = "VND",
            PaymentType = "QR_PAYMENT",
            Provider = string.IsNullOrWhiteSpace(request.Provider) ? "MockQR" : request.Provider.Trim(),
            Status = "Paid",
            CustomerLabel = purchaserDisplayName,
            Note = "Mock QR checkout success.",
            CreatedAt = DateTime.Now,
            PaidAt = DateTime.Now
        };

        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        var existingUnlock = await FindExistingUnlockAsync(poi.Id, transaction.UserId, transaction.GuestId);
        if (existingUnlock is null)
        {
            _context.PoiAudioUnlocks.Add(new PoiAudioUnlock
            {
                PoiId = poi.Id,
                UserId = transaction.UserId,
                GuestId = transaction.GuestId,
                PurchaserDisplayName = purchaserDisplayName,
                PaymentTransactionId = transaction.Id,
                UnlockedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            transactionId = transaction.Id,
            transactionCode = transaction.TransactionCode,
            poiId = poi.Id,
            poiName = poi.Name,
            amount = transaction.Amount,
            hasAccess = true,
            accessType = "POI_UNLOCK"
        });
    }

    [HttpPut("reconcile/{id:int}")]
    public async Task<IActionResult> Reconcile(int id)
    {
        var transaction = await _context.PaymentTransactions.FindAsync(id);
        if (transaction is null)
        {
            return NotFound(new { message = "Transaction not found." });
        }

        transaction.ReconciledAt = DateTime.Now;
        transaction.Note = string.Equals(transaction.Status, "Paid", StringComparison.OrdinalIgnoreCase)
            ? "Mock reconcile succeeded."
            : "Mock reconcile flagged non-paid transaction.";

        await _context.SaveChangesAsync();
        return Ok(transaction);
    }

    private IQueryable<PaymentTransaction> FilterTransactions(DateTime? startDate, DateTime? endDate, int? poiId, string? status)
    {
        var query = _context.PaymentTransactions.AsNoTracking().AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(item => item.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(item => item.CreatedAt <= endDate.Value);
        }

        if (poiId.HasValue)
        {
            query = query.Where(item => item.PoiId == poiId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => item.Status == status.Trim());
        }

        return query;
    }

    private async Task<string> ResolvePurchaserDisplayNameAsync(int? userId, string? guestId)
    {
        if (userId.HasValue && userId.Value > 0)
        {
            var user = await _context.Users.FindAsync(userId.Value);
            if (user is not null)
            {
                return string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
            }
        }

        if (!string.IsNullOrWhiteSpace(guestId))
        {
            var virtualUser = await _context.Users.FirstOrDefaultAsync(user => user.IsVirtual && user.GuestId == guestId);
            if (virtualUser is not null)
            {
                return string.IsNullOrWhiteSpace(virtualUser.FullName) ? virtualUser.Username : virtualUser.FullName;
            }

            return $"guid-{guestId.Trim()}";
        }

        return "Guest";
    }

    private Task<PoiAudioUnlock?> FindExistingUnlockAsync(int poiId, int? userId, string? guestId)
    {
        guestId ??= string.Empty;
        return _context.PoiAudioUnlocks.FirstOrDefaultAsync(unlock =>
            unlock.PoiId == poiId &&
            (
                (userId.HasValue && userId.Value > 0 && unlock.UserId == userId.Value) ||
                (!string.IsNullOrWhiteSpace(guestId) && unlock.GuestId == guestId)
            ));
    }

    private static bool IsUnlockPurchase(PaymentTransaction item) =>
        string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase) &&
        (!string.IsNullOrWhiteSpace(item.GuestId) || item.UserId.HasValue);
}

public sealed class MockCheckoutRequest
{
    public int PoiId { get; set; }
    public decimal Amount { get; set; }
    public int? UserId { get; set; }
    public string? GuestId { get; set; }
    public string? Provider { get; set; }
}
