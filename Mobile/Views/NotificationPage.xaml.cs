using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views;

public partial class NotificationPage : ContentPage
{
    private readonly ApiService _apiService;

    public NotificationPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var notifications =
                await _apiService.GetNotificationsAsync();

            NotificationsCollectionView.ItemsSource =
                notifications;

            var unreadCount =
                await _apiService
                    .GetUnreadNotificationCountAsync();

            UnreadLabel.Text =
                $"Chưa đọc: {unreadCount}";
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                "Không thể tải thông báo.\n\n" +
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void MarkRead_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter
            is not NotificationResponse notification)
            return;

        try
        {
            await _apiService
                .MarkNotificationAsReadAsync(
                    notification.NotificationId);

            await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
    }

    private async void MarkAllRead_Clicked(
        object sender,
        EventArgs e)
    {
        try
        {
            await _apiService
                .MarkAllNotificationsAsReadAsync();

            await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
    }
}