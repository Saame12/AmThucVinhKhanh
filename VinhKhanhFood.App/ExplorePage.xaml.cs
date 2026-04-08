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

        // Đăng ký sự kiện đổi ngôn ngữ để làm mới danh sách ngay lập tức
        Services.LocalizationService.LanguageChanged += OnLanguageChanged;

        // Tải dữ liệu lần đầu hoặc làm mới
        if (_viewModel.Locations.Count == 0)
        {
            await _viewModel.LoadLocationsAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Hủy đăng ký để tránh rò rỉ bộ nhớ
        Services.LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, Services.LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // 1. Cập nhật Tiêu đề trang từ file Resource dịch thuật
            Title = Services.LocalizationService.GetString("Explore");

            // 2. Ép danh sách cập nhật lại để gọi vào DisplayName/DisplayDescription
            if (_viewModel.Locations != null && _viewModel.Locations.Any())
            {
                var currentData = _viewModel.Locations.ToList();
                _viewModel.Locations.Clear();
                foreach (var item in currentData)
                {
                    _viewModel.Locations.Add(item);
                }
            }
        });
    }

    private async void OnLocationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is FoodLocation selectedLocation)
        {
            // Điều hướng sang trang chi tiết
            await Navigation.PushAsync(new DetailPage(selectedLocation));

            // Bỏ chọn item trên UI
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    
}