using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.Hubs;
using EmergencyHomeService.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace EmergencyHomeService.API.Services;

public class NotificationService : INotificationService
{
    private readonly EmergencyHomeServiceDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        EmergencyHomeServiceDbContext context,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(
        int userId,
        long? requestId,
        string title,
        string message,
        string type)
    {
        var notification = new Notification
        {
            UserId = userId,
            RequestId = requestId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .User(userId.ToString())
            .SendAsync(
                "ReceiveNotification",
                new
                {
                    notificationId = notification.NotificationId,
                    requestId = notification.RequestId,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    isRead = notification.IsRead,
                    createdAt = notification.CreatedAt
                });
    }
}