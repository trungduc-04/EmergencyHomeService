using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Worker;

public partial class WorkerRatingsPage : ContentPage
{
    private readonly ApiService _apiService;

    public WorkerRatingsPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var ratings =
                await _apiService.GetMyRatingsAsync();

            RatingsCollectionView.ItemsSource =
                ratings;
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