using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VinhKhanhFood.App.Models;

namespace VinhKhanhFood.App.Services;

public sealed class AuthService
{
    private const string PreferenceIsLoggedIn = "IsLoggedIn";
    private const string PreferenceUserId = "UserId";
    private const string PreferenceUserName = "UserName";
    private const string PreferenceUserRole = "UserRole";
    private const string PreferenceUserLogin = "UserLogin";
    private const string PreferenceUserStatus = "UserStatus";
    private const string PreferenceOnlineStatus = "UserOnlineStatus";
    private const string PreferenceIsVip = "UserIsVip";

    private readonly HttpClient _httpClient;
    private PeriodicTimer? _presenceTimer;
    private CancellationTokenSource? _presenceCancellation;

    public AuthService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler);
        CurrentSession = LoadSessionFromPreferences();

        if (CurrentSession is not null)
        {
            StartPresenceHeartbeat();
            _ = SetPresenceAsync(true);
        }
    }

    public event EventHandler<UserSession?>? SessionChanged;

    public UserSession? CurrentSession { get; private set; }

    public bool IsLoggedIn => CurrentSession is not null;

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, LocalizationService.GetString("AuthValidationRequired"));
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpointResolver.UserEndpoint}/login", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorAsync(response, cancellationToken));
            }

            var session = await response.Content.ReadFromJsonAsync<UserSession>(cancellationToken: cancellationToken);
            if (session is null)
            {
                return (false, LocalizationService.GetString("AuthUnexpectedResponse"));
            }

            SaveSession(session);
            await SetPresenceAsync(true, cancellationToken);
            return (true, null);
        }
        catch (HttpRequestException)
        {
            return (false, ApiHealthService.GetServerUnavailableMessage());
        }
        catch (Exception ex)
        {
            return (false, $"{LocalizationService.GetString("AuthConnectionError")}: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, LocalizationService.GetString("AuthValidationRequired"));
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpointResolver.UserEndpoint}/register", request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return (false, LocalizationService.GetString("AuthUsernameExists"));
            }

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorAsync(response, cancellationToken));
            }

            var session = await response.Content.ReadFromJsonAsync<UserSession>(cancellationToken: cancellationToken);
            if (session is null)
            {
                return (false, LocalizationService.GetString("AuthUnexpectedResponse"));
            }

            SaveSession(session);
            await SetPresenceAsync(true, cancellationToken);
            return (true, null);
        }
        catch (HttpRequestException)
        {
            return (false, ApiHealthService.GetServerUnavailableMessage());
        }
        catch (Exception ex)
        {
            return (false, $"{LocalizationService.GetString("AuthConnectionError")}: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> PurchaseVipAsync(CancellationToken cancellationToken = default)
    {
        var session = CurrentSession;
        if (session is null || session.Id <= 0)
        {
            return (false, LocalizationService.GetString("LoginToYourAccount"));
        }

        try
        {
            var response = await _httpClient.PostAsync(
                $"{ApiEndpointResolver.UserEndpoint}/vip/purchase/{session.Id}",
                content: null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorAsync(response, cancellationToken));
            }

            var updatedSession = await response.Content.ReadFromJsonAsync<UserSession>(cancellationToken: cancellationToken);
            if (updatedSession is null)
            {
                return (false, LocalizationService.GetString("AuthUnexpectedResponse"));
            }

            updatedSession.OnlineStatus = session.OnlineStatus;
            SaveSession(updatedSession);
            return (true, null);
        }
        catch (HttpRequestException)
        {
            return (false, ApiHealthService.GetServerUnavailableMessage());
        }
        catch (Exception ex)
        {
            return (false, $"{LocalizationService.GetString("AuthConnectionError")}: {ex.Message}");
        }
    }

    public void Logout()
    {
        _ = SetPresenceAsync(false);
        StopPresenceHeartbeat();

        Preferences.Default.Set(PreferenceIsLoggedIn, false);
        Preferences.Default.Remove(PreferenceUserId);
        Preferences.Default.Remove(PreferenceUserName);
        Preferences.Default.Remove(PreferenceUserRole);
        Preferences.Default.Remove(PreferenceUserLogin);
        Preferences.Default.Remove(PreferenceUserStatus);
        Preferences.Default.Remove(PreferenceOnlineStatus);
        Preferences.Default.Remove(PreferenceIsVip);

        CurrentSession = null;
        SessionChanged?.Invoke(this, null);
    }

    public async Task SetPresenceAsync(bool isOnline, CancellationToken cancellationToken = default)
    {
        var session = CurrentSession;
        if (session is null || session.Id <= 0)
        {
            return;
        }

        try
        {
            using var response = await _httpClient.PutAsync(
                $"{ApiEndpointResolver.UserEndpoint}/presence/{session.Id}?isOnline={isOnline.ToString().ToLowerInvariant()}",
                content: null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            session.OnlineStatus = isOnline ? "Online" : "Offline";
            Preferences.Default.Set(PreferenceOnlineStatus, session.OnlineStatus);

            if (ReferenceEquals(CurrentSession, session))
            {
                SessionChanged?.Invoke(this, session);
            }
        }
        catch
        {
            // Presence should never break the main auth flow.
        }
    }

    public void StartPresenceHeartbeat()
    {
        StopPresenceHeartbeat();

        if (CurrentSession is null)
        {
            return;
        }

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

    private void SaveSession(UserSession session)
    {
        session.OnlineStatus = "Online";

        Preferences.Default.Set(PreferenceIsLoggedIn, true);
        Preferences.Default.Set(PreferenceUserId, session.Id);
        Preferences.Default.Set(PreferenceUserName, session.FullName);
        Preferences.Default.Set(PreferenceUserRole, session.Role);
        Preferences.Default.Set(PreferenceUserLogin, session.Username);
        Preferences.Default.Set(PreferenceUserStatus, session.Status);
        Preferences.Default.Set(PreferenceOnlineStatus, session.OnlineStatus);
        Preferences.Default.Set(PreferenceIsVip, session.IsVip);

        CurrentSession = session;
        StartPresenceHeartbeat();
        SessionChanged?.Invoke(this, session);
    }

    private static UserSession? LoadSessionFromPreferences()
    {
        if (!Preferences.Default.Get(PreferenceIsLoggedIn, false))
        {
            return null;
        }

        return new UserSession
        {
            Id = Preferences.Default.Get(PreferenceUserId, 0),
            FullName = Preferences.Default.Get(PreferenceUserName, string.Empty),
            Role = Preferences.Default.Get(PreferenceUserRole, "User"),
            Username = Preferences.Default.Get(PreferenceUserLogin, string.Empty),
            Status = Preferences.Default.Get(PreferenceUserStatus, "Active"),
            OnlineStatus = Preferences.Default.Get(PreferenceOnlineStatus, "Offline"),
            IsVip = Preferences.Default.Get(PreferenceIsVip, false)
        };
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

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return LocalizationService.GetString("AuthRequestFailed");
            }

            if (payload.StartsWith("{", StringComparison.Ordinal))
            {
                using var json = JsonDocument.Parse(payload);
                if (json.RootElement.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    return messageElement.GetString() ?? LocalizationService.GetString("AuthRequestFailed");
                }
            }

            return payload.Trim('"');
        }
        catch
        {
            return LocalizationService.GetString("AuthRequestFailed");
        }
    }
}
