using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;
using EmergencyHomeServiceMobile.Views.Customer;
using EmergencyHomeServiceMobile.Views.Worker;
using EmergencyHomeServiceMobile.Views.Auth;


namespace EmergencyHomeServiceMobile.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        EmailOrPhoneEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        string emailOrPhone = EmailOrPhoneEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(emailOrPhone) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert(
                "Thông báo",
                "Vui lòng nhập email/số điện thoại và mật khẩu.",
                "OK");

            return;
        }

        LoginButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var request = new LoginRequest
            {
                EmailOrPhone = emailOrPhone,
                Password = password
            };

            var result = await _apiService.LoginAsync(request);

            if (result == null)
            {
                await DisplayAlert(
                    "Đăng nhập thất bại",
                    "Email/số điện thoại hoặc mật khẩu không đúng.",
                    "OK");

                return;
            }

            // Lưu thông tin đăng nhập
            Preferences.Set("jwt_token", result.Token);
            Preferences.Set("user_id", result.UserId);
            Preferences.Set("user_role", result.Role);
            Preferences.Set("full_name", result.FullName);
            Preferences.Set("email", result.Email);
            Preferences.Set("phone", result.Phone);

            await DisplayAlert(
                "Đăng nhập thành công",
                $"Xin chào {result.FullName}\n" +
                $"Vai trò: {result.Role}",
                "OK");

            if (result.Role.Equals(
                "CUSTOMER",
                StringComparison.OrdinalIgnoreCase))
                    {
                Application.Current!.MainPage =
                 new NavigationPage(
                      new CustomerHomePage());

                return;
                    }


            if (result.Role.Equals(
                    "WORKER",
                    StringComparison.OrdinalIgnoreCase))
            {
                Application.Current!.MainPage =
                 new NavigationPage(
                    new WorkerHomePage());

                return;
            }





        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.ToString(),
                "OK");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
    /////////////////////////////////////////////////////////////////////////
    private async void RegisterButton_Clicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new RegisterPage());
    }
    ////////////////////////////////////////////////////////////




}