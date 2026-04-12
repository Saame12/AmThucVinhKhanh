using Microsoft.Maui.Devices.Sensors;
using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App.ViewModels;

public sealed class MapViewModel
{
    private readonly ApiService _apiService;
    private readonly AudioGuideService _audioGuideService;

    private CancellationTokenSource? _trackingCts;
    private bool _isInitialized;
    private bool _isTrackingLocation;
    private readonly HashSet<int> _announcedPoiIds = new();

    public MapViewModel()
        : this(new ApiService(), App.AudioGuide)
    {
    }

    public MapViewModel(ApiService apiService, AudioGuideService audioGuideService)
    {
        _apiService = apiService;
        _audioGuideService = audioGuideService;
    }

    public List<FoodLocation> Locations { get; private set; } = new();

    public event Action<List<FoodLocation>>? OnLocationsLoaded;

    public async Task InitializeAsync()
    {
        if (!_isInitialized || Locations.Count == 0)
        {
            await LoadDataAsync();
            _isInitialized = Locations.Count > 0;
        }

        await StartTrackingLocationAsync();
    }

    public void StopTracking()
    {
        _isTrackingLocation = false;
        _trackingCts?.Cancel();
    }

    public void ResetAutoGuide()
    {
        _announcedPoiIds.Clear();
    }

    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            return await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
        }
        catch
        {
            return null;
        }
    }

    public void CancelSpeech() => _audioGuideService.Cancel();

    public async Task<bool> PlayPoiAudioAsync(FoodLocation poi, CancellationToken cancellationToken = default)
    {
        return await _audioGuideService.PlayPoiAsync(poi, cancellationToken);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            Locations = await _apiService.GetFoodLocationsAsync();
            OnLocationsLoaded?.Invoke(Locations);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load data failed: {ex.Message}");
        }
    }

    private async Task StartTrackingLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted || _isTrackingLocation)
            {
                return;
            }

            _isTrackingLocation = true;
            _trackingCts = new CancellationTokenSource();

            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromSeconds(4), () =>
            {
                if (!_isTrackingLocation || _trackingCts.IsCancellationRequested)
                {
                    return false;
                }

                _ = PollLocationAsync(_trackingCts.Token);
                return true;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Start tracking failed: {ex.Message}");
        }
    }

    private async Task PollLocationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)),
                cancellationToken);

            if (location is not null)
            {
                await CheckGeofenceAsync(location, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Poll location failed: {ex.Message}");
        }
    }

    private async Task CheckGeofenceAsync(Location userLocation, CancellationToken cancellationToken)
    {
        if (Locations.Count == 0)
        {
            return;
        }

        var closestPoi = Locations
            .Select(poi => new
            {
                Poi = poi,
                DistanceInKm = userLocation.CalculateDistance(
                    new Location(poi.Latitude, poi.Longitude),
                    DistanceUnits.Kilometers)
            })
            .Where(item => item.DistanceInKm <= 0.03)
            .Where(item => !_announcedPoiIds.Contains(item.Poi.Id))
            .OrderBy(item => item.DistanceInKm)
            .Select(item => item.Poi)
            .FirstOrDefault();

        if (closestPoi is null)
        {
            return;
        }

        var started = await _audioGuideService.AutoPlayPoiAsync(closestPoi, cancellationToken);
        if (started)
        {
            _announcedPoiIds.Add(closestPoi.Id);
        }
    }
}
