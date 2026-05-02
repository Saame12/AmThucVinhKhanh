using System.Net.Http.Json;
using VinhKhanhFood.App.Models;

namespace VinhKhanhFood.App.Services;

public sealed class AuthService
{
    private const string PreferenceGuestId = "GuestId";

    private readonly HttpClient _httpClient;
    private PeriodicTimer? _presenceTimer;
    private CancellationTokenSource? _presenceCancellation;
    private PeriodicTimer? _audioTimer;
    private CancellationTokenSource? _audioCancellation;
    private int? _currentAudioPoiId;

    public AuthService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler);
        StartPresenceHeartbeat();
        _ = SetPresenceAsync(true);
    }

    public string GuestId { get; } = GetOrCreateGuestId();

    public async Task SetPresenceAsync(bool isOnline, CancellationToken cancellationToken = default)
    {
        try
        {
            await UpdateGuestPresenceAsync(isOnline, cancellationToken);
        }
        catch
        {
        }
    }

    public void StartPresenceHeartbeat()
    {
        StopPresenceHeartbeat();

        _presenceCancellation = new CancellationTokenSource();
        _presenceTimer = new PeriodicTimer(TimeSpan.FromSeconds(45));
        _ = RunPresenceHeartbeatAsync(_presenceTimer, _presenceCancellation.Token);
    }

    public void StopPresenceHeartbeat()
    {
        _presenceCancellation?.Cancel();
        _presenceCancellation?.Dispose();
        _presenceCancellation = null;

        _presenceTimer?.Dispose();
        _presenceTimer = null;
    }

    public async Task UpdateVisitorLocationAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var identity = GetUsageIdentity();

        try
        {
            await _httpClient.PutAsJsonAsync(
                $"{ApiEndpointResolver.UserEndpoint}/visitor-location",
                new
                {
                    UserId = identity.UserId > 0 ? (int?)identity.UserId : null,
                    GuestId = identity.GuestId,
                    Latitude = latitude,
                    Longitude = longitude
                },
                cancellationToken);
        }
        catch
        {
        }
    }

    public void StartAudioHeartbeat(FoodLocation poi)
    {
        StopAudioHeartbeat(sendOffline: false);

        _currentAudioPoiId = poi.Id;
        _audioCancellation = new CancellationTokenSource();
        _audioTimer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        _ = SendAudioHeartbeatAsync(poi.Id, true, _audioCancellation.Token);
        _ = RunAudioHeartbeatAsync(poi.Id, _audioTimer, _audioCancellation.Token);
    }

    public void StopAudioHeartbeat(bool sendOffline = true)
    {
        var poiId = _currentAudioPoiId;

        _audioCancellation?.Cancel();
        _audioCancellation?.Dispose();
        _audioCancellation = null;

        _audioTimer?.Dispose();
        _audioTimer = null;
        _currentAudioPoiId = null;

        if (sendOffline && poiId.HasValue)
        {
            _ = SendAudioHeartbeatAsync(poiId.Value, false);
        }
    }

    public UsageActorIdentity GetUsageIdentity()
    {
        return new UsageActorIdentity
        {
            UserId = 0,
            UserName = "guest",
            Role = "TravelerGuest",
            GuestId = GuestId
        };
    }

    private static string GetOrCreateGuestId()
    {
        var existingGuestId = Preferences.Default.Get(PreferenceGuestId, string.Empty);
        if (!string.IsNullOrWhiteSpace(existingGuestId))
        {
            return existingGuestId;
        }

        var newGuestId = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(PreferenceGuestId, newGuestId);
        return newGuestId;
    }

    private async Task UpdateGuestPresenceAsync(bool isOnline, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsync(
            $"{ApiEndpointResolver.UserEndpoint}/guest-presence?guestId={Uri.EscapeDataString(GuestId)}&isOnline={isOnline.ToString().ToLowerInvariant()}",
            content: null,
            cancellationToken);
    }

    private async Task RunPresenceHeartbeatAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await SetPresenceAsync(true, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunAudioHeartbeatAsync(int poiId, PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await SendAudioHeartbeatAsync(poiId, true, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SendAudioHeartbeatAsync(int poiId, bool isPlaying, CancellationToken cancellationToken = default)
    {
        var identity = GetUsageIdentity();

        try
        {
            await _httpClient.PutAsJsonAsync(
                $"{ApiEndpointResolver.UserEndpoint}/audio-heartbeat",
                new
                {
                    UserId = identity.UserId > 0 ? (int?)identity.UserId : null,
                    GuestId = identity.GuestId,
                    PoiId = poiId,
                    IsPlaying = isPlaying
                },
                cancellationToken);
        }
        catch
        {
        }
    }
}
