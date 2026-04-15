param(
    [string]$DeviceId = "",
    [int]$Port = 5020,
    [string]$ApiUrl = "http://localhost:5020",
    [switch]$Watch,
    [switch]$StartApi
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param(
        [string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::Cyan
    )

    Write-Host ("[{0}] {1}" -f (Get-Date -Format "HH:mm:ss"), $Message) -ForegroundColor $Color
}

function Resolve-AdbPath {
    $sdkAdb = Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"
    if (Test-Path $sdkAdb) {
        return $sdkAdb
    }

    $adbCommand = Get-Command adb -ErrorAction SilentlyContinue
    if ($adbCommand) {
        return $adbCommand.Source
    }

    throw "Khong tim thay adb.exe. Hay cai Android SDK Platform-Tools hoac mo Visual Studio Android workload."
}

function Test-ApiReady {
    param([string]$Url)

    try {
        $null = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 2
        return $true
    }
    catch {
        return $false
    }
}

function Get-ConnectedAndroidDevice {
    param(
        [string]$AdbPath,
        [string]$PreferredDeviceId
    )

    $lines = & $AdbPath devices | Select-Object -Skip 1
    $devices = foreach ($line in $lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $parts = ($line -split "\s+") | Where-Object { $_ }
        if ($parts.Count -lt 2) {
            continue
        }

        [pscustomobject]@{
            Serial = $parts[0]
            State  = $parts[1]
        }
    }

    if ($PreferredDeviceId) {
        return $devices | Where-Object { $_.Serial -eq $PreferredDeviceId -and $_.State -eq "device" } | Select-Object -First 1
    }

    return $devices |
        Where-Object { $_.State -eq "device" -and $_.Serial -notlike "emulator-*" } |
        Select-Object -First 1
}

function Ensure-ReversePort {
    param(
        [string]$AdbPath,
        [string]$Serial,
        [int]$MappedPort
    )

    & $AdbPath -s $Serial reverse "tcp:$MappedPort" "tcp:$MappedPort" | Out-Null
}

function Start-ApiProject {
    param([string]$RepoRoot)

    $apiProject = Join-Path $RepoRoot "VinhKhanhFood.API\VinhKhanhFood.API.csproj"
    if (-not (Test-Path $apiProject)) {
        throw "Khong tim thay project API tai $apiProject"
    }

    $command = "dotnet run --project `"$apiProject`" --launch-profile http"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command | Out-Null
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$adbPath = Resolve-AdbPath
$currentBoundDevice = ""
$lastApiReady = $false
$shownWaitingDevice = $false
$shownWaitingApi = $false

Write-Status "ADB: $adbPath"
Write-Status "API target: $ApiUrl"

if ($StartApi -and -not (Test-ApiReady -Url $ApiUrl)) {
    Write-Status "API chua chay. Dang mo API bang dotnet run..." Yellow
    Start-ApiProject -RepoRoot $repoRoot
}

do {
    $device = Get-ConnectedAndroidDevice -AdbPath $adbPath -PreferredDeviceId $DeviceId
    if (-not $device) {
        if (-not $shownWaitingDevice) {
            Write-Status "Chua thay dien thoai Android that o trang thai 'device'. Hay cam USB, bat USB debugging, roi Allow." Yellow
            $shownWaitingDevice = $true
        }

        Start-Sleep -Seconds 2
        continue
    }

    if ($shownWaitingDevice -or $currentBoundDevice -ne $device.Serial) {
        Write-Status "Da nhan dien thoai: $($device.Serial)" Green
        $shownWaitingDevice = $false
    }

    $apiReady = Test-ApiReady -Url $ApiUrl
    if (-not $apiReady) {
        if (-not $shownWaitingApi) {
            Write-Status "API chua san sang o $ApiUrl. Hay bam Run API trong Visual Studio hoac dung -StartApi." Yellow
            $shownWaitingApi = $true
        }

        Start-Sleep -Seconds 2
        continue
    }

    if ($shownWaitingApi -or -not $lastApiReady) {
        Write-Status "API da len. Dang noi adb reverse..." Green
        $shownWaitingApi = $false
    }

    Ensure-ReversePort -AdbPath $adbPath -Serial $device.Serial -MappedPort $Port

    if ($currentBoundDevice -ne $device.Serial -or -not $lastApiReady) {
        Write-Status "Da map dien thoai $($device.Serial) -> 127.0.0.1:$Port. App tren may that se goi duoc API local." Green
        Write-Status "Ban co the mo app tren dien thoai ngay bay gio." Green
    }

    $currentBoundDevice = $device.Serial
    $lastApiReady = $true

    if (-not $Watch) {
        break
    }

    Start-Sleep -Seconds 3
}
while ($true)
