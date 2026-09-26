using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace EmergencyHomeServiceMobile.Views.Worker;

public partial class WorkerJobPage : ContentPage
{
    private readonly ApiService _apiService;

    private readonly WorkerAvailableRequest _request;

    private string _currentStatus = "ACCEPTED";

    private CancellationTokenSource? _locationCts;

    public WorkerJobPage(WorkerAvailableRequest request)
    {
        InitializeComponent();

        _apiService = new ApiService();

        _request = request;

        ServiceNameLabel.Text =
            request.ServiceName;

        CustomerLabel.Text =
            $"Khách hàng: {request.CustomerName}";

        DescriptionLabel.Text =
            request.Description;

        AddressLabel.Text =
            $"Địa chỉ: {request.Address ?? "Chưa cung cấp"}";

        DistanceLabel.Text =
            $"Khoảng cách: {request.DistanceKm:F2} km";

        StatusLabel.Text =
            _currentStatus;

        UpdateButtons();
    }

    //////////////////////////////////////////////////////////////////////
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _locationCts?.Cancel();
        _locationCts?.Dispose();

        _locationCts = new CancellationTokenSource();

        _ = StartLocationTrackingAsync(
            _locationCts.Token);
    }
    //////////////////////////////////////////////////////////////////////

    private async Task StartLocationTrackingAsync(
    CancellationToken cancellationToken)
    {
        try
        {
            var permissionStatus =
                await Permissions.CheckStatusAsync<
                    Permissions.LocationWhenInUse>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus =
                    await Permissions.RequestAsync<
                        Permissions.LocationWhenInUse>();
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text =
                        "GPS: chưa được cấp quyền.";
                });

                return;
            }

            if (!Geolocation.Default.IsEnabled)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text =
                        "GPS: dịch vụ vị trí đang tắt.";
                });

                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LocationLabel.Text =
                    "GPS: đang theo dõi...";
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(
                        GeolocationAccuracy.Medium,
                        TimeSpan.FromSeconds(10));

                    var location =
                        await Geolocation.Default.GetLocationAsync(
                            request);

                    if (location != null)
                    {
                        var result =
                            await _apiService
                                .UpdateWorkerLocationAsync(
                                    location.Latitude,
                                    location.Longitude);

                        if (result != null)
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                LocationLabel.Text =
                                    $"GPS: {result.Latitude:F6}, " +
                                    $"{result.Longitude:F6}\n" +
                                    $"Cập nhật: " +
                                    $"{result.UpdatedAt.ToLocalTime():HH:mm:ss}";
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "GPS update error: " + ex);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Bình thường khi rời khỏi WorkerJobPage
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                "Location tracking error: " + ex);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _locationCts?.Cancel();
        _locationCts?.Dispose();
        _locationCts = null;
    }

    ///////////////////////////////////////////////////////////////////////////////////

    private void UpdateButtons()
    {
        OnTheWayButton.IsVisible =
            _currentStatus == "ACCEPTED";

        ArrivedButton.IsVisible =
            _currentStatus == "ON_THE_WAY";

        InProgressButton.IsVisible =
            _currentStatus == "ARRIVED";

        CompletedButton.IsVisible =
            _currentStatus == "IN_PROGRESS";
    }

    private async Task ChangeStatusAsync(
        string newStatus,
        string note)
    {
        bool confirm = await DisplayAlert(
            "Xác nhận",
            $"Chuyển trạng thái thành:\n{newStatus}?",
            "Xác nhận",
            "Hủy");

        if (!confirm)
            return;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        OnTheWayButton.IsEnabled = false;
        ArrivedButton.IsEnabled = false;
        InProgressButton.IsEnabled = false;
        CompletedButton.IsEnabled = false;

        try
        {
            var result =
                await _apiService.UpdateRequestStatusAsync(
                    _request.RequestId,
                    newStatus,
                    note);

            if (result == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không nhận được kết quả từ server.",
                    "OK");

                return;
            }

            _currentStatus = result.Status;

            StatusLabel.Text =
                _currentStatus;

            UpdateButtons();

            await DisplayAlert(
                "Thành công",
                $"Trạng thái đã chuyển sang {_currentStatus}.",
                "OK");

            if (_currentStatus == "COMPLETED")
            {
                await DisplayAlert(
                    "Hoàn thành",
                    "Công việc đã hoàn thành.\nWorker đã sẵn sàng nhận việc mới.",
                    "OK");

                await Navigation.PopAsync();
            }
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "Không thể cập nhật",
                ex.Message,
                "OK");

            UpdateButtons();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");

            UpdateButtons();
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            OnTheWayButton.IsEnabled = true;
            ArrivedButton.IsEnabled = true;
            InProgressButton.IsEnabled = true;
            CompletedButton.IsEnabled = true;

            UpdateButtons();
        }
    }

    private async void OnTheWayButton_Clicked(
        object sender,
        EventArgs e)
    {
        await ChangeStatusAsync(
            "ON_THE_WAY",
            "Worker bắt đầu di chuyển đến địa điểm.");
    }

    private async void ArrivedButton_Clicked(
        object sender,
        EventArgs e)
    {
        await ChangeStatusAsync(
            "ARRIVED",
            "Worker đã đến địa điểm.");
    }

    private async void InProgressButton_Clicked(
        object sender,
        EventArgs e)
    {
        await ChangeStatusAsync(
            "IN_PROGRESS",
            "Worker bắt đầu sửa chữa.");
    }

    private async void CompletedButton_Clicked(
        object sender,
        EventArgs e)
    {
        await ChangeStatusAsync(
            "COMPLETED",
            "Worker đã hoàn thành công việc.");
    }
}