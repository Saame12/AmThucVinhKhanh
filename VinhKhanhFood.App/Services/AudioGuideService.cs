using VinhKhanhFood.App.Models;

#if ANDROID
using Android.App;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using AndroidOperationResult = Android.Speech.Tts.OperationResult;
using AndroidUtteranceProgressListener = Android.Speech.Tts.UtteranceProgressListener;
using AndroidQueueMode = Android.Speech.Tts.QueueMode;
using AndroidLocale = Java.Util.Locale;
#endif

#if WINDOWS
using System.Speech.Synthesis;
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

    public event EventHandler<AudioGuideStateChangedEventArgs>? StateChanged;

    public AudioGuidePlaybackState State => _state;
    public FoodLocation? CurrentPoi => _currentPoi;
    public bool IsPlaying => State == AudioGuidePlaybackState.Playing;

    public async Task<bool> PlayPoiAsync(FoodLocation poi, CancellationToken cancellationToken = default)
    {
        var narration = poi.DisplayDescription?.Trim();
        if (string.IsNullOrWhiteSpace(narration))
        {
            return false;
        }

        await _playbackLock.WaitAsync(cancellationToken);
        try
        {
            StopCore();

            _currentPoi = poi;
            _state = AudioGuidePlaybackState.Playing;
            _playbackCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            PublishState();

            _ = RunPlaybackAsync(narration, poi, _playbackCts.Token);
            return true;
        }
        finally
        {
            _playbackLock.Release();
        }
    }

    public void Cancel()
    {
        StopCore();
        PublishState();
    }

    public async Task<bool> AutoPlayPoiAsync(FoodLocation poi, CancellationToken cancellationToken = default)
    {
        if (CurrentPoi?.Id == poi.Id && IsPlaying)
        {
            return false;
        }

        return await PlayPoiAsync(poi, cancellationToken);
    }

    private async Task RunPlaybackAsync(string narration, FoodLocation poi, CancellationToken cancellationToken)
    {
        try
        {
            await SpeakNarrationAsync(narration, cancellationToken);

            if (!cancellationToken.IsCancellationRequested && CurrentPoi?.Id == poi.Id)
            {
                StopCore();
                PublishState();
            }
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
