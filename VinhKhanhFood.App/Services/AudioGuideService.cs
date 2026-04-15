using VinhKhanhFood.App.Models;

#if ANDROID
using Android.App;
using AndroidMediaPlayer = Android.Media.MediaPlayer;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using AndroidOperationResult = Android.Speech.Tts.OperationResult;
using AndroidUtteranceProgressListener = Android.Speech.Tts.UtteranceProgressListener;
using AndroidQueueMode = Android.Speech.Tts.QueueMode;
using AndroidLocale = Java.Util.Locale;
using AndroidNetUri = Android.Net.Uri;
#endif

#if WINDOWS
using System.Speech.Synthesis;
using Windows.Foundation;
using WinMediaSource = Windows.Media.Core.MediaSource;
using WinMediaPlayer = Windows.Media.Playback.MediaPlayer;
#endif

namespace VinhKhanhFood.App.Services;

public enum AudioGuidePlaybackState
{
    Idle,
    Playing
}

public sealed class AudioGuideStateChangedEventArgs : EventArgs
{
    public AudioGuidePlaybackState State { get; init; }
    public FoodLocation? CurrentPoi { get; init; }
}

public sealed class AudioGuideService
{
    private readonly SemaphoreSlim _playbackLock = new(1, 1);

    private CancellationTokenSource? _playbackCts;
    private FoodLocation? _currentPoi;
    private AudioGuidePlaybackState _state = AudioGuidePlaybackState.Idle;

#if WINDOWS
    private WinMediaPlayer? _windowsMediaPlayer;
#endif

#if ANDROID
    private AndroidMediaPlayer? _androidMediaPlayer;
#endif

    public event EventHandler<AudioGuideStateChangedEventArgs>? StateChanged;

    public AudioGuidePlaybackState State => _state;
    public FoodLocation? CurrentPoi => _currentPoi;
    public bool IsPlaying => State == AudioGuidePlaybackState.Playing;

    public Task<bool> AutoPlayPoiAsync(FoodLocation poi, CancellationToken cancellationToken = default)
    {
        if (CurrentPoi?.Id == poi.Id && IsPlaying)
        {
            return Task.FromResult(false);
        }

        return PlayPoiAsync(poi, cancellationToken);
    }

    public async Task<bool> PlayPoiAsync(FoodLocation poi, CancellationToken cancellationToken = default)
    {
        var narration = poi.DisplayDescription?.Trim();
        if (string.IsNullOrWhiteSpace(narration))
        {
            return false;
        }

        await StartPlaybackAsync(
            poi,
            token => RunNarrationPlaybackAsync(narration, poi, token),
            cancellationToken);

        return true;
    }

    public async Task<bool> PlayProfessionalAudioAsync(FoodLocation poi, CancellationToken cancellationToken = default)
    {
        var audioUrl = poi.DisplayAudioUrl?.Trim();
        if (string.IsNullOrWhiteSpace(audioUrl))
        {
            return false;
        }

        var resolvedAudioUrl = ApiEndpointResolver.ResolveAssetUrl(audioUrl);
        await StartPlaybackAsync(
            poi,
            token => RunProfessionalPlaybackAsync(resolvedAudioUrl, poi, token),
            cancellationToken);

        return true;
    }

    public void Cancel()
    {
        StopCore();
        PublishState();
    }

    private async Task StartPlaybackAsync(FoodLocation poi, Func<CancellationToken, Task> playbackAction, CancellationToken cancellationToken)
    {
        await _playbackLock.WaitAsync(cancellationToken);
        try
        {
            StopCore();

            _currentPoi = poi;
            _state = AudioGuidePlaybackState.Playing;
            _playbackCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            PublishState();

            _ = playbackAction(_playbackCts.Token);
        }
        finally
        {
            _playbackLock.Release();
        }
    }

    private async Task RunNarrationPlaybackAsync(string narration, FoodLocation poi, CancellationToken cancellationToken)
    {
        try
        {
            await SpeakNarrationAsync(narration, cancellationToken);
            CompletePlaybackForPoi(poi.Id);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            StopCore();
            PublishState();
        }
    }

    private async Task RunProfessionalPlaybackAsync(string audioUrl, FoodLocation poi, CancellationToken cancellationToken)
    {
        try
        {
            await PlayProfessionalAudioCoreAsync(audioUrl, cancellationToken);
            CompletePlaybackForPoi(poi.Id);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            StopCore();
            PublishState();
        }
    }

    private void CompletePlaybackForPoi(int poiId)
    {
        if (_playbackCts?.IsCancellationRequested == true)
        {
            return;
        }

        if (CurrentPoi?.Id != poiId)
        {
            return;
        }

        StopCore();
        PublishState();
    }

