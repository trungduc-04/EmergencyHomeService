using System.Security.Claims;
using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.Worker;
using EmergencyHomeService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "WORKER")]
public class WorkersController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;

    public WorkersController(EmergencyHomeServiceDbContext context)
    {
        _context = context;
    }

    private bool TryGetWorkerId(out int workerId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out workerId);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!TryGetWorkerId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var worker = await _context.WorkerProfiles
            .AsNoTracking()
            .Include(w => w.WorkerServices)
                .ThenInclude(ws => ws.Service)
            .Include(w => w.Worker)
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        return Ok(new WorkerProfileResponse
        {
            WorkerId = worker.WorkerId,
            FullName = worker.Worker.FullName,
            Email = worker.Worker.Email ?? string.Empty,
            Phone = worker.Worker.Phone,
            AvatarUrl = worker.Worker.AvatarUrl,

            Bio = worker.Bio,
            ExperienceYears = worker.ExperienceYears,
            IsOnline = worker.IsOnline,
            IsAvailable = worker.IsAvailable,
            AverageRating = worker.AverageRating,
            TotalJobs = worker.TotalJobs,
            IsVerified = worker.IsVerified,

            Services = worker.WorkerServices
                .Select(ws => new ServiceResponse
                {
                    ServiceId = ws.Service.ServiceId,
                    ServiceName = ws.Service.ServiceName
                })
                .ToList()
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateWorkerProfileRequest request)
    {
        if (!TryGetWorkerId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        if (request.ExperienceYears < 0)
        {
            return BadRequest(new
            {
                message = "Số năm kinh nghiệm không hợp lệ."
            });
        }

        var worker = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        worker.Bio = string.IsNullOrWhiteSpace(request.Bio)
            ? null
            : request.Bio.Trim();

        worker.ExperienceYears = request.ExperienceYears;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Cập nhật hồ sơ thợ thành công."
        });
    }

    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability(
        [FromBody] UpdateAvailabilityRequest request)
    {
        if (!TryGetWorkerId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var worker = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        worker.IsOnline = request.IsOnline;
        worker.IsAvailable = request.IsOnline && request.IsAvailable;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            isOnline = worker.IsOnline,
            isAvailable = worker.IsAvailable
        });
    }

    [HttpPut("services")]
    public async Task<IActionResult> UpdateServices(
        [FromBody] UpdateWorkerServicesRequest request)
    {
        if (!TryGetWorkerId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var workerExists = await _context.WorkerProfiles
            .AnyAsync(w => w.WorkerId == workerId);

        if (!workerExists)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        var serviceIds = request.ServiceIds
            .Distinct()
            .ToList();

        if (serviceIds.Count == 0)
        {
            return BadRequest(new
            {
                message = "Phải chọn ít nhất một dịch vụ."
            });
        }

        var validServiceIds = await _context.Services
            .Where(s => s.IsActive && serviceIds.Contains(s.ServiceId))
            .Select(s => s.ServiceId)
            .ToListAsync();

        if (validServiceIds.Count != serviceIds.Count)
        {
            return BadRequest(new
            {
                message = "Có dịch vụ không tồn tại hoặc đã bị vô hiệu hóa."
            });
        }

        var oldServices = await _context.WorkerServices
            .Where(ws => ws.WorkerId == workerId)
            .ToListAsync();

        _context.WorkerServices.RemoveRange(oldServices);

        var newServices = validServiceIds
            .Select(serviceId => new WorkerService
            {
                WorkerId = workerId,
                ServiceId = serviceId,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        await _context.WorkerServices.AddRangeAsync(newServices);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Cập nhật dịch vụ thành công.",
            serviceIds = validServiceIds
        });
    }
}