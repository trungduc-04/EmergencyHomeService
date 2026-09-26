using EmergencyHomeService.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;

    public AdminController(
        EmergencyHomeServiceDbContext context)
    {
        _context = context;
    }

    // =========================
    // DASHBOARD
    // =========================

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = new
        {
            totalUsers = await _context.Users.CountAsync(),

            customerCount = await _context.Users
                .CountAsync(x => x.Role == "CUSTOMER"),

            workerCount = await _context.Users
                .CountAsync(x => x.Role == "WORKER"),

            adminCount = await _context.Users
                .CountAsync(x => x.Role == "ADMIN"),

            activeUsers = await _context.Users
                .CountAsync(x => x.IsActive),

            totalWorkers = await _context.WorkerProfiles.CountAsync(),

            verifiedWorkers = await _context.WorkerProfiles
                .CountAsync(x => x.IsVerified),

            onlineWorkers = await _context.WorkerProfiles
                .CountAsync(x => x.IsOnline),

            availableWorkers = await _context.WorkerProfiles
                .CountAsync(x =>
                    x.IsOnline &&
                    x.IsAvailable),

            totalRequests = await _context.ServiceRequests
                .CountAsync(),

            searchingRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "SEARCHING"),

            acceptedRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "ACCEPTED"),

            onTheWayRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "ON_THE_WAY"),

            arrivedRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "ARRIVED"),

            inProgressRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "IN_PROGRESS"),

            completedRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "COMPLETED"),

            cancelledRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == "CANCELLED"),

            totalServices = await _context.Services.CountAsync(),

            activeServices = await _context.Services
                .CountAsync(x => x.IsActive),

            totalRatings = await _context.Ratings.CountAsync()
        };

        return Ok(result);
    }

    // =========================
    // USERS
    // =========================

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                userId = x.UserId,
                fullName = x.FullName,
                email = x.Email,
                phone = x.Phone,
                role = x.Role,
                isActive = x.IsActive,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // =========================
    // BLOCK / UNBLOCK USER
    // =========================

    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        int id,
        [FromBody] UserStatusRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy người dùng."
            });
        }

        if (user.Role == "ADMIN")
        {
            return BadRequest(new
            {
                message = "Không thay đổi trạng thái tài khoản ADMIN."
            });
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            userId = user.UserId,
            isActive = user.IsActive
        });
    }

    // =========================
    // WORKERS
    // =========================

    [HttpGet("workers")]
    public async Task<IActionResult> GetWorkers()
    {
        var workers = await _context.WorkerProfiles
            .AsNoTracking()
            .Include(x => x.Worker)
            .Include(x => x.WorkerServices)
                .ThenInclude(x => x.Service)
            .OrderByDescending(x => x.Worker.IsActive)
            .ThenByDescending(x => x.IsVerified)
            .Select(x => new
            {
                workerId = x.WorkerId,
                fullName = x.Worker.FullName,
                email = x.Worker.Email,
                phone = x.Worker.Phone,

                isActive = x.Worker.IsActive,
                isOnline = x.IsOnline,
                isAvailable = x.IsAvailable,

                isVerified = x.IsVerified,

                experienceYears = x.ExperienceYears,
                averageRating = x.AverageRating,
                totalJobs = x.TotalJobs,

                services = x.WorkerServices
                    .Select(ws => ws.Service.ServiceName)
                    .ToList()
            })
            .ToListAsync();

        return Ok(workers);
    }

    // =========================
    // VERIFY / UNVERIFY WORKER
    // =========================

    [HttpPut("workers/{id:int}/verify")]
    public async Task<IActionResult> VerifyWorker(
        int id,
        [FromBody] WorkerVerifyRequest request)
    {
        var worker = await _context.WorkerProfiles
            .FirstOrDefaultAsync(x => x.WorkerId == id);

        if (worker == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy Worker."
            });
        }

        worker.IsVerified = request.IsVerified;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            workerId = worker.WorkerId,
            isVerified = worker.IsVerified
        });
    }

    // =========================
    // REQUESTS
    // =========================

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests()
    {
        var requests = await _context.ServiceRequests
            .AsNoTracking()
            .Include(x => x.Customer)
                .ThenInclude(x => x.Customer)
            .Include(x => x.Worker)
                .ThenInclude(x => x.Worker)
            .Include(x => x.Service)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => new
            {
                requestId = x.RequestId,

                customerId = x.CustomerId,
                customerName =
                    x.Customer != null &&
                    x.Customer.Customer != null
                        ? x.Customer.Customer.FullName
                        : null,

                workerId = x.WorkerId,
                
                workerName =
                    x.Worker != null &&
                    x.Worker.Worker != null
                        ? x.Worker.Worker.FullName
                        : null,

                serviceId = x.ServiceId,
                serviceName = x.Service.ServiceName,

                description = x.Description,

                status = x.Status,

                address = x.Address,

                latitude = x.Latitude,
                longitude = x.Longitude,

                createdAt = x.CreatedAt,
                acceptedAt = x.AcceptedAt,
                completedAt = x.CompletedAt,
                cancelledAt = x.CancelledAt,

                ratingScore = x.Rating != null
                    ? x.Rating.Score
                    : (int?)null
            })
            .ToListAsync();

        return Ok(requests);
    }

    // =========================
    // SERVICES
    // =========================

    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var services = await _context.Services
            .AsNoTracking()
            .OrderBy(x => x.ServiceName)
            .Select(x => new
            {
                serviceId = x.ServiceId,
                serviceName = x.ServiceName,
                description = x.Description,
                isActive = x.IsActive,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(services);
    }

    // =========================
    // ENABLE / DISABLE SERVICE
    // =========================

    [HttpPut("services/{id:int}/status")]
    public async Task<IActionResult> UpdateServiceStatus(
        int id,
        [FromBody] ServiceStatusRequest request)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(x => x.ServiceId == id);

        if (service == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy dịch vụ."
            });
        }

        service.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            serviceId = service.ServiceId,
            isActive = service.IsActive
        });
    }
}

// DTO nhỏ dùng riêng cho AdminController

public class UserStatusRequest
{
    public bool IsActive { get; set; }
}

public class WorkerVerifyRequest
{
    public bool IsVerified { get; set; }
}

public class ServiceStatusRequest
{
    public bool IsActive { get; set; }
}