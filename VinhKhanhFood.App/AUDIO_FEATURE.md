# 🎙️ Audio Feature - VinhKhanh Food App (MAUI)

## 📱 Tính Năng Auto Text-to-Speech

App MAUI giờ có tính năng **tự động phát âm thanh**:

### 🔊 Logic Phát Âm Thanh

**Khi người dùng nhấp "Nghe":**

1. **Priority 1**: Nếu có `AudioUrl` (file audio từ owner)
   - → Phát file âm thanh trực tiếp
   
2. **Priority 2**: Không có `AudioUrl` nhưng có text (mô tả)
   - → Gọi API `/api/texttospeech/speak`
   - → Lấy Google Translate TTS URL
   - → Phát âm thanh TTS
   
3. **Priority 3**: Lỗi kết nối hoặc không có TTS
   - → Fallback: Dùng MAUI `TextToSpeech` API (native speech)

---

## 🏗️ Architecture

```
AudioService (Core Service)
├── PlayAudioAsync() - Play từ URL hoặc TTS
├── GetTtsAudioUrlAsync() - Call API để get TTS URL
├── SpeakTextAsync() - MAUI TextToSpeech fallback
└── PlayDirectAudioAsync() - Play file audio

DetailViewModel
├── FoodLocation (Data)
├── PlayAudioCommand → PlayAudioAsync()
└── SpeakCommand → SpeakAsync()

DetailPage.xaml.cs
└── OnPlayAudioClicked() → ViewModel.PlayAudioCommand
```

---

## 📋 Các File Tạo/Sửa

### ✅ Tạo Mới

- `VinhKhanhFood.App/Services/AudioService.cs`
  - Handle tất cả logic phát âm thanh
  - Support 3 ngôn ngữ (VI, EN, ZH)
  - Tự động fallback nếu lỗi

- `VinhKhanhFood.App/ViewModels/DetailViewModel.cs`
  - Binding cho detail page
  - Commands: PlayAudioCommand, SpeakCommand
  - Load FoodLocation từ API

### ✏️ Sửa

- `VinhKhanhFood.App/MauiProgram.cs`
  - Register `AudioService`
  - Register `DetailViewModel`

- `VinhKhanhFood.App/Models/FoodLocation.cs`
  - Thêm `DisplayText` (fallback text cho TTS)
  - Thêm `LanguageCode` (VI, EN, ZH)

- `VinhKhanhFood.App/DetailPage.xaml.cs`
  - Thêm `OnPlayAudioClicked()` handler
  - Inject `AudioService`

---

## 🔌 API Endpoints Used

```
GET /api/texttospeech/speak?text=...&lang=vi
├── Response: { url, text, language }
├── URL: https://translate.google.com/translate_tts?...
└── Format: Audio stream (mp3)

GET /api/texttospeech/languages
├── Response: [{ code, name, nativeName }]
└── Support: VI, EN, ZH
```

---

## 💻 Code Example

### DetailPage.xaml (Button)
```xaml
<Button
    Text="🔊 Nghe Mô Tả"
    Clicked="OnPlayAudioClicked"
    IsEnabled="{Binding IsPlayingAudio, Converter={StaticResource InvertedBoolConverter}}"
/>
```

### DetailPage.xaml.cs
```csharp
private async void OnPlayAudioClicked(object sender, EventArgs e)
{
    try
    {
        // AudioService tự động xử lý:
        // 1. Nếu có audio file → phát file
        // 2. Không có → Get TTS URL từ API
        // 3. Lỗi → Fallback TextToSpeech
        await _audioService.PlayAudioAsync(
            _currentLocation.DisplayAudioUrl,
            _currentLocation.DisplayText,
            _currentLocation.LanguageCode
        );
    }
    catch (Exception ex)
    {
        await DisplayAlert("Lỗi", $"Error: {ex.Message}", "OK");
    }
}
```

---

## 🌐 Language Support

| Language | Code | Example |
|----------|------|---------|
| Tiếng Việt | `vi` | Phở Vĩnh Khánh |
| English | `en` | Vinh Khanh Pho |
| 中文 | `zh` | 文昌粉 |

**Tự động chọn ngôn ngữ** dựa vào `App.CurrentLanguage`

---

## 🎵 Media Format Support

- **Direct Audio**: MP3, WAV, AAC (từ `AudioUrl`)
- **TTS Audio**: MP3 (từ Google Translate API)
- **Native Speech**: Dùng OS TextToSpeech (fallback)

---

## ⚙️ Configuration

### API Configuration
```csharp
const string API_BASE = "http://localhost:5020/api";
// Adjust port & host trong AudioService.cs nếu cần
```

### Speech Settings
```csharp
var settings = new SpeechOptions
{
    Locale = new Locale("vi-VN"),
    Volume = 1.0f,    // 0-1
    Pitch = 1.0f      // 0.5-2
};
```

---

## 🧪 Testing Scenarios

### Scenario 1: Audio URL có sẵn ✅
```
1. Owner thêm quán với AudioUrl
2. App load quán → DisplayAudioUrl ≠ null
3. Nhấp "Nghe" → Phát file audio trực tiếp
```

### Scenario 2: Không có Audio URL (TTS) ✅
```
1. Owner thêm quán KHÔNG có AudioUrl
2. App load quán → DisplayAudioUrl = null, DisplayText ≠ null
3. Nhấp "Nghe" → Call API TTS
4. API return URL → Phát audio TTS
```

### Scenario 3: Lỗi API (Fallback) ✅
```
1. API down hoặc lỗi
2. AudioService catch exception
3. Fallback → Dùng MAUI TextToSpeech
4. Native OS đọc text (Vietnamese voice)
```

---

## 📊 Flow Diagram

```
User Clicks "Play"
    ↓
OnPlayAudioClicked()
    ↓
AudioService.PlayAudioAsync()
    ↓
    ├─→ DisplayAudioUrl ≠ null?
    │   ├─→ YES: PlayDirectAudioAsync() → Play file ✅
    │   └─→ NO: Continue...
    │
    ├─→ DisplayText ≠ null?
    │   ├─→ YES: GetTtsAudioUrlAsync()
    │   │   ├─→ API success? → PlayDirectAudioAsync() → Play TTS ✅
    │   │   └─→ API fail? → SpeakTextAsync() → Native TTS ✅
    │   └─→ NO: Show alert "No content" ❌
```

---

## 🚀 Future Enhancements

- [ ] Download audio locally (cache)
- [ ] Background playback
- [ ] Playback controls (pause, resume, speed)
- [ ] Multiple language audio mixing
- [ ] Neural voices (Azure Cognitive Services)
- [ ] Offline TTS (native only)

---

## 🔐 Permissions (Android/iOS)

Add to `AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

Add to `Info.plist` (iOS):
```xml
<key>NSLocalNetworkUsageDescription</key>
<string>App needs access to network for audio</string>
```

---

## ✅ Status

- [x] AudioService implementation
- [x] API integration
- [x] DetailViewModel setup
- [x] DetailPage.xaml.cs update
- [x] Multi-language support
- [x] Error handling & fallback
- [ ] UI elements (add play button to XAML)
- [ ] Testing on device
- [ ] Performance optimization

---

**Ready to use! 🎙️**
