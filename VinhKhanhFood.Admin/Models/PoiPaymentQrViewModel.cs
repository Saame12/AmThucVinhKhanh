using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.Admin.Models;

public class PoiPaymentQrViewModel
{
    public FoodLocation Poi { get; set; } = new();
    public decimal SelectedAmount { get; set; }
    public List<decimal> SuggestedAmounts { get; set; } = [];
}
