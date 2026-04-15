using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;
using VinhKhanhFood.App.ViewModels;

namespace VinhKhanhFood.App;

public partial class MainPage : ContentPage
{
    private readonly MapViewModel _viewModel;
    private readonly ObservableCollection<FoodLocation> _searchResults = new();

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MapViewModel();
        BindingContext = _viewModel;
        _viewModel.OnLocationsLoaded += DrawMapElements;
        SearchResultsView.ItemsSource = _searchResults;
        UpdateLocalizedTexts();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LocalizationService.LanguageChanged += OnLanguageChanged;
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        _viewModel.StopTracking();
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocalizedTexts();
            DrawMapElements(_viewModel.Locations);
            RefreshSearchResults(PoiSearchEntry.Text);
        });
    }

    private void UpdateLocalizedTexts()
    {
        PoiSearchEntry.Placeholder = GetSearchPlaceholderText();
        BottomAudioBadgeLabel.Text = GetBottomAudioBadgeText();
        ViewDetailsButton.Text = LocalizationService.GetString("View Details");
    }

    private void DrawMapElements(List<FoodLocation> locations)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            vinhKhanhMap.Pins.Clear();
            vinhKhanhMap.MapElements.Clear();

            foreach (var loc in locations)
            {
                var pinLocation = new Location(loc.Latitude, loc.Longitude);
                var pin = new Pin
                {
                    Label = loc.DisplayName,
                    Address = LocalizationService.GetString("MapPinTapHint"),
                    Location = pinLocation
                };

                pin.MarkerClicked += OnMapInfoWindowClicked;
                vinhKhanhMap.Pins.Add(pin);

                var circle = new Circle
                {
                    Center = pinLocation,
                    Radius = new Distance(30),
                    StrokeColor = Colors.Blue,
                    StrokeWidth = 2,
                    FillColor = Color.FromArgb("#330099FF")
                };
                vinhKhanhMap.MapElements.Add(circle);
            }

            if (locations.Any())
            {
                var first = locations[0];
                MoveMapToLocation(first);
            }
        });
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshSearchResults(e.NewTextValue);
    }

    private void RefreshSearchResults(string? keyword)
    {
        _searchResults.Clear();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            SearchResultsCard.IsVisible = false;
            return;
        }

        foreach (var item in _viewModel.Locations
                     .Where(location => location.DisplayName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
                     .Take(6))
        {
            _searchResults.Add(item);
        }

        SearchResultsCard.IsVisible = _searchResults.Count > 0;
    }

    private async void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FoodLocation selectedLocation)
        {
            return;
        }

        SearchResultsCard.IsVisible = false;
        PoiSearchEntry.Text = selectedLocation.DisplayName;
        MoveMapToLocation(selectedLocation);
        await ShowBottomSheetAsync(selectedLocation);
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnMapInfoWindowClicked(object? sender, PinClickedEventArgs e)
    {
        e.HideInfoWindow = true;

        if (sender is not Pin clickedPin)
        {
            return;
        }

        var locData = _viewModel.Locations.FirstOrDefault(location => location.DisplayName == clickedPin.Label);
        if (locData is null)
        {
            return;
        }

        await ShowBottomSheetAsync(locData);
    }

    private async Task ShowBottomSheetAsync(FoodLocation location)
    {
        FoodBottomSheet.BindingContext = location;
        FoodBottomSheet.IsVisible = true;
        FoodBottomSheet.TranslationY = 300;
        await FoodBottomSheet.TranslateTo(0, 0, 300, Easing.SinOut);
    }

    private async void OnCloseBottomSheetClicked(object sender, EventArgs e)
    {
        _viewModel.CancelSpeech();
        await FoodBottomSheet.TranslateTo(0, 300, 250, Easing.SinIn);
        FoodBottomSheet.IsVisible = false;
    }

    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        if (FoodBottomSheet.BindingContext is not FoodLocation selectedLocation)
        {
            return;
        }

        _viewModel.CancelSpeech();
        await Navigation.PushAsync(new DetailPage(selectedLocation));
    }

    private async void OnCurrentLocationClicked(object sender, EventArgs e)
    {
        var location = await _viewModel.GetCurrentLocationAsync();
        if (location is null)
        {
            return;
        }

        vinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            location,
            Distance.FromKilometers(0.5)));
    }

    private void MoveMapToLocation(FoodLocation location)
    {
        vinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(location.Latitude, location.Longitude),
            Distance.FromKilometers(0.35)));
    }

    private static string GetSearchPlaceholderText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Search POIs",
        "zh" => "搜索 POI",
        _ => "Tìm kiếm POIs"
    };

    private static string GetBottomAudioBadgeText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Audio",
        "zh" => "音频",
        _ => "Audio"
    };
}
