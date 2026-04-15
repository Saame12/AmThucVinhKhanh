using System.Diagnostics;

namespace VinhKhanhFood.API.Services;

public static class AndroidAdbReverseService
{
    private const int ApiPort = 5020;

    public static async Task TryConnectDevicesAsync(ILogger logger, CancellationToken cancellationToken = default)
    {
        var snapshot = await EnsureReverseAsync(cancellationToken);

        try
        {
            if (!snapshot.AdbAvailable)
            {
                logger.LogInformation("[ADB] adb.exe not found. Skipping adb reverse setup.");
                return;
            }

            if (snapshot.DeviceSerials.Count == 0)
            {
                logger.LogInformation("[ADB] No physical Android devices in 'device' state. Skipping adb reverse setup.");
                return;
            }

            logger.LogInformation("[ADB] Found {Count} physical Android device(s). Configuring tcp:{Port}...", snapshot.DeviceSerials.Count, ApiPort);

            foreach (var serial in snapshot.ReversedSerials)
            {
                logger.LogInformation("[ADB] reverse active for device {Serial} on tcp:{Port}.", serial, ApiPort);
            }

            foreach (var failure in snapshot.Failures)
            {
                logger.LogWarning("[ADB] {Failure}", failure);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[ADB] Unable to configure adb reverse automatically.");
        }
    }

    public static async Task<AdbReverseSnapshot> EnsureReverseAsync(CancellationToken cancellationToken = default)
    {
        var adbPath = ResolveAdbPath();
        if (string.IsNullOrWhiteSpace(adbPath))
        {
            return new AdbReverseSnapshot(false, [], [], ["adb.exe not found"]);
        }

        var devicesOutput = await RunProcessAsync(adbPath, "devices", cancellationToken);
        var deviceSerials = ParsePhysicalDeviceSerials(devicesOutput.StandardOutput);
        if (deviceSerials.Count == 0)
        {
            return new AdbReverseSnapshot(true, [], [], []);
        }

        var reversedSerials = new List<string>();
        var failures = new List<string>();

        foreach (var serial in deviceSerials)
        {
            var reverseResult = await RunProcessAsync(
                adbPath,
                $"-s {serial} reverse tcp:{ApiPort} tcp:{ApiPort}",
                cancellationToken);

            if (reverseResult.ExitCode == 0)
            {
                reversedSerials.Add(serial);
            }
            else
            {
                var error = string.IsNullOrWhiteSpace(reverseResult.StandardError)
                    ? "Unknown error"
                    : reverseResult.StandardError.Trim();
                failures.Add($"reverse failed for device {serial}. ExitCode={reverseResult.ExitCode}. Error={error}");
            }
        }

        return new AdbReverseSnapshot(true, deviceSerials, reversedSerials, failures);
    }

    private static string? ResolveAdbPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var sdkAdb = Path.Combine(localAppData, "Android", "Sdk", "platform-tools", "adb.exe");
        if (File.Exists(sdkAdb))
        {
            return sdkAdb;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (var segment in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var adbPath = Path.Combine(segment, "adb.exe");
            if (File.Exists(adbPath))
            {
                return adbPath;
            }
        }

        return null;
    }

    private static List<string> ParsePhysicalDeviceSerials(string output)
    {
        var devices = new List<string>();
        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("List of devices attached", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            var serial = parts[0];
            var state = parts[1];
            if (!string.Equals(state, "device", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (serial.StartsWith("emulator-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            devices.Add(serial);
        }

        return devices;
    }

    private static async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult(
            process.ExitCode,
            await standardOutputTask,
            await standardErrorTask);
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

    public sealed record AdbReverseSnapshot(
        bool AdbAvailable,
        IReadOnlyList<string> DeviceSerials,
        IReadOnlyList<string> ReversedSerials,
        IReadOnlyList<string> Failures);
}
