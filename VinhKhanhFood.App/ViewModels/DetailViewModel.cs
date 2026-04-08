using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App.ViewModels
{
    public class DetailViewModel : BaseViewModel
    {
        private FoodLocation foodLocation;
        private AudioService audioService;
        private ApiService apiService;
        private bool isPlayingAudio;

        public FoodLocation FoodLocation
        {
            get => foodLocation;
            set
            {
                if (foodLocation != value)
                {
                    foodLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlayingAudio
        {
            get => isPlayingAudio;
            set
            {
                if (isPlayingAudio != value)
                {
                    isPlayingAudio = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands
        public IAsyncRelayCommand PlayAudioCommand { get; }
        public IAsyncRelayCommand SpeakCommand { get; }

        public DetailViewModel()
        {
            audioService = new AudioService();
            apiService = new ApiService();

            PlayAudioCommand = new AsyncRelayCommand(PlayAudioAsync);
            SpeakCommand = new AsyncRelayCommand(SpeakAsync);
        }

        /// <summary>
        /// Play audio: 
        /// 1. Nếu có AudioUrl → phát file
        /// 2. Không có → gọi API TTS → phát Google Translate audio
        /// </summary>
        private async Task PlayAudioAsync()
        {
            if (FoodLocation == null)
                return;

            try
            {
                IsPlayingAudio = true;

                // Ưu tiên 1: Nếu có Audio URL → phát trực tiếp
                if (!string.IsNullOrWhiteSpace(FoodLocation.DisplayAudioUrl))
                {
                    await audioService.PlayAudioAsync(
                        FoodLocation.DisplayAudioUrl,
                        null,
                        FoodLocation.LanguageCode
                    );
                }
                // Ưu tiên 2: Không có Audio URL → dùng TTS
                else if (!string.IsNullOrWhiteSpace(FoodLocation.DisplayText))
                {
                    // Get TTS URL from API
                    var ttsUrl = await audioService.GetTtsAudioUrlAsync(
                        FoodLocation.DisplayText,
                        FoodLocation.LanguageCode
                    );

                    if (!string.IsNullOrWhiteSpace(ttsUrl))
                    {
                        await audioService.PlayAudioAsync(
                            ttsUrl,
                            null,
                            FoodLocation.LanguageCode
                        );
                    }
                    else
                    {
                        // Fallback: Dùng MAUI TextToSpeech
                        await audioService.SpeakTextAsync(
                            FoodLocation.DisplayText,
                            FoodLocation.LanguageCode
                        );
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Thông báo",
                        "Không có nội dung để phát âm thanh",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing audio: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Lỗi",
                    $"Không thể phát âm thanh: {ex.Message}",
                    "OK"
                );
            }
            finally
            {
                IsPlayingAudio = false;
            }
        }

        /// <summary>
        /// Speak using MAUI TextToSpeech API (fallback)
        /// </summary>
        private async Task SpeakAsync()
        {
            if (FoodLocation == null || string.IsNullOrWhiteSpace(FoodLocation.DisplayText))
                return;

            try
            {
                IsPlayingAudio = true;
                await audioService.SpeakTextAsync(
                    FoodLocation.DisplayText,
                    FoodLocation.LanguageCode
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error speaking: {ex.Message}");
            }
            finally
            {
                IsPlayingAudio = false;
            }
        }

        /// <summary>
        /// Load food location by ID from API
        /// </summary>
        public async Task LoadFoodLocationAsync(int foodLocationId)
        {
            try
            {
                IsBusy = true;
                
                // Gọi API để lấy chi tiết
                var foodLocation = await apiService.GetFoodLocationAsync(foodLocationId);
                FoodLocation = foodLocation;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading food location: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Lỗi",
                    $"Không thể tải thông tin: {ex.Message}",
                    "OK"
                );
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
