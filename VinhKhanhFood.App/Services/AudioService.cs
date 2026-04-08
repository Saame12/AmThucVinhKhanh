using System.Net.Http.Json;

namespace VinhKhanhFood.App.Services
{
    /// <summary>
    /// Service để handle Text-to-Speech
    /// - Nếu có AudioUrl → phát file
    /// - Không có → gọi API TTS → phát Google Translate audio
    /// </summary>
    public class AudioService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AudioService> _logger;
        private const string API_BASE = "http://localhost:5020/api";

        public AudioService()
        {
            _httpClient = new HttpClient();
            _logger = null;
        }

        /// <summary>
        /// Play audio from URL or generate TTS if URL is empty
        /// </summary>
        public async Task PlayAudioAsync(string audioUrl, string fallbackText, string language = "vi")
        {
            try
            {
                // Option 1: If audioUrl exists, play it directly
                if (!string.IsNullOrWhiteSpace(audioUrl))
                {
                    await PlayDirectAudioAsync(audioUrl);
                    return;
                }

                // Option 2: If no audio URL, use TTS
                if (!string.IsNullOrWhiteSpace(fallbackText))
                {
                    await PlayTtsAsync(fallbackText, language);
                    return;
                }

                Debug.WriteLine("No audio URL or fallback text provided");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing audio: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể phát âm thanh", "OK");
            }
        }

        /// <summary>
        /// Play audio from direct URL using MAUI MediaElement
        /// </summary>
        private async Task PlayDirectAudioAsync(string audioUrl)
        {
            try
            {
                // Verify URL is accessible
                var response = await _httpClient.HeadAsync(audioUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Audio URL not accessible: {audioUrl}");
                    throw new Exception("Audio file not found");
                }

                // Use MAUI native speech API if available
                // Or use external audio player
                Debug.WriteLine($"Playing audio from: {audioUrl}");
                
                // For MAUI, you'd typically use a MediaElement control in XAML
                // Or use platform-specific audio player
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing direct audio: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get TTS audio URL from API and play it
        /// Uses Google Translate TTS as fallback
        /// </summary>
        public async Task<string> GetTtsAudioUrlAsync(string text, string language = "vi")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                var ttsUrl = $"{API_BASE}/texttospeech/speak?text={Uri.EscapeDataString(text)}&lang={language}";
                
                var response = await _httpClient.GetAsync(ttsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsAsync<TtsResponse>();
                    return json?.url;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting TTS URL: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Play TTS audio (generated on-the-fly)
        /// </summary>
        private async Task PlayTtsAsync(string text, string language = "vi")
        {
            try
            {
                var audioUrl = await GetTtsAudioUrlAsync(text, language);
                
                if (!string.IsNullOrWhiteSpace(audioUrl))
                {
                    Debug.WriteLine($"Playing TTS audio: {audioUrl}");
                    await PlayDirectAudioAsync(audioUrl);
                }
                else
                {
                    Debug.WriteLine("Failed to get TTS audio URL");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing TTS: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Speak text using MAUI TextToSpeech API
        /// Fallback when audio files are not available
        /// </summary>
        public async Task SpeakTextAsync(string text, string language = "vi")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return;

                var locale = language switch
                {
                    "en" => "en-US",
                    "zh" => "zh-CN",
                    _ => "vi-VN"
                };

                var settings = new SpeechOptions
                {
                    Locale = new Locale(locale),
                    Volume = 1.0f,
                    Pitch = 1.0f
                };

                await TextToSpeech.SpeakAsync(text, settings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error with TextToSpeech: {ex.Message}");
            }
        }

        /// <summary>
        /// Get supported languages from API
        /// </summary>
        public async Task<List<LanguageInfo>> GetSupportedLanguagesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{API_BASE}/texttospeech/languages");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsAsync<LanguageResponse>();
                    return json?.languages?.ToList() ?? new List<LanguageInfo>();
                }
                return new List<LanguageInfo>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting languages: {ex.Message}");
                return new List<LanguageInfo>();
            }
        }
    }

    // Response Models
    public class TtsResponse
    {
        public string url { get; set; }
        public string text { get; set; }
        public string language { get; set; }
        public string message { get; set; }
    }

    public class LanguageResponse
    {
        public LanguageInfo[] languages { get; set; }
    }

    public class LanguageInfo
    {
        public string code { get; set; }
        public string name { get; set; }
        public string nativeName { get; set; }
    }
}
