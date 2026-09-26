using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class RequestDetailPage : ContentPage
{
    private readonly ApiService _apiService;

    private readonly long _requestId;

    private ServiceRequestResponse? _request;

    public RequestDetailPage(long requestId)
    {
        InitializeComponent();

        _apiService = new ApiService();

        _requestId = requestId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadRequestAsync();
    }

    private async Task LoadRequestAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            _request =
                await _apiService.GetRequestDetailAsync(
                    _requestId);

            if (_request == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không tìm thấy yêu cầu.",
                    "OK");

                await Navigation.PopAsync();

                return;
            }

            DisplayRequest();

            if (_request.Status.Equals(
                    "COMPLETED",
                    StringComparison.OrdinalIgnoreCase))
            {
                var rating =
                    await _apiService.GetMyRatingAsync(
                        _requestId);

                if (rating != null)
                {
                    RatingButton.Text =
                        $"ĐÃ ĐÁNH GIÁ ⭐ {rating.Score}/5";

                    RatingButton.IsEnabled = false;

                    RatingButton.IsVisible = true;
                }
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                "Không thể tải chi tiết yêu cầu.\n\n" +
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private void DisplayRequest()
    {
        if (_request == null)
            return;

        RequestTitleLabel.Text =
            $"Yêu cầu #{_request.RequestId}";

        ServiceLabel.Text =
            $"Dịch vụ: {_request.ServiceName}";

        StatusLabel.Text =
            $"Trạng thái: {GetStatusText(_request.Status)}";

        DescriptionLabel.Text =
            _request.Description;

        AddressLabel.Text =
            string.IsNullOrWhiteSpace(_request.Address)
                ? "Chưa cung cấp"
                : _request.Address;

        WorkerLabel.Text =
            _request.WorkerId.HasValue
                ? $"Worker #{_request.WorkerId.Value}"
                : "Chưa có Worker";

        CreatedAtLabel.Text =
            $"Tạo lúc: {_request.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm}";

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (_request == null)
            return;

        string status =
            _request.Status.ToUpperInvariant();

        TrackingButton.IsVisible =
            status == "ACCEPTED" ||
            status == "ON_THE_WAY" ||
            status == "ARRIVED" ||
            status == "IN_PROGRESS";

        CancelButton.IsVisible =
            status != "COMPLETED" &&
            status != "CANCELLED" &&
            status != "IN_PROGRESS";


        RatingButton.IsVisible =
            status == "COMPLETED";
    }

    private string GetStatusText(string status)
    {
        return status.ToUpperInvariant() switch
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

    private async void TrackingButton_Clicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new MapTrackingPage());
    }

    private async void RatingButton_Clicked(
        object sender,
        EventArgs e)
    {
        string workerName =
            _request?.WorkerId.HasValue == true
                ? $"Worker #{_request.WorkerId.Value}"
                : "Thợ sửa chữa";

        await Navigation.PushAsync(
            new RatingPage(
                _requestId,
                workerName));
    }

    private async void CancelButton_Clicked(
        object sender,
        EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Xác nhận hủy",
            "Bạn có chắc muốn hủy yêu cầu này?",
            "Hủy yêu cầu",
            "Không");

        if (!confirm)
            return;

        await DisplayAlert(
            "Thông báo",
            "Phần hủy yêu cầu sẽ được nối với API ở bước tiếp theo.",
            "OK");
    }
}