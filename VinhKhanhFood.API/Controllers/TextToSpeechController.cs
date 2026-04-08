using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace VinhKhanhFood.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TextToSpeechController : ControllerBase
    {
        private readonly ILogger<TextToSpeechController> _logger;

        public TextToSpeechController(ILogger<TextToSpeechController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Convert text to speech using Google Translate TTS (free service)
        /// Returns audio stream URL that can be played
        /// </summary>
        [HttpGet("speak")]
        public IActionResult Speak([FromQuery] string text, [FromQuery] string lang = "vi")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return BadRequest("Text cannot be empty");

                // Encode text for Google Translate TTS
                var encodedText = Uri.EscapeDataString(text);
                
                // Language codes: vi (Vietnamese), en (English), zh-CN (Chinese)
                var languageCode = lang switch
                {
                    "en" => "en",
                    "zh" => "zh-CN",
                    _ => "vi"
                };

                // Google Translate TTS URL (free service)
                var ttsUrl = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q={encodedText}&tl={languageCode}";

                // Return URL that app can use to stream audio
                return Ok(new
                {
                    url = ttsUrl,
                    text = text,
                    language = languageCode,
                    message = "Use this URL in your player to play audio"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating TTS");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Alternative: Generate TTS using free service and return base64 (if needed for offline)
        /// This is for app that wants to download audio locally
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateTts([FromBody] TtsRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                    return BadRequest("Text cannot be empty");

                // For now, just return the URL approach (simplest)
                // If you want to generate MP3, you'd use external service like Azure Cognitive Services
                
                var encodedText = Uri.EscapeDataString(request.Text);
                var lang = request.Language ?? "vi";

                var languageCode = lang switch
                {
                    "en" => "en",
                    "zh" => "zh-CN",
                    _ => "vi"
                };

                var ttsUrl = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q={encodedText}&tl={languageCode}";

                return Ok(new
                {
                    success = true,
                    audioUrl = ttsUrl,
                    originalText = request.Text,
                    language = languageCode,
                    instruction = "Use this URL in AudioPlayer to play"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating TTS");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get list of supported languages
        /// </summary>
        [HttpGet("languages")]
        public IActionResult GetSupportedLanguages()
        {
            return Ok(new
            {
                languages = new[]
                {
                    new { code = "vi", name = "Tiếng Việt", nativeName = "Tiếng Việt" },
                    new { code = "en", name = "English", nativeName = "English" },
                    new { code = "zh", name = "Chinese", nativeName = "中文" }
                }
            });
        }
    }

    // Request model
    public class TtsRequest
    {
        public string Text { get; set; }
        public string Language { get; set; } = "vi";
    }
}
