using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;
using EmergencyHomeServiceMobile.Views.Auth;


namespace EmergencyHomeServiceMobile.Views.Worker;

public partial class WorkerHomePage : ContentPage
{
    private readonly ApiService _apiService;

    private bool _isOnline = false;

    private readonly HashSet<long> _hiddenRequestIds = new();

    public WorkerHomePage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        string fullName = Preferences.Get(
            "full_name",
            "Thợ sửa chữa");

        WelcomeLabel.Text = $"Xin chào, {fullName}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        string fullName = Preferences.Get(
            "full_name",
            "Thợ sửa chữa");

        WelcomeLabel.Text =
            $"Xin chào, {fullName}";

        UpdateOnlineUI();

        await LoadRequestsAsync();
    }

    private void UpdateOnlineUI()
    {
        if (_isOnline)
        {
            OnlineStatusLabel.Text = "ONLINE";
            OnlineButton.Text = "TẮT ONLINE";
        }
        else
        {
            OnlineStatusLabel.Text = "OFFLINE";
            OnlineButton.Text = "BẬT ONLINE";
        }
    }

    ///////////////////////////////////////////////////////////
    private async void OnlineButton_Clicked(
    object sender,
    EventArgs e)
    {
        OnlineButton.IsEnabled = false;

        try
        {
            bool newOnlineStatus = !_isOnline;

            var result =
                await _apiService
                    .UpdateWorkerAvailabilityAsync(
                        newOnlineStatus,
                        newOnlineStatus);

            if (result == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không nhận được dữ liệu từ server.",
                    "OK");

                return;
            }

            _isOnline = result.IsOnline;

            UpdateOnlineUI();

            if (_isOnline)
            {
                await DisplayAlert(
                    "Trạng thái",
                    "Bạn đang ONLINE và có thể nhận yêu cầu.",
                    "OK");
            }
            else
            {
                await DisplayAlert(
                    "Trạng thái",
                    "Bạn đang OFFLINE và sẽ không nhận yêu cầu mới.",
                    "OK");
            }

            await LoadRequestsAsync();
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "Không thể thay đổi trạng thái",
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
            OnlineButton.IsEnabled = true;
        }
    }
    ////////////////////////////////////////////////////////////

    private async Task LoadRequestsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var result =
                await _apiService.GetAvailableRequestsForWorkerAsync();

            if (result == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không nhận được dữ liệu từ server.",
                    "OK");

                return;
            }

            var visibleRequests = result.Requests
                .Where(x => !_hiddenRequestIds.Contains(x.RequestId))
                .ToList();

            SummaryLabel.Text =
                $"Tìm thấy {visibleRequests.Count} yêu cầu " +
                $"trong bán kính {result.SearchRadiusKm:F1} km.";

            RequestsCollectionView.ItemsSource =
                visibleRequests;
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
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
    //////////////////////////////////////////////////////////////////////////////////
    private async void AcceptRequest_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not WorkerAvailableRequest request)
        {
            return;
        }

        bool confirm = await DisplayAlert(
            "Xác nhận",
            $"Bạn muốn nhận yêu cầu #{request.RequestId}?",
            "Nhận",
            "Hủy");

        if (!confirm)
            return;

        button.IsEnabled = false;

        try
        {
            await _apiService.AcceptRequestAsync(
                request.RequestId);

            _hiddenRequestIds.Add(request.RequestId);

            await DisplayAlert(
                "Thành công",
                $"Đã nhận yêu cầu #{request.RequestId}.",
                "OK");

            await Navigation.PushAsync(
                new WorkerJobPage(request));
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "Không thể nhận việc",
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
            button.IsEnabled = true;
        }
    }
    ///////////////////////////////////////////////////////////////////////////////
    private async void RejectRequest_Clicked(
    object sender,
    EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext
                is not WorkerAvailableRequest request)
        {
            return;
        }

        string? reason =
            await DisplayPromptAsync(
                "Từ chối yêu cầu",
                "Nhập lý do:");

        if (reason == null)
            return;

        button.IsEnabled = false;

        try
        {
            await _apiService.RejectRequestAsync(
                request.RequestId,
                reason);

            _hiddenRequestIds.Add(request.RequestId);

            await DisplayAlert(
                "Thành công",
                $"Đã từ chối yêu cầu #{request.RequestId}.",
                "OK");

            await LoadRequestsAsync();
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
            button.IsEnabled = true;
        }
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    private async void RefreshButton_Clicked(
        object sender,
        EventArgs e)
    {
        await LoadRequestsAsync();
    }

    private async void LogoutButton_Clicked(
    object sender,
    EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Đăng xuất",
            "Bạn có chắc muốn đăng xuất?",
            "Đăng xuất",
            "Hủy");

        if (!confirm)
            return;

        SessionService.Clear();

        Application.Current!.MainPage =
            new NavigationPage(
                new LoginPage());
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    private async void UpdateLocationButton_Clicked(
    object sender,
    EventArgs e)
    {
        UpdateLocationButton.IsEnabled = false;

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
                    "Ứng dụng cần quyền vị trí để cập nhật GPS.",
                    "OK");

                return;
            }

            if (!Geolocation.Default.IsEnabled)
            {
                await DisplayAlert(
                    "GPS",
                    "Vui lòng bật vị trí trên emulator.",
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

            var result =
                await _apiService.UpdateWorkerLocationAsync(
                    location.Latitude,
                    location.Longitude);

            if (result == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không nhận được kết quả từ server.",
                    "OK");

                return;
            }

            LocationLabel.Text =
                $"Vĩ độ: {result.Latitude:F6}\n" +
                $"Kinh độ: {result.Longitude:F6}\n" +
                $"Cập nhật: {result.UpdatedAt.ToLocalTime():HH:mm:ss}";

            await DisplayAlert(
                "Thành công",
                "Đã cập nhật vị trí Worker.",
                "OK");

            await LoadRequestsAsync();
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
            UpdateLocationButton.IsEnabled = true;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private async void Notifications_Clicked(
    object sender,
    EventArgs e)
    {
        await Navigation.PushAsync(
            new NotificationPage());
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private async void Profile_Clicked(
    object sender,
    EventArgs e)
    {
        await Navigation.PushAsync(
            new WorkerProfilePage());
    }











}