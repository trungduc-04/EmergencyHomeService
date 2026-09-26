using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class CreateRequestPage : ContentPage
{
    private readonly ApiService _apiService;

    private readonly Service _selectedService;

    private double? _latitude;
    private double? _longitude;

    public CreateRequestPage(Service selectedService)
    {
        InitializeComponent();

        _apiService = new ApiService();

        _selectedService = selectedService;

        ServiceNameLabel.Text = selectedService.ServiceName;
    }

    private async void GetLocationButton_Clicked(
        object sender,
        EventArgs e)
    {
        GetLocationButton.IsEnabled = false;

        try
        {
            var permissionStatus =
                await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus =
                    await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                await DisplayAlert(
                    "Quyền vị trí",
                    "Ứng dụng cần quyền vị trí để tạo yêu cầu sửa chữa.",
                    "OK");

                return;
            }

            if (!Geolocation.Default.IsEnabled)
            {
                await DisplayAlert(
                    "GPS",
                    "Vui lòng bật dịch vụ vị trí trên thiết bị.",
                    "OK");

                return;
            }

            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10));

            var location =
                await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                await DisplayAlert(
                    "GPS",
                    "Không lấy được vị trí hiện tại.",
                    "OK");

                return;
            }

            _latitude = location.Latitude;
            _longitude = location.Longitude;

            LocationLabel.Text =
                $"Vĩ độ: {_latitude:F6}\n" +
                $"Kinh độ: {_longitude:F6}";
        }
        catch (PermissionException)
        {
            await DisplayAlert(
                "Quyền vị trí",
                "Không có quyền truy cập vị trí.",
                "OK");
        }
        catch (FeatureNotEnabledException)
        {
            await DisplayAlert(
                "GPS",
                "Dịch vụ vị trí đang tắt.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi GPS",
                ex.Message,
                "OK");
        }
        finally
        {
            GetLocationButton.IsEnabled = true;
        }
    }

    private async void CreateRequestButton_Clicked(
        object sender,
        EventArgs e)
    {
        string description =
            DescriptionEditor.Text?.Trim() ?? "";

        string address =
            AddressEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(description))
        {
            await DisplayAlert(
                "Thiếu thông tin",
                "Vui lòng mô tả sự cố.",
                "OK");

            return;
        }

        if (!_latitude.HasValue || !_longitude.HasValue)
        {
            await DisplayAlert(
                "Thiếu vị trí",
                "Vui lòng lấy vị trí hiện tại trước khi tạo yêu cầu.",
                "OK");

            return;
        }

        CreateRequestButton.IsEnabled = false;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var request = new CreateServiceRequestRequest
            {
                ServiceId = _selectedService.ServiceId,
                Description = description,
                Latitude = _latitude.Value,
                Longitude = _longitude.Value,
                Address = string.IsNullOrWhiteSpace(address)
                    ? null
                    : address
            };

            var result =
                await _apiService.CreateServiceRequestAsync(request);

            if (result == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không nhận được dữ liệu từ server.",
                    "OK");

                return;
            }

            await DisplayAlert(
                    "Tạo yêu cầu thành công",
                    $"Mã yêu cầu: {result.RequestId}\n" +
                    $"Dịch vụ: {result.ServiceName}\n" +
                    $"Trạng thái: {result.Status}",
                    "OK");

            // Lưu yêu cầu hiện tại để Customer có thể mở lại sau này
            Preferences.Set(
                "current_request_id",
                result.RequestId);

            await Navigation.PushAsync(
                new MatchingWorkersPage(result.RequestId));

            // Không cần giữ CreateRequestPage trong navigation stack
            Navigation.RemovePage(this);
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "API Error",
                ex.Message,
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
            CreateRequestButton.IsEnabled = true;

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}