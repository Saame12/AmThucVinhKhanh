using System.Net.Http.Json;

namespace VinhKhanhFood.App;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginBtnClicked(object sender, EventArgs e)
    {
        string username = TxtUsername.Text;
        string password = TxtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Thông báo", "Vui lòng nhập đầy đủ thông tin", "OK");
            return;
        }

        try
        {
            using var client = new HttpClient();
            // LƯU Ý: Thay đổi IP này thành IP máy tính của bạn (kiểm tra bằng ipconfig)
            string apiUrl = "http://10.0.2.2:5020/api/User/login";

            var loginInfo = new { Username = username, Password = password };
            var response = await client.PostAsJsonAsync(apiUrl, loginInfo);

            if (response.IsSuccessStatusCode)
            {
                // Đổi từ dynamic sang LoginResponse
                var user = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (user != null)
                {
                    // Lưu vào bộ nhớ máy
                    Preferences.Default.Set("IsLoggedIn", true);
                    Preferences.Default.Set("UserName", user.fullName);
                    Preferences.Default.Set("UserRole", user.role);

                    await DisplayAlert("Thành công", $"Chào mừng {user.fullName}!", "OK");
                    await Navigation.PopModalAsync();
                }
            }
            else
            {
                await DisplayAlert("Lỗi", "Tài khoản hoặc mật khẩu không đúng", "Thử lại");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi kết nối", "Không thể kết nối đến máy chủ: " + ex.Message, "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
    public class LoginResponse
    {
        public int id { get; set; }
        public string username { get; set; }
        public string fullName { get; set; }
        public string role { get; set; }
    }
}