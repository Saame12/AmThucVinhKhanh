using VinhKhanhFood.App.ViewModels;
using VinhKhanhFood.App.Models;

namespace VinhKhanhFood.App;

public partial class ExplorePage : ContentPage
{
    private readonly ExploreViewModel _viewModel;

    public ExplorePage()
    {
        InitializeComponent();
        _viewModel = new ExploreViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Subscribe vào event LanguageChanged
        Services.LocalizationService.LanguageChanged += OnLanguageChanged;

        await _viewModel.LoadLocationsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe từ event
        Services.LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, Services.LanguageChangedEventArgs e)
    {
        // Cập nhật UI khi ngôn ngữ thay đổi
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Cập nhật tiêu đề trang khi đổi ngôn ngữ
            Title = Services.LocalizationService.GetString("Explore");
            // Làm mới danh sách để DisplayName và DisplayDescription cập nhật theo ngôn ngữ mới
            var tempLocations = _viewModel.Locations.ToList();
            _viewModel.Locations.Clear();
            foreach (var item in tempLocations) _viewModel.Locations.Add(item);
        });
    }

    // Xử lý khi người dùng bấm vào một quán ốc trong danh sách
    private async void OnLocationSelected(object sender, SelectionChangedEventArgs e)
    {
        // Lấy dữ liệu của quán vừa được bấm
        if (e.CurrentSelection.FirstOrDefault() is FoodLocation selectedLocation)
        {
            // Điều hướng đến trang chi tiết, truyền dữ liệu của quán vừa chọn
            await Shell.Current.Navigation.PushAsync(new DetailPage(selectedLocation));
            // Bỏ highlight (bỏ bôi đen) cái thẻ vừa chọn
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    
}