using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class RatingPage : ContentPage
{
    private readonly ApiService _apiService;

    private readonly long _requestId;

    private int _score = 0;

    public RatingPage(long requestId, string workerName)
    {
        InitializeComponent();

        _apiService = new ApiService();

        _requestId = requestId;

        WorkerNameLabel.Text =
            $"Đánh giá: {workerName}";
    }

    private void StarButton_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button == Star1Button)
            _score = 1;
        else if (button == Star2Button)
            _score = 2;
        else if (button == Star3Button)
            _score = 3;
        else if (button == Star4Button)
            _score = 4;
        else if (button == Star5Button)
            _score = 5;

        ScoreLabel.Text =
            $"Bạn đã chọn {_score}/5 sao";
    }

    private async void SubmitButton_Clicked(
    object sender,
    EventArgs e)
    {
        if (_score < 1 || _score > 5)
        {
            await DisplayAlert(
                "Thiếu đánh giá",
                "Vui lòng chọn từ 1 đến 5 sao.",
                "OK");

            return;
        }

        string comment =
            CommentEditor.Text?.Trim() ?? "";

        bool confirm = await DisplayAlert(
            "Xác nhận",
            $"Bạn chấm {_score}/5 sao cho công việc này?",
            "Gửi",
            "Hủy");

        if (!confirm)
            return;

        SubmitButton.IsEnabled = false;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var success =
                await _apiService.CreateRatingAsync(
                    _requestId,
                    _score,
                    comment);

            if (!success)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không thể gửi đánh giá.",
                    "OK");

                return;
            }

            await DisplayAlert(
                "Thành công",
                "Đánh giá của bạn đã được gửi.",
                "OK");

            Preferences.Set(
                $"rated_request_{_requestId}",
                true);

            await Navigation.PopAsync();
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert(
                "Không thể gửi đánh giá",
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
            SubmitButton.IsEnabled = true;

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}