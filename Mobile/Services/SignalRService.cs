using EmergencyHomeServiceMobile.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace EmergencyHomeServiceMobile.Services;

public class SignalRService
{
    private readonly HubConnection _connection;

    public event Action<string, string, string>? NotificationReceived;

    public event Action<WorkerLocationResponse>? WorkerLocationReceived;

    public bool IsConnected =>
        _connection.State == HubConnectionState.Connected;

    public SignalRService()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(
                "http://10.0.2.2:5127/hubs/notifications",
                options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        var token =
                            Preferences.Get("jwt_token", "");

                        return Task.FromResult<string?>(
                            token);
                    };
                })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<object>(
            "ReceiveNotification",
            notification =>
            {
                try
                {
                    var json =
                        System.Text.Json.JsonSerializer.Serialize(
                            notification);

                    var document =
                        System.Text.Json.JsonDocument.Parse(json);

                    string title =
                        document.RootElement.TryGetProperty(
                            "title",
                            out var titleProperty)
                            ? titleProperty.GetString() ?? ""
                            : "";

                    string message =
                        document.RootElement.TryGetProperty(
                            "message",
                            out var messageProperty)
                            ? messageProperty.GetString() ?? ""
                            : "";

                    string type =
                        document.RootElement.TryGetProperty(
                            "type",
                            out var typeProperty)
                            ? typeProperty.GetString() ?? ""
                            : "";

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        NotificationReceived?.Invoke(
                            title,
                            message,
                            type);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });

        _connection.On<WorkerLocationResponse>(
            "ReceiveWorkerLocation",
            location =>
            {
                WorkerLocationReceived?.Invoke(location);
            });
    }

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            return;

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
            return;

        await _connection.StopAsync();
    }
}