using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class CustomerProfilePage : ContentPage
{
    private readonly ApiService _apiService;

    public CustomerProfilePage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var profile =
                await _apiService.GetMyProfileAsync();

            if (profile == null)
                return;

            FullNameEntry.Text = profile.FullName;
            EmailEntry.Text = profile.Email;
            PhoneEntry.Text = profile.Phone;
            AvatarEntry.Text = profile.AvatarUrl;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
    }

    private async void SaveButton_Clicked(
        object sender,
        EventArgs e)
    {
        string fullName =
            FullNameEntry.Text?.Trim() ?? "";

        string phone =
            PhoneEntry.Text?.Trim() ?? "";

        string avatarUrl =
            AvatarEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(phone))
        {
            await DisplayAlert(
                "Thông báo",
                "Họ tên và số điện thoại không được để trống.",
                "OK");

            return;
        }

        SaveButton.IsEnabled = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            await _apiService.UpdateMyProfileAsync(
                fullName,
                phone,
                avatarUrl);

            Preferences.Set(
                "full_name",
                fullName);

            Preferences.Set(
                "phone",
                phone);

            await DisplayAlert(
                "Thành công",
                "Đã cập nhật hồ sơ.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}