using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.ViewModels;

namespace VinhKhanhFood.App;

public partial class MainPage : ContentPage
{
    private readonly MapViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MapViewModel();
        BindingContext = _viewModel;
        _viewModel.OnLocationsLoaded += DrawMapElements;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopTracking();
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
                    Label = loc.Name,
                    Address = "Nhấn để xem chi tiết",
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
                vinhKhanhMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(first.Latitude, first.Longitude),
                    Distance.FromKilometers(0.5)));
            }
        });
    }

    private async void OnMapInfoWindowClicked(object? sender, PinClickedEventArgs e)
    {
        e.HideInfoWindow = true;

        if (sender is not Pin clickedPin)
        {
            return;
        }

        var locData = _viewModel.Locations.FirstOrDefault(location => location.Name == clickedPin.Label);
        if (locData is null)
        {
            return;
        }

        FoodBottomSheet.BindingContext = locData;
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
}
