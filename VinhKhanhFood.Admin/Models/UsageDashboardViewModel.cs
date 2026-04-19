namespace VinhKhanhFood.Admin.Models;

public sealed class UsageDashboardViewModel
{
    public string SelectedPeriod { get; set; } = "week";
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalTravelerViews { get; set; }
    public int TotalAudioPlays { get; set; }
    public int DistinctPois { get; set; }
    public int ActiveDevicesNow { get; set; }
    public int ActiveDevicesLast24Hours { get; set; }
    public int ActiveDevicesThisMonth { get; set; }
    public PoiUsageStat? MostVisitedPoi { get; set; }
    public PoiUsageStat? LeastVisitedPoi { get; set; }
    public PoiUsageStat? MostPlayedPoi { get; set; }
    public PoiUsageStat? LeastPlayedPoi { get; set; }
    public List<PoiUsageStat> ViewRankings { get; set; } = [];
    public List<PoiUsageStat> AudioRankings { get; set; } = [];
    public List<PoiHeatmapPoint> HeatmapPoints { get; set; } = [];
    public List<UsageHistory> HistoryItems { get; set; } = [];
}

public sealed class PoiUsageStat
{
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
}

public sealed class PoiHeatmapPoint
{
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Intensity { get; set; }
    public double IntensityRatio { get; set; }
}

public sealed class DeviceHistoryViewModel
{
    public int ActiveDevicesNow { get; set; }
    public int ActiveDevicesLast24Hours { get; set; }
    public int ActiveDevicesThisMonth { get; set; }
    public List<User> Devices { get; set; } = [];
}
