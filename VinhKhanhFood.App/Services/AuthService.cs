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

    private readonly HttpClient _httpClient;

    public AuthService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler);
    }

    public event EventHandler<UserSession?>? SessionChanged;

    public UserSession? CurrentSession { get; private set; } = LoadSessionFromPreferences();

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
            return (true, null);
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
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"{LocalizationService.GetString("AuthConnectionError")}: {ex.Message}");
        }
    }

    public void Logout()
    {
        Preferences.Default.Set(PreferenceIsLoggedIn, false);
        Preferences.Default.Remove(PreferenceUserId);
        Preferences.Default.Remove(PreferenceUserName);
        Preferences.Default.Remove(PreferenceUserRole);
        Preferences.Default.Remove(PreferenceUserLogin);
        Preferences.Default.Remove(PreferenceUserStatus);

        CurrentSession = null;
        SessionChanged?.Invoke(this, null);
    }

    private void SaveSession(UserSession session)
    {
        Preferences.Default.Set(PreferenceIsLoggedIn, true);
        Preferences.Default.Set(PreferenceUserId, session.Id);
        Preferences.Default.Set(PreferenceUserName, session.FullName);
        Preferences.Default.Set(PreferenceUserRole, session.Role);
        Preferences.Default.Set(PreferenceUserLogin, session.Username);
        Preferences.Default.Set(PreferenceUserStatus, session.Status);

        CurrentSession = session;
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
            Status = Preferences.Default.Get(PreferenceUserStatus, "Active")
        };
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