    private async Task SpeakNarrationAsync(string narration, CancellationToken cancellationToken)
    {
#if ANDROID
        await SpeakWithAndroidTtsAsync(narration, cancellationToken);
#elif WINDOWS
        await SpeakWithWindowsTtsAsync(narration, cancellationToken);
#else
        var locale = await ResolveLocaleAsync();
        var options = new SpeechOptions
        {
            Locale = locale,
            Volume = 1f,
            Pitch = 1f
        };

        await Microsoft.Maui.Media.TextToSpeech.Default.SpeakAsync(narration, options, cancellationToken);
#endif
    }

    private async Task PlayProfessionalAudioCoreAsync(string audioUrl, CancellationToken cancellationToken)
    {
#if ANDROID
        await PlayWithAndroidMediaAsync(audioUrl, cancellationToken);
#elif WINDOWS
        await PlayWithWindowsMediaAsync(audioUrl, cancellationToken);
#else
        throw new NotSupportedException("Professional audio playback is not configured on this platform.");
#endif
    }

#if WINDOWS
    private static async Task SpeakWithWindowsTtsAsync(string narration, CancellationToken cancellationToken)
    {
        using var synthesizer = new SpeechSynthesizer();
        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                synthesizer.SpeakAsyncCancelAll();
            }
            catch
            {
            }
        });

        ApplyWindowsVoice(synthesizer);

        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        EventHandler<SpeakCompletedEventArgs>? handler = null;
        handler = (_, args) =>
        {
            if (args.Cancelled)
            {
                completion.TrySetCanceled(cancellationToken);
            }
            else if (args.Error is not null)
            {
                completion.TrySetException(args.Error);
            }
            else
            {
                completion.TrySetResult(true);
            }
        };

        synthesizer.SpeakCompleted += handler;
        try
        {
            synthesizer.SpeakAsync(narration);
            await completion.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            synthesizer.SpeakCompleted -= handler;
        }
    }

    private static void ApplyWindowsVoice(SpeechSynthesizer synthesizer)
    {
        var keyword = LocalizationService.CurrentLanguage switch
        {
            "en" => "en-",
            "zh" => "zh-",
            _ => "vi-"
        };

        var voice = synthesizer.GetInstalledVoices()
            .Select(item => item.VoiceInfo)
            .FirstOrDefault(info => info.Culture.Name.StartsWith(keyword, StringComparison.OrdinalIgnoreCase));

        if (voice is not null)
        {
            synthesizer.SelectVoice(voice.Name);
        }
    }

    private async Task PlayWithWindowsMediaAsync(string audioUrl, CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaPlayer = new WinMediaPlayer();
        _windowsMediaPlayer = mediaPlayer;

        TypedEventHandler<WinMediaPlayer, object>? endedHandler = null;
        TypedEventHandler<WinMediaPlayer, Windows.Media.Playback.MediaPlayerFailedEventArgs>? failedHandler = null;

        endedHandler = (_, _) => completion.TrySetResult(true);
        failedHandler = (_, args) => completion.TrySetException(new InvalidOperationException(args.ErrorMessage));

        mediaPlayer.MediaEnded += endedHandler;
        mediaPlayer.MediaFailed += failedHandler;
        mediaPlayer.Source = WinMediaSource.CreateFromUri(new Uri(audioUrl));

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                mediaPlayer.Pause();
                mediaPlayer.Source = null;
            }
            catch
            {
            }

            completion.TrySetCanceled(cancellationToken);
        });

        try
        {
            mediaPlayer.Play();
            await completion.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            mediaPlayer.MediaEnded -= endedHandler;
            mediaPlayer.MediaFailed -= failedHandler;

            try
            {
                mediaPlayer.Source = null;
                mediaPlayer.Dispose();
            }
            catch
            {
            }

            if (ReferenceEquals(_windowsMediaPlayer, mediaPlayer))
            {
                _windowsMediaPlayer = null;
            }
        }
    }
#endif

