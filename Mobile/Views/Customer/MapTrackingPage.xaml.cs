
using EmergencyHomeServiceMobile.Models;
using EmergencyHomeServiceMobile.Services;

namespace EmergencyHomeServiceMobile.Views.Customer;

public partial class MapTrackingPage : ContentPage
{
    private readonly SignalRService _signalRService;

    private bool _mapReady = false;

    public MapTrackingPage()
    {
        InitializeComponent();

        _signalRService = new SignalRService();

        _signalRService.WorkerLocationReceived +=
            OnWorkerLocationReceived;

        MapWebView.Navigated +=
            MapWebView_Navigated;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await LoadMapAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text =
                "Không thể tải bản đồ: " + ex.Message;
        }

        try
        {
            await _signalRService.StartAsync();

            StatusLabel.Text =
                "Đã kết nối realtime.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text =
                "SignalR lỗi: " + ex.Message;
        }
    }

    private async Task LoadMapAsync()
    {
        double latitude = 10.772622;
        double longitude = 106.660172;

        try
        {
            var location =
                await Geolocation.Default.GetLastKnownLocationAsync();

            if (location != null)
            {
                latitude = location.Latitude;
                longitude = location.Longitude;
            }
        }
        catch
        {
            // Nếu không lấy được GPS thì dùng tọa độ mặc định
        }

        string html = """
<!DOCTYPE html>
<html>
<head>

    <meta
        name="viewport"
        content="width=device-width,
                 initial-scale=1.0,
                 maximum-scale=1.0,
                 user-scalable=no" />

    <link
        rel="stylesheet"
        href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />

    <script
        src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js">
    </script>

    <style>
        html, body {
            margin: 0;
            padding: 0;
            width: 100%;
            height: 100%;
        }

        #map {
            width: 100%;
            height: 100%;
        }
    </style>

</head>

<body>

<div id="map"></div>

<script>

    var customerLatitude = __LATITUDE__;
    var customerLongitude = __LONGITUDE__;

    var map = L.map('map')
        .setView(
            [customerLatitude, customerLongitude],
            15
        );

    L.tileLayer(
        'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
        {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }
    ).addTo(map);

    var customerIcon = L.divIcon({
        html: '📍',
        className: '',
        iconSize: [32, 32],
        iconAnchor: [16, 16]
    });

    var workerIcon = L.divIcon({
        html: '🚗',
        className: '',
        iconSize: [36, 36],
        iconAnchor: [18, 18]
    });

    var customerMarker = L.marker(
        [customerLatitude, customerLongitude],
        {
            icon: customerIcon
        }
    ).addTo(map);

    customerMarker.bindPopup('Vị trí của bạn');

    var workerMarker = null;

    function updateWorkerLocation(lat, lng)
    {
        if (workerMarker === null)
        {
            workerMarker = L.marker(
                [lat, lng],
                {
                    icon: workerIcon
                }
            ).addTo(map);

            workerMarker.bindPopup('Thợ sửa chữa');

            map.setView([lat, lng], 15);
        }
        else
        {
            workerMarker.setLatLng([lat, lng]);
        }
    }

</script>

</body>
</html>
""";

        html = html.Replace(
            "__LATITUDE__",
            latitude.ToString(
                System.Globalization.CultureInfo.InvariantCulture));

        html = html.Replace(
            "__LONGITUDE__",
            longitude.ToString(
                System.Globalization.CultureInfo.InvariantCulture));

        MapWebView.Source =
            new HtmlWebViewSource
            {
                Html = html
            };
    }

    private void MapWebView_Navigated(
        object? sender,
        WebNavigatedEventArgs e)
    {
        _mapReady = true;
    }

    private async void OnWorkerLocationReceived(
        WorkerLocationResponse location)
    {
        await MainThread.InvokeOnMainThreadAsync(
            async () =>
            {
                if (!_mapReady)
                    return;

                LocationLabel.Text =
                    $"Vị trí Worker: " +
                    $"{location.Latitude:F6}, " +
                    $"{location.Longitude:F6}";

                await MapWebView.EvaluateJavaScriptAsync(
                    $"updateWorkerLocation(" +
                    $"{location.Latitude.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"{location.Longitude.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)})");
            });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        _signalRService.WorkerLocationReceived -=
            OnWorkerLocationReceived;

        try
        {
            await _signalRService.StopAsync();
        }
        catch
        {
        }
    }
}