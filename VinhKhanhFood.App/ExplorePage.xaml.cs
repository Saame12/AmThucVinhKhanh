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
        WidgetTitleLabel.Text = LocalizationService.GetString("FeaturedPoiWidgetTitle");
        WidgetSubtitleLabel.Text = LocalizationService.GetString("FeaturedPoiWidgetSubtitle");
        NearbyListTitleLabel.Text = LocalizationService.GetString("NearbyPoiListTitle");
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
}