#if ANDROID
    private static async Task SpeakWithAndroidTtsAsync(string narration, CancellationToken cancellationToken)
    {
        var context = Android.App.Application.Context;
        var initSource = new TaskCompletionSource<bool>();
        var utteranceSource = new TaskCompletionSource<bool>();
        AndroidTextToSpeech? tts = null;

        try
        {
            tts = new AndroidTextToSpeech(context, new InitListener(status =>
            {
                initSource.TrySetResult(status == AndroidOperationResult.Success);
            }));

            if (!await initSource.Task.WaitAsync(cancellationToken))
            {
                throw new InvalidOperationException("Android TTS initialization failed.");
            }

            tts.SetLanguage(BuildAndroidLocale());
            tts.SetPitch(1f);

            var utteranceId = Guid.NewGuid().ToString("N");
            tts.SetOnUtteranceProgressListener(new ProgressListener(utteranceSource));

            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                try
                {
                    tts.Stop();
                }
                catch
                {
                }

                utteranceSource.TrySetCanceled(cancellationToken);
            });

            tts.Speak(narration, AndroidQueueMode.Flush, null, utteranceId);
            await utteranceSource.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            tts?.Stop();
            tts?.Shutdown();
            tts?.Dispose();
        }
    }

    private async Task PlayWithAndroidMediaAsync(string audioUrl, CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaPlayer = new AndroidMediaPlayer();
        _androidMediaPlayer = mediaPlayer;

        EventHandler? preparedHandler = null;
        EventHandler? completedHandler = null;
        EventHandler<Android.Media.MediaPlayer.ErrorEventArgs>? errorHandler = null;

        preparedHandler = (_, _) => mediaPlayer.Start();
        completedHandler = (_, _) => completion.TrySetResult(true);
        errorHandler = (_, args) =>
        {
            args.Handled = true;
            completion.TrySetException(new InvalidOperationException("Android professional audio playback failed."));
        };

        mediaPlayer.Prepared += preparedHandler;
        mediaPlayer.Completion += completedHandler;
        mediaPlayer.Error += errorHandler;

        using var cancellationRegistration = cancellationToken.Register(() =>
        {
            try
            {
                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Stop();
                }
            }
            catch
            {
            }

            completion.TrySetCanceled(cancellationToken);
        });

        try
        {
            var mediaUri = AndroidNetUri.Parse(audioUrl) ?? throw new InvalidOperationException("Android audio URL is invalid.");
            mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);
            mediaPlayer.SetDataSource(Android.App.Application.Context, mediaUri);
            mediaPlayer.PrepareAsync();
            await completion.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            mediaPlayer.Prepared -= preparedHandler;
            mediaPlayer.Completion -= completedHandler;
            mediaPlayer.Error -= errorHandler;

            try
            {
                mediaPlayer.Reset();
                mediaPlayer.Release();
                mediaPlayer.Dispose();
            }
            catch
            {
            }

            if (ReferenceEquals(_androidMediaPlayer, mediaPlayer))
            {
                _androidMediaPlayer = null;
            }
        }
    }

    private static AndroidLocale BuildAndroidLocale() => LocalizationService.CurrentLanguage switch
    {
        "en" => new AndroidLocale("en", "US"),
        "zh" => new AndroidLocale("zh", "CN"),
        _ => new AndroidLocale("vi", "VN")
    };

    private sealed class InitListener : Java.Lang.Object, AndroidTextToSpeech.IOnInitListener
    {
        private readonly Action<AndroidOperationResult> _onInit;

        public InitListener(Action<AndroidOperationResult> onInit)
        {
            _onInit = onInit;
        }

        public void OnInit(AndroidOperationResult status)
        {
            _onInit(status);
        }
    }

    private sealed class ProgressListener : AndroidUtteranceProgressListener
    {
        private readonly TaskCompletionSource<bool> _completion;

        public ProgressListener(TaskCompletionSource<bool> completion)
        {
            _completion = completion;
        }

        public override void OnStart(string? utteranceId)
        {
        }

        public override void OnDone(string? utteranceId)
        {
            _completion.TrySetResult(true);
        }

        [Obsolete]
        public override void OnError(string? utteranceId)
        {
            _completion.TrySetException(new InvalidOperationException("Android TTS playback failed."));
        }
    }
#endif

    private async Task<Microsoft.Maui.Media.Locale?> ResolveLocaleAsync()
    {
        var locales = await Microsoft.Maui.Media.TextToSpeech.Default.GetLocalesAsync();
        return locales.FirstOrDefault(locale =>
            locale.Language.StartsWith(LocalizationService.CurrentLanguage, StringComparison.OrdinalIgnoreCase));
    }

    private void StopCore()
    {
        _playbackCts?.Cancel();
        _playbackCts?.Dispose();
        _playbackCts = null;

#if WINDOWS
        if (_windowsMediaPlayer is not null)
        {
            try
            {
                _windowsMediaPlayer.Pause();
                _windowsMediaPlayer.Source = null;
                _windowsMediaPlayer.Dispose();
            }
            catch
            {
            }

            _windowsMediaPlayer = null;
        }
#endif

#if ANDROID
        if (_androidMediaPlayer is not null)
        {
            try
            {
                if (_androidMediaPlayer.IsPlaying)
                {
                    _androidMediaPlayer.Stop();
                }

                _androidMediaPlayer.Reset();
                _androidMediaPlayer.Release();
                _androidMediaPlayer.Dispose();
            }
            catch
            {
            }

            _androidMediaPlayer = null;
        }
#endif

        _currentPoi = null;
        _state = AudioGuidePlaybackState.Idle;
    }

    private void PublishState()
    {
        StateChanged?.Invoke(this, new AudioGuideStateChangedEventArgs
        {
            State = _state,
            CurrentPoi = _currentPoi
        });
    }
}
