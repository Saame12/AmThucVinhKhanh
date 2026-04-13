using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;
using VinhKhanhFood.App.ViewModels;

namespace VinhKhanhFood.App;

public partial class ExplorePage : ContentPage
{
    private readonly ExploreViewModel _viewModel;

    public ExplorePage()
    {
        InitializeComponent();
        _viewModel = new ExploreViewModel();
        BindingContext = _viewModel;
        UpdateLocalizedTexts();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LocalizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedTexts();
        _viewModel.RefreshLocalizedProjection();

        if (_viewModel.Locations.Count == 0)
        {
            await _viewModel.LoadLocationsAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocalizedTexts();
            _viewModel.RefreshLocalizedProjection();
        });
    }

    private void UpdateLocalizedTexts()
    {
        PageEyebrowLabel.Text = LocalizationService.GetString("Explore");
        PageTitleLabel.Text = GetPageTitleText();
        PageSubtitleLabel.Text = GetPageSubtitleText();
        WidgetTitleLabel.Text = LocalizationService.GetString("FeaturedPoiWidgetTitle");
        WidgetSubtitleLabel.Text = LocalizationService.GetString("FeaturedPoiWidgetSubtitle");
        NearbyListTitleLabel.Text = LocalizationService.GetString("NearbyPoiListTitle");
        TopBadgeLabel.Text = GetTopBadgeText();
    }

    private async void OnLocationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FoodLocation selectedLocation)
        {
            return;
        }

        await Navigation.PushAsync(new DetailPage(selectedLocation));
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnHotLocationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FoodLocation selectedLocation)
        {
            return;
        }

        await Navigation.PushAsync(new DetailPage(selectedLocation));
        ((CollectionView)sender).SelectedItem = null;
    }

    private static string GetPageTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Places worth visiting",
        "zh" => "值得一试的地点",
        _ => "Những điểm đáng ghé"
    };

    private static string GetPageSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "A compact list of standout venues and POIs around Vinh Khanh.",
        "zh" => "荣庆周边值得一逛的店铺与 POI 精选。",
        _ => "Danh sách gọn các quán và POI nổi bật quanh khu Vĩnh Khánh."
    };

    private static string GetTopBadgeText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Top picks",
        "zh" => "精选",
        _ => "Nổi bật"
    };
}
