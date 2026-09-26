using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;

    public RegisterPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        RolePicker.SelectedIndex = 0;
    }

    private async void RegisterButton_Clicked(
        object sender,
        EventArgs e)
    {
        string fullName =
            FullNameEntry.Text?.Trim() ?? "";

        string email =
            EmailEntry.Text?.Trim() ?? "";

        string phone =
            PhoneEntry.Text?.Trim() ?? "";

        string password =
            PasswordEntry.Text ?? "";

        string role =
            RolePicker.SelectedItem?.ToString()
            ?? "CUSTOMER";

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert(
                "Thông báo",
                "Vui lòng nhập đầy đủ thông tin.",
                "OK");

            return;
        }

        if (password.Length < 8 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) ||
            !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            await DisplayAlert(
                "Mật khẩu không đủ mạnh",
                "Mật khẩu phải có ít nhất 8 ký tự, " +
                "bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.",
                "OK");

            return;
        }

        RegisterButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var request = new RegisterRequest
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Password = password,
                Role = role
            };

            await _apiService.RegisterAsync(request);

            await DisplayAlert(
                "Thành công",
                "Đăng ký tài khoản thành công.",
                "OK");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Đăng ký thất bại",
                ex.Message,
                "OK");
        }
        finally
        {
            RegisterButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}