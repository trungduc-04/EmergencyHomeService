using EmergencyHomeService.API.Models;

namespace EmergencyHomeService.API.Services;

public interface INotificationService
{
    Task SendNotificationAsync(
        int userId,
        long? requestId,
        string title,
        string message,
        string type);
}