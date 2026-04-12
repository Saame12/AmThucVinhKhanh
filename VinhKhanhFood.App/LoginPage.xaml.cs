using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class LoginPage : ContentPage
{
    private bool _isRegisterMode;

    public LoginPage()
    {
        InitializeComponent();
        UpdateMode();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalizationService.LanguageChanged += OnLanguageChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateMode);
    }

    private void UpdateMode()
    {
        AuthTitleLabel.Text = LocalizationService.GetString(_isRegisterMode ? "RegisterTitle" : "LoginTitle");
        AuthSubtitleLabel.Text = LocalizationService.GetString("AuthSubtitle");
        TxtFullName.Placeholder = LocalizationService.GetString("FullName");
        TxtUsername.Placeholder = LocalizationService.GetString("Username");
        TxtPassword.Placeholder = LocalizationService.GetString("Password");
        TxtConfirmPassword.Placeholder = LocalizationService.GetString("ConfirmPassword");
        SubmitButton.Text = LocalizationService.GetString(_isRegisterMode ? "SubmitRegister" : "SubmitLogin");
        SwitchModeButton.Text = LocalizationService.GetString(_isRegisterMode ? "SwitchToLogin" : "SwitchToRegister");
        CancelButton.Text = LocalizationService.GetString("Cancel");
        TxtFullName.IsVisible = _isRegisterMode;
        ConfirmPasswordBorder.IsVisible = _isRegisterMode;
    }

    private async void OnSubmitBtnClicked(object sender, EventArgs e)
    {
        SubmitButton.IsEnabled = false;

        try
        {
            if (_isRegisterMode)
            {
                await RegisterAsync();
            }
            else
            {
                await LoginAsync();
            }
        }
        finally
        {
            SubmitButton.IsEnabled = true;
        }
    }

    private async Task LoginAsync()
    {
        var request = new LoginRequest
        {
            Username = TxtUsername.Text?.Trim() ?? string.Empty,
            Password = TxtPassword.Text ?? string.Empty
        };

        var result = await App.Auth.LoginAsync(request);
        if (!result.Success)
        {
            await DisplayAlert(LocalizationService.GetString("Error"), result.ErrorMessage ?? LocalizationService.GetString("AuthRequestFailed"), LocalizationService.GetString("OK"));
            return;
        }

        await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("AuthLoginSuccess"), LocalizationService.GetString("OK"));
        await Navigation.PopModalAsync();
    }

    private async Task RegisterAsync()
    {
        if (!string.Equals(TxtPassword.Text, TxtConfirmPassword.Text, StringComparison.Ordinal))
        {
            await DisplayAlert(LocalizationService.GetString("Error"), LocalizationService.GetString("AuthPasswordMismatch"), LocalizationService.GetString("OK"));
            return;
        }

        var request = new RegisterRequest
        {
            FullName = TxtFullName.Text?.Trim() ?? string.Empty,
            Username = TxtUsername.Text?.Trim() ?? string.Empty,
            Password = TxtPassword.Text ?? string.Empty
        };

        var result = await App.Auth.RegisterAsync(request);
        if (!result.Success)
        {
            await DisplayAlert(LocalizationService.GetString("Error"), result.ErrorMessage ?? LocalizationService.GetString("AuthRequestFailed"), LocalizationService.GetString("OK"));
            return;
        }

        await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("AuthRegisterSuccess"), LocalizationService.GetString("OK"));
        await Navigation.PopModalAsync();
    }

    private void OnSwitchModeClicked(object sender, EventArgs e)
    {
        _isRegisterMode = !_isRegisterMode;
        UpdateMode();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
