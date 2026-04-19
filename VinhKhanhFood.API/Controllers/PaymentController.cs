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
}
