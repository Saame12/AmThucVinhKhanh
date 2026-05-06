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

        var webPayments = subscriptions.Count(item => string.Equals(ResolveSource(item), "Web", StringComparison.OrdinalIgnoreCase));
        var appPayments = subscriptions.Count(item => string.Equals(ResolveSource(item), "App", StringComparison.OrdinalIgnoreCase));

        var model = new PaymentHistoryViewModel
        {
            SelectedStatus = normalizedStatus,
            TotalPayments = subscriptions.Count,
            ActivePayments = subscriptions.Count(item => string.Equals(item.Status, "Active", StringComparison.OrdinalIgnoreCase)),
            WebPayments = webPayments,
            AppPayments = appPayments,
            TotalRevenue = subscriptions.Sum(item => item.Amount),
            Items = subscriptions.Select(item => new PaymentHistoryItem
            {
                Id = item.Id,
                GuestId = item.GuestId,
                PaymentCode = item.PaymentCode,
                Status = item.Status,
                Amount = item.Amount,
                Source = ResolveSource(item),
                CreatedAt = item.CreatedAt,
                StartDate = item.StartDate,
                EndDate = item.EndDate
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

    private static string ResolveSource(VinhKhanhFood.API.Models.Subscription item)
    {
        return item.PaymentCode.StartsWith("APP-10K-", StringComparison.OrdinalIgnoreCase)
            ? "App"
            : "Web";
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
