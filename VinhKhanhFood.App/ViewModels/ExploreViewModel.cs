using System.Collections.ObjectModel;
using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App.ViewModels;

public class ExploreViewModel : BindableObject
{
    private readonly ApiService _apiService = new ApiService();

    // Danh sách này sẽ tự động đổ vào CollectionView ở XAML
    public ObservableCollection<FoodLocation> Locations { get; set; } = new();
    public ObservableCollection<FoodLocation> HotLocations { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public async Task LoadLocationsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var data = await _apiService.GetFoodLocationsAsync();
            Locations.Clear();
            foreach (var item in data)
            {
                Locations.Add(item);
            }

            RefreshHotLocations();
        }
        catch (Exception)
        {
            // Có thể dùng DisplayAlert để báo lỗi
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void RefreshLocalizedProjection()
    {
        var snapshot = Locations.ToList();
        Locations.Clear();

        foreach (var item in snapshot)
        {
            Locations.Add(item);
        }

        RefreshHotLocations();
    }

    private void RefreshHotLocations()
    {
        HotLocations.Clear();

        foreach (var item in Locations.Take(5))
        {
            HotLocations.Add(item);
        }
    }
}
