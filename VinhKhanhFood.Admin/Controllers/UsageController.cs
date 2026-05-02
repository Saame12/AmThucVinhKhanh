using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers;

public class UsageController : Controller
{
    private static readonly TimeSpan LiveAudioWindow = TimeSpan.FromSeconds(30);
    private readonly HttpClient _http;

    public UsageController(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("MyAPI");
    }

    public async Task<IActionResult> Index(string period = "week")
    {
        try
        {
            var history = await _http.GetFromJsonAsync<List<UsageHistory>>("Food/history") ?? new List<UsageHistory>();
            var devices = await _http.GetFromJsonAsync<List<User>>("User") ?? new List<User>();
            var pois = await _http.GetFromJsonAsync<List<FoodLocation>>("Food") ?? new List<FoodLocation>();

            var model = BuildDashboard(history, devices, pois, period);
            return View(model);
        }
        catch
        {
            TempData["Error"] = "Khong the tai lich su su dung vi API chua chay.";
            return View(new UsageDashboardViewModel
            {
                SelectedPeriod = NormalizePeriod(period),
                PeriodLabel = GetPeriodLabel(NormalizePeriod(period))
            });
        }
    }

    public async Task<IActionResult> Devices()
    {
        try
        {
            var users = await _http.GetFromJsonAsync<List<User>>("User") ?? new List<User>();
            var deviceUsers = users
                .Where(IsTravelerDevice)
                .OrderByDescending(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(user => user.LastSeenUtc)
                .ToList();

            var nowUtc = DateTime.UtcNow;
            var model = new DeviceHistoryViewModel
            {
                ActiveDevicesNow = deviceUsers.Count(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase)),
                ActiveDevicesLast24Hours = deviceUsers.Count(user => user.LastSeenUtc.HasValue && nowUtc - user.LastSeenUtc.Value <= TimeSpan.FromHours(24)),
                ActiveDevicesThisMonth = deviceUsers.Count(user => user.LastSeenUtc.HasValue && user.LastSeenUtc.Value.Year == nowUtc.Year && user.LastSeenUtc.Value.Month == nowUtc.Month),
                Devices = deviceUsers
            };

            return View(model);
        }
        catch
        {
            TempData["Error"] = "Khong the tai lich su thiet bi vi API chua chay.";
            return View(new DeviceHistoryViewModel());
        }
    }

    private static UsageDashboardViewModel BuildDashboard(List<UsageHistory> allHistory, List<User> devices, List<FoodLocation> pois, string period)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var periodRange = ResolvePeriodRange(normalizedPeriod);
        var nowUtc = DateTime.UtcNow;
        var deviceUsers = devices.Where(IsTravelerDevice).ToList();

        var travelerHistory = allHistory
            .Where(item => !string.Equals(item.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            .Where(item => !string.Equals(item.Role, "Owner", StringComparison.OrdinalIgnoreCase))
            .Where(item => item.CreatedAt >= periodRange.Start && item.CreatedAt < periodRange.End)
            .OrderByDescending(item => item.CreatedAt)
            .ToList();

        var viewHistory = travelerHistory
            .Where(item => string.Equals(item.Action, "VIEW_DETAIL", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var audioHistory = travelerHistory
            .Where(item => string.Equals(item.Action, "AUDIO_PLAY", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var viewRankings = BuildPoiRanking(viewHistory);
        var audioRankings = BuildPoiRanking(audioHistory);
        var heatmapPoints = BuildHeatmapPoints(audioRankings, pois);
        var activePoiVisitors = deviceUsers
            .Where(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase))
            .Where(user => string.Equals(user.LocationZoneStatus, "AT_POI", StringComparison.OrdinalIgnoreCase))
            .Where(user => user.CurrentPoiId.HasValue && !string.IsNullOrWhiteSpace(user.CurrentPoiName))
            .GroupBy(user => new { PoiId = user.CurrentPoiId!.Value, PoiName = user.CurrentPoiName! })
            .Select(group => new VisitorPoiPresenceStat
            {
                PoiId = group.Key.PoiId,
                PoiName = group.Key.PoiName,
                VisitorCount = group.Count()
            })
            .OrderByDescending(item => item.VisitorCount)
            .ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var betweenPoiVisitors = deviceUsers
            .Where(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase))
            .Where(user => string.Equals(user.LocationZoneStatus, "BETWEEN_POIS", StringComparison.OrdinalIgnoreCase))
            .Where(user => !string.IsNullOrWhiteSpace(user.CurrentPoiName) && !string.IsNullOrWhiteSpace(user.SecondaryPoiName))
            .GroupBy(user => $"{user.CurrentPoiName} ↔ {user.SecondaryPoiName}")
            .Select(group => new VisitorBetweenPoiStat
            {
                RouteLabel = group.Key,
                VisitorCount = group.Count()
            })
            .OrderByDescending(item => item.VisitorCount)
            .ThenBy(item => item.RouteLabel, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var liveAudioQueues = deviceUsers
            .Where(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase))
            .Where(IsLiveAudioActive)
            .Where(user => user.CurrentAudioPoiId.HasValue && !string.IsNullOrWhiteSpace(user.CurrentAudioPoiName))
            .GroupBy(user => new { PoiId = user.CurrentAudioPoiId!.Value, PoiName = user.CurrentAudioPoiName! })
            .Select(group => new PoiLiveAudioQueue
            {
                PoiId = group.Key.PoiId,
                PoiName = group.Key.PoiName,
                ListenerCount = group.Count(),
                Listeners = group
                    .OrderByDescending(user => user.LastAudioHeartbeatUtc)
                    .Select((user, index) => new PoiLiveAudioQueueItem
                    {
                        QueuePosition = index + 1,
                        VisitorName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName,
                        LastHeartbeatUtc = user.LastAudioHeartbeatUtc
                    })
                    .ToList()
            })
            .OrderByDescending(item => item.ListenerCount)
            .ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new UsageDashboardViewModel
        {
            SelectedPeriod = normalizedPeriod,
            PeriodLabel = GetPeriodLabel(normalizedPeriod),
            TotalTravelerViews = viewHistory.Count,
            TotalAudioPlays = audioHistory.Count,
            DistinctPois = viewRankings.Select(item => item.PoiId).Union(audioRankings.Select(item => item.PoiId)).Count(),
            ActiveDevicesNow = deviceUsers.Count(user => string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase)),
            ActiveDevicesLast24Hours = deviceUsers.Count(user => user.LastSeenUtc.HasValue && nowUtc - user.LastSeenUtc.Value <= TimeSpan.FromHours(24)),
            ActiveDevicesThisMonth = deviceUsers.Count(user => user.LastSeenUtc.HasValue && user.LastSeenUtc.Value.Year == nowUtc.Year && user.LastSeenUtc.Value.Month == nowUtc.Month),
            VisitorsAtPoiNow = activePoiVisitors.Sum(item => item.VisitorCount),
            VisitorsBetweenPoisNow = betweenPoiVisitors.Sum(item => item.VisitorCount),
            ActiveAudioListenersNow = liveAudioQueues.Sum(item => item.ListenerCount),
            MostVisitedPoi = viewRankings.FirstOrDefault(),
            LeastVisitedPoi = viewRankings.OrderBy(item => item.VisitCount).ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(),
            MostPlayedPoi = audioRankings.FirstOrDefault(),
            LeastPlayedPoi = audioRankings.OrderBy(item => item.VisitCount).ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(),
            ViewRankings = viewRankings,
            AudioRankings = audioRankings,
            HeatmapPoints = heatmapPoints,
            VisitorPresence = activePoiVisitors,
            BetweenPoiVisitors = betweenPoiVisitors,
            LiveAudioQueues = liveAudioQueues,
            HistoryItems = travelerHistory.Take(100).ToList()
        };
    }

    private static List<PoiUsageStat> BuildPoiRanking(List<UsageHistory> history)
    {
        return history
            .GroupBy(item => new { item.PoiId, PoiName = item.PoiName ?? $"POI {item.PoiId}" })
            .Select(group => new PoiUsageStat
            {
                PoiId = group.Key.PoiId,
                PoiName = group.Key.PoiName,
                VisitCount = group.Count()
            })
            .OrderByDescending(item => item.VisitCount)
            .ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<PoiHeatmapPoint> BuildHeatmapPoints(List<PoiUsageStat> audioRankings, List<FoodLocation> pois)
    {
        var maxIntensity = audioRankings.Count == 0 ? 0 : audioRankings.Max(item => item.VisitCount);

        return audioRankings
            .Join(
                pois,
                ranking => ranking.PoiId,
                poi => poi.Id,
                (ranking, poi) => new PoiHeatmapPoint
                {
                    PoiId = poi.Id,
                    PoiName = poi.Name,
                    Latitude = poi.Latitude,
                    Longitude = poi.Longitude,
                    Intensity = ranking.VisitCount,
                    IntensityRatio = maxIntensity == 0 ? 0 : Math.Round((double)ranking.VisitCount / maxIntensity, 2)
                })
            .OrderByDescending(item => item.Intensity)
            .ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsTravelerDevice(User user) =>
        !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(user.Role, "Owner", StringComparison.OrdinalIgnoreCase);

    private static bool IsLiveAudioActive(User user) =>
        user.LastAudioHeartbeatUtc.HasValue &&
        DateTime.UtcNow - user.LastAudioHeartbeatUtc.Value <= LiveAudioWindow;

    private static string NormalizePeriod(string? period) => period?.Trim().ToLowerInvariant() switch
    {
        "month" => "month",
        "year" => "year",
        _ => "week"
    };

    private static (DateTime Start, DateTime End) ResolvePeriodRange(string period)
    {
        var now = DateTime.Now;
        return period switch
        {
            "month" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)),
            "year" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1)),
            _ => ResolveWeekRange(now)
        };
    }

    private static (DateTime Start, DateTime End) ResolveWeekRange(DateTime now)
    {
        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var start = now.Date.AddDays(-diff);
        return (start, start.AddDays(7));
    }

    private static string GetPeriodLabel(string period) => period switch
    {
        "month" => "Thang nay",
        "year" => "Nam nay",
        _ => "Tuan nay"
    };
}
