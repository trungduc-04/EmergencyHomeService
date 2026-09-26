using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class MyRequestsPage : ContentPage
{
    private readonly ApiService _apiService;

    public MyRequestsPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadRequestsAsync();
    }

    private async Task LoadRequestsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var requests =
                await _apiService.GetMyRequestsAsync();

            RequestsCollectionView.ItemsSource =
                requests;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                "Không thể tải danh sách yêu cầu.\n\n" +
                ex.Message,
                "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void ViewRequest_Clicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.CommandParameter
            is not ServiceRequestResponse request)
            return;

        await Navigation.PushAsync(
            new RequestDetailPage(request.RequestId));
    }
}