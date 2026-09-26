using System.Security.Claims;
using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.Location;
using EmergencyHomeService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmergencyHomeService.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "WORKER")]
public class WorkerLocationsController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public WorkerLocationsController(
        EmergencyHomeServiceDbContext context,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    private bool TryGetWorkerId(out int workerId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out workerId);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateLocation(
        [FromBody] UpdateWorkerLocationRequest request)
    {
        if (!TryGetWorkerId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        if (request.Latitude < -90 || request.Latitude > 90)
        {
            return BadRequest(new
            {
                message = "Latitude không hợp lệ."
            });
        }

        if (request.Longitude < -180 || request.Longitude > 180)
        {
            return BadRequest(new
            {
                message = "Longitude không hợp lệ."
            });
        }

        var workerExists = await _context.WorkerProfiles
            .AnyAsync(x => x.WorkerId == workerId);

        if (!workerExists)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        var location = await _context.WorkerLocations
            .FirstOrDefaultAsync(x => x.WorkerId == workerId);

        if (location == null)
        {
            location = new WorkerLocation
            {
                WorkerId = workerId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkerLocations.Add(location);
        }
        else
        {
            location.Latitude = request.Latitude;
            location.Longitude = request.Longitude;
            location.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Tìm các đơn đang được Worker thực hiện
        var activeRequests = await _context.ServiceRequests
            .AsNoTracking()
            .Where(r =>
                r.WorkerId == workerId &&
                (
                    r.Status == "ACCEPTED" ||
                    r.Status == "ON_THE_WAY" ||
                    r.Status == "ARRIVED" ||
                    r.Status == "IN_PROGRESS"
                ))
            .Select(r => new
            {
                r.RequestId,
                r.CustomerId,
                r.Status
            })
            .ToListAsync();

        // Gửi vị trí realtime tới Customer của từng đơn
        foreach (var activeRequest in activeRequests)
        {
            await _hubContext.Clients
                .User(activeRequest.CustomerId.ToString())
                .SendAsync(
                    "ReceiveWorkerLocation",
                    new
                    {
                        requestId = activeRequest.RequestId,
                        workerId,
                        status = activeRequest.Status,
                        latitude = location.Latitude,
                        longitude = location.Longitude,
                        updatedAt = location.UpdatedAt
                    });
        }

        return Ok(new
        {
            success = true,
            message = "Cập nhật vị trí thành công.",
            workerId,
            latitude = location.Latitude,
            longitude = location.Longitude,
            updatedAt = location.UpdatedAt,
            activeRequestCount = activeRequests.Count
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyLocation()
    {
        if (!TryGetWorkerId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var location = await _context.WorkerLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkerId == workerId);

        if (location == null)
        {
            return NotFound(new
            {
                message = "Worker chưa có vị trí."
            });
        }

        return Ok(new
        {
            workerId = location.WorkerId,
            latitude = location.Latitude,
            longitude = location.Longitude,
            updatedAt = location.UpdatedAt
        });
    }
}