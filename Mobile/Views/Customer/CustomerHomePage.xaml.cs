using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;
using EmergencyHomeServiceMobile.Views;
using EmergencyHomeServiceMobile.Views.Auth;
using EmergencyHomeServiceMobile.Views.Customer;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class CustomerHomePage : ContentPage
{
    private readonly ApiService _apiService;

    private readonly SignalRService _signalRService;

    public CustomerHomePage()
    {
        InitializeComponent();

        _apiService = new ApiService();
        _signalRService = new SignalRService();

        string fullName = Preferences.Get(
            "full_name",
            "Khách hàng");

        WelcomeLabel.Text = $"Xin chào, {fullName}";


    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            _signalRService.NotificationReceived -=
                OnNotificationReceived;

            _signalRService.NotificationReceived +=
                OnNotificationReceived;

            await _signalRService.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                "SignalR connection error: " + ex);
        }

        await LoadServicesAsync();
        LoadCurrentRequest();
    }
    /////////////////////////////////////////////////////////////
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        _signalRService.NotificationReceived -=
            OnNotificationReceived;

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
    /////////////////////////////////////////////////////////

    private async Task LoadServicesAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            var services = await _apiService.GetServicesAsync();

            if (services == null)
            {
                await DisplayAlert(
                    "Lỗi",
                    "Không thể tải danh sách dịch vụ.",
                    "OK");

                return;
            }

            ServicesCollectionView.ItemsSource = services;
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

    ///////////////////////////////////////////////////////////////////////////
    private void LoadCurrentRequest()
    {
        long requestId = Preferences.Get(
            "current_request_id",
            0L);

        if (requestId <= 0)
        {
            CurrentRequestBorder.IsVisible = false;
            return;
        }

        CurrentRequestLabel.Text =
            $"Bạn đang theo dõi yêu cầu #{requestId}";

        CurrentRequestBorder.IsVisible = true;
    }

    //////////////////////////////////////////////////////////////////////////
    private async void ViewCurrentRequest_Clicked(
    object sender,
    EventArgs e)
    {
        long requestId = Preferences.Get(
            "current_request_id",
            0L);

        if (requestId <= 0)
        {
            CurrentRequestBorder.IsVisible = false;
            return;
        }

        await Navigation.PushAsync(
            new MatchingWorkersPage(requestId));
    }

    //////////////////////////////////////////////////////////////////////////

    private async void ServicesCollectionView_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        var selectedService =
            e.CurrentSelection.FirstOrDefault() as Service;

        if (selectedService == null)
            return;

        ServicesCollectionView.SelectedItem = null;

        await Navigation.PushAsync(
            new CreateRequestPage(selectedService));
    }

    private async void Logout_Clicked(
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

    private async void OnNotificationReceived(
    string title,
    string message,
    string type)
    {
        await MainThread.InvokeOnMainThreadAsync(
            async () =>
            {
                await DisplayAlert(
                    title,
                    message,
                    "OK");
            });
    }

    /////////////////////////////////////////////////////////
    private async void MyRequests_Clicked(
    object sender,
    EventArgs e)
    {
        await Navigation.PushAsync(
            new MyRequestsPage());
    }

    private async void Notifications_Clicked(
    object sender,
    EventArgs e)
    {
        await Navigation.PushAsync(
            new NotificationPage());
    }
    //////////////////////////////////////////////////////////
    private async void Profile_Clicked(
    object sender,
    EventArgs e)
    {
        await Navigation.PushAsync(
            new CustomerProfilePage());
    }
    ////////////////////////////////////////

}