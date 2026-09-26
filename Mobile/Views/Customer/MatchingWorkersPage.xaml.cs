using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class MatchingWorkersPage : ContentPage
{
    private readonly ApiService _apiService;

    private readonly SignalRService _signalRService;

    private readonly long _requestId;

    private CancellationTokenSource? _refreshCancellation;

    public MatchingWorkersPage(long requestId)
    {
        InitializeComponent();

        _apiService = new ApiService();

        _signalRService = new SignalRService();

        _requestId = requestId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _signalRService.NotificationReceived -=
            OnNotificationReceived;

        _signalRService.NotificationReceived +=
            OnNotificationReceived;

        try
        {
            await _signalRService.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                "SignalR connection error: " + ex);
        }

        await LoadMatchingWorkersAsync();
    }
    //////////////////////////////////////////////////////////////////////////////
    private async Task AutoRefreshAsync(
    CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                await LoadMatchingWorkersAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Bình thường khi rời khỏi trang
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                "Auto refresh error: " + ex);
        }
    }
    ////////////////////////////////////////////////////////////////
    private string GetStatusText(string status)
    {
        return status switch
        {
            "SEARCHING" => "Đang tìm thợ",
            "ACCEPTED" => "Thợ đã nhận yêu cầu",
            "ON_THE_WAY" => "Thợ đang trên đường",
            "ARRIVED" => "Thợ đã đến",
            "IN_PROGRESS" => "Đang sửa chữa",
            "COMPLETED" => "Đã hoàn thành",
            "CANCELLED" => "Đã hủy",
            _ => status
        };
    }
    ////////////////////////////////////////////////////////////////

    private async Task LoadMatchingWorkersAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var result =
                await _apiService.GetMatchingWorkersAsync(_requestId);

            if (result == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không nhận được dữ liệu từ server.",
                    "OK");

                return;
            }

            StatusLabel.Text =
                $"Trạng thái yêu cầu: {GetStatusText(result.Status)}";

            bool isCompleted =
                result.Status.Equals(
                    "COMPLETED",
                    StringComparison.OrdinalIgnoreCase);

            bool alreadyRated =
                Preferences.Get(
                    $"rated_request_{_requestId}",
                    false);

            RateButton.IsVisible =
                isCompleted && !alreadyRated;

            RadiusLabel.Text =
                $"Bán kính tìm kiếm: {result.SearchRadiusKm:F1} km\n" +
                $"Tìm thấy: {result.Total} thợ";

            WorkersCollectionView.ItemsSource =
                result.Workers;
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
    //////////////////////////////////////////////////////////////////////
    private async void OnNotificationReceived(
    string title,
    string message,
    string type)
    {
        await MainThread.InvokeOnMainThreadAsync(
            async () =>
            {
                // Khi backend gửi notification,
                // tải lại trạng thái request ngay lập tức.
                await LoadMatchingWorkersAsync();
            });
    }

    ////////////////////////////////////////////////////////////////////
    private async void RefreshButton_Clicked(
        object sender,
        EventArgs e)
    {
        await LoadMatchingWorkersAsync();
    }

    private async void BackButton_Clicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PopAsync();
    }

    ///////////////////////////////////////////////////////////////////////
    private async void OpenMap_Clicked(
    object sender,
    EventArgs e)
    {
        await Navigation.PushAsync(
            new MapTrackingPage());
    }
    ///////////////////////////////////////////////////////////////////////
    private async void RateButton_Clicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new RatingPage(
                _requestId,
                "Thợ sửa chữa"));
    }
    /////////////////////////////////////////////////////////////////////

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        // Dừng auto refresh
        _refreshCancellation?.Cancel();
        _refreshCancellation?.Dispose();
        _refreshCancellation = null;

        // Ngắt nhận notification của trang này
        _signalRService.NotificationReceived -=
            OnNotificationReceived;

        // Ngắt kết nối SignalR
        try
        {
            await _signalRService.StopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                "SignalR stop error: " + ex);
        }
    }




}