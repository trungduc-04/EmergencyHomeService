using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EmergencyHomeService.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}