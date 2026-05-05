using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhFood.API.Data;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers;

public class PaymentController : Controller
{
    private readonly AppDbContext _dbContext;

    public PaymentController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string status = "all")
    {
        var adminGate = EnsureAdminAccess();
        if (adminGate is not null)
        {
            return adminGate;
        }

        var normalizedStatus = NormalizeStatus(status);
        var query = _dbContext.Subscriptions.AsNoTracking().AsQueryable();

        if (!string.Equals(normalizedStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(item => item.Status == normalizedStatus);
        }

        var subscriptions = await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        var model = new PaymentHistoryViewModel
        {
            SelectedStatus = normalizedStatus,
            TotalPayments = subscriptions.Count,
            ActivePayments = subscriptions.Count(item => string.Equals(item.Status, "Active", StringComparison.OrdinalIgnoreCase)),
            ClaimedPayments = subscriptions.Count(item => !string.IsNullOrWhiteSpace(item.ClaimedGuestId)),
            TotalRevenue = subscriptions.Sum(item => item.Amount),
            Items = subscriptions.Select(item => new PaymentHistoryItem
            {
                Id = item.Id,
                GuestId = item.GuestId,
                PaymentCode = item.PaymentCode,
                Status = item.Status,
                Amount = item.Amount,
                CreatedAt = item.CreatedAt,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                ClaimToken = item.ClaimToken,
                ClaimedGuestId = item.ClaimedGuestId,
                ClaimedAtUtc = item.ClaimedAtUtc
            }).ToList()
        };

        return View(model);
    }

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToLowerInvariant() switch
        {
            "active" => "Active",
            "expired" => "Expired",
            _ => "all"
        };
    }

    private IActionResult? EnsureAdminAccess()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrWhiteSpace(role))
        {
            return RedirectToAction("Login", "Account");
        }

        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? null
            : RedirectToAction("Index", "Owner");
    }
}
