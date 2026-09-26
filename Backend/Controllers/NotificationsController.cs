using System.Security.Claims;
using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;



    public NotificationsController(
        EmergencyHomeServiceDbContext context)
    {
        _context = context;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out userId);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse
            {
                NotificationId = n.NotificationId,
                RequestId = n.RequestId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var count = await _context.Notifications
            .CountAsync(n =>
                n.UserId == userId &&
                !n.IsRead);

        return Ok(new
        {
            unreadCount = count
        });
    }

    [HttpPut("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.NotificationId == id &&
                n.UserId == userId);

        if (notification == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy thông báo."
            });
        }

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đã đánh dấu thông báo là đã đọc."
        });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var notifications = await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đã đánh dấu tất cả thông báo là đã đọc.",
            updatedCount = notifications.Count
        });
    }

}