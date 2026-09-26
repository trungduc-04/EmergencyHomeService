using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Worker;

public partial class WorkerProfilePage : ContentPage
{
    private readonly ApiService _apiService;

    private WorkerProfileResponse? _profile;

    private readonly List<(int Id, CheckBox Box)>
        _serviceBoxes = new();

    public WorkerProfilePage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            _profile =
                await _apiService
                    .GetMyWorkerProfileAsync();

            if (_profile == null)
                return;

            NameEntry.Text =
                _profile.FullName;

            EmailEntry.Text =
                _profile.Email;

            PhoneEntry.Text =
                _profile.Phone;

            AvatarEntry.Text =
                _profile.AvatarUrl;

            RatingLabel.Text =
                $"Đánh giá: {_profile.AverageRating:F2}/5";

            JobsLabel.Text =
                $"Số công việc: {_profile.TotalJobs}";

            VerifiedLabel.Text =
                _profile.IsVerified
                    ? "Đã xác minh"
                    : "Chưa xác minh";

            BioEditor.Text =
                _profile.Bio;

            ExperienceEntry.Text =
                _profile.ExperienceYears.ToString();

            await LoadServiceSelectionAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
    }

    private async Task LoadServiceSelectionAsync()
    {
        var services =
            await _apiService.GetServicesAsync();

        if (services == null)
            return;

        ServicesLayout.Children.Clear();
        _serviceBoxes.Clear();

        var selectedIds =
            _profile?.Services
                .Select(x => x.ServiceId)
                .ToHashSet()
            ?? new HashSet<int>();

        foreach (var service in services)
        {
            var checkBox = new CheckBox
            {
                IsChecked =
                    selectedIds.Contains(
                        service.ServiceId)
            };

            var label = new Label
            {
                Text = service.ServiceName,
                VerticalOptions =
                    LayoutOptions.Center
            };

            var row =
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        checkBox,
                        label
                    }
                };

            ServicesLayout.Children.Add(row);

            _serviceBoxes.Add(
                (service.ServiceId, checkBox));
        }
    }

    private async void SaveProfile_Clicked(
    object sender,
    EventArgs e)
    {
        if (!int.TryParse(
            ExperienceEntry.Text,
            out int experience))
        {
            experience = 0;
        }

        if (experience < 0)
        {
            await DisplayAlert(
                "Thông báo",
                "Số năm kinh nghiệm không được âm.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert(
                "Thông báo",
                "Họ tên không được để trống.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert(
                "Thông báo",
                "Email không được để trống.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            await DisplayAlert(
                "Thông báo",
                "Số điện thoại không được để trống.",
                "OK");

            return;
        }

        try
        {
            // 1. Cập nhật thông tin tài khoản
            await _apiService.UpdateMyProfileAsync(
                NameEntry.Text.Trim(),
                PhoneEntry.Text.Trim(),
                AvatarEntry.Text?.Trim());

            // 2. Cập nhật thông tin nghề nghiệp
            await _apiService.UpdateWorkerProfileAsync(
                BioEditor.Text?.Trim(),
                experience);

            // 3. Cập nhật lại session
            Preferences.Set(
                "full_name",
                NameEntry.Text.Trim());

            Preferences.Set(
                "phone",
                PhoneEntry.Text.Trim());

            Preferences.Set(
                "email",
                EmailEntry.Text.Trim());

            await DisplayAlert(
                "Thành công",
                "Đã cập nhật hồ sơ.",
                "OK");

            await LoadProfileAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
    }

    private async void SaveServices_Clicked(
        object sender,
        EventArgs e)
    {
        var selectedIds =
            _serviceBoxes
                .Where(x => x.Box.IsChecked)
                .Select(x => x.Id)
                .ToList();

        if (selectedIds.Count == 0)
        {
            await DisplayAlert(
                "Thông báo",
                "Phải chọn ít nhất một dịch vụ.",
                "OK");

            return;
        }

        try
        {
            await _apiService
                .UpdateWorkerServicesAsync(
                    selectedIds);

            await DisplayAlert(
                "Thành công",
                "Đã cập nhật dịch vụ.",
                "OK");

            await LoadProfileAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Lỗi",
                ex.Message,
                "OK");
        }
    }

    private async void ViewRatings_Clicked(
        object sender,
        EventArgs e)
    {
        await Navigation.PushAsync(
            new WorkerRatingsPage());
    }
}