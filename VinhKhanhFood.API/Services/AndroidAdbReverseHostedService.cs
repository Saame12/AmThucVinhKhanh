using Microsoft.Extensions.Hosting;

namespace VinhKhanhFood.API.Services;

public sealed class AndroidAdbReverseHostedService(
    ILogger<AndroidAdbReverseHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private string? _lastStateKey;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var snapshot = await AndroidAdbReverseService.EnsureReverseAsync(stoppingToken);
                var currentStateKey = BuildStateKey(snapshot);

                if (!string.Equals(_lastStateKey, currentStateKey, StringComparison.Ordinal))
                {
                    LogSnapshot(snapshot);
                    _lastStateKey = currentStateKey;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ADB] Background reconnect check failed.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private void LogSnapshot(AndroidAdbReverseService.AdbReverseSnapshot snapshot)
    {
        if (!snapshot.AdbAvailable)
        {
            logger.LogInformation("[ADB] adb.exe not found. Auto reconnect is unavailable.");
            return;
        }

        if (snapshot.DeviceSerials.Count == 0)
        {
            logger.LogInformation("[ADB] Waiting for a physical Android device...");
            return;
        }

        if (snapshot.ReversedSerials.Count > 0)
        {
            logger.LogInformation(
                "[ADB] Auto reconnect ready for device(s): {Devices} on tcp:5020.",
                string.Join(", ", snapshot.ReversedSerials));
        }

        foreach (var failure in snapshot.Failures)
        {
            logger.LogWarning("[ADB] {Failure}", failure);
        }
    }

    private static string BuildStateKey(AndroidAdbReverseService.AdbReverseSnapshot snapshot)
    {
        var devices = string.Join(",", snapshot.DeviceSerials.OrderBy(x => x, StringComparer.Ordinal));
        var reversed = string.Join(",", snapshot.ReversedSerials.OrderBy(x => x, StringComparer.Ordinal));
        var failures = string.Join("|", snapshot.Failures.OrderBy(x => x, StringComparer.Ordinal));
        return $"{snapshot.AdbAvailable}:{devices}:{reversed}:{failures}";
    }
}
