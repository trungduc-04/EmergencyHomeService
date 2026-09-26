using System.Security.Claims;
using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.ServiceRequest;
using EmergencyHomeService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmergencyHomeService.API.DTOs.Matching;
using EmergencyHomeService.API.Services;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;

    private readonly INotificationService _notificationService;

    public ServiceRequestsController(
        EmergencyHomeServiceDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out userId);
    }

    [Authorize(Roles = "CUSTOMER")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceRequestRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        if (request.ServiceId <= 0)
        {
            return BadRequest(new
            {
                message = "ServiceId không hợp lệ."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new
            {
                message = "Mô tả sự cố không được để trống."
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

        var customerExists = await _context.CustomerProfiles
            .AnyAsync(x => x.CustomerId == userId);

        if (!customerExists)
        {
            return BadRequest(new
            {
                message = "Tài khoản chưa có hồ sơ khách hàng."
            });
        }

        var serviceExists = await _context.Services
            .AnyAsync(x =>
                x.ServiceId == request.ServiceId &&
                x.IsActive);

        if (!serviceExists)
        {
            return BadRequest(new
            {
                message = "Dịch vụ không tồn tại hoặc đã bị vô hiệu hóa."
            });
        }

        var serviceRequest = new ServiceRequest
        {
            CustomerId = userId,
            WorkerId = null,
            ServiceId = request.ServiceId,  

            Description = request.Description.Trim(),

            Status = "SEARCHING",

            Latitude = request.Latitude,
            Longitude = request.Longitude,

            Address = string.IsNullOrWhiteSpace(request.Address)
                ? null
                : request.Address.Trim(),

            ScheduledAt = request.ScheduledAt,

            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);

        await _context.SaveChangesAsync();

        var history = new RequestStatusHistory
        {
            RequestId = serviceRequest.RequestId,
            Status = "SEARCHING",
            Note = "Khách hàng đã tạo yêu cầu. Hệ thống bắt đầu tìm thợ.",
            ChangedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        var service = await _context.Services
            .AsNoTracking()
            .FirstAsync(x => x.ServiceId == request.ServiceId);

        return Ok(new ServiceRequestResponse
        {
            RequestId = serviceRequest.RequestId,
            CustomerId = serviceRequest.CustomerId,
            WorkerId = serviceRequest.WorkerId,
            ServiceId = serviceRequest.ServiceId,
            ServiceName = service.ServiceName,
            Description = serviceRequest.Description,
            Status = serviceRequest.Status,
            Latitude = serviceRequest.Latitude,
            Longitude = serviceRequest.Longitude,
            Address = serviceRequest.Address,
            ScheduledAt = serviceRequest.ScheduledAt,
            CreatedAt = serviceRequest.CreatedAt
        });
    }

    [Authorize(Roles = "CUSTOMER")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var requests = await _context.ServiceRequests
            .AsNoTracking()
            .Where(x => x.CustomerId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ServiceRequestResponse
            {
                RequestId = x.RequestId,
                CustomerId = x.CustomerId,
                WorkerId = x.WorkerId,
                ServiceId = x.ServiceId,
                ServiceName = x.Service.ServiceName,
                Description = x.Description,
                Status = x.Status,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Address = x.Address,
                ScheduledAt = x.ScheduledAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [Authorize(Roles = "CUSTOMER")]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetDetail(long id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var request = await _context.ServiceRequests
            .AsNoTracking()
            .Where(x =>
                x.RequestId == id &&
                x.CustomerId == userId)
            .Select(x => new ServiceRequestResponse
            {
                RequestId = x.RequestId,
                CustomerId = x.CustomerId,
                WorkerId = x.WorkerId,
                ServiceId = x.ServiceId,
                ServiceName = x.Service.ServiceName,
                Description = x.Description,
                Status = x.Status,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Address = x.Address,
                ScheduledAt = x.ScheduledAt,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        return Ok(request);
    }

    [Authorize(Roles = "CUSTOMER")]
    [HttpPut("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var request = await _context.ServiceRequests
            .FirstOrDefaultAsync(x =>
                x.RequestId == id &&
                x.CustomerId == userId);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        // Không cho hủy request đã hoàn thành
        if (request.Status == "COMPLETED")
        {
            return BadRequest(new
            {
                message = "Yêu cầu đã hoàn thành, không thể hủy."
            });
        }

        // Không cho hủy lần 2
        if (request.Status == "CANCELLED")
        {
            return BadRequest(new
            {
                message = "Yêu cầu đã được hủy trước đó."
            });
        }

        // Không cho khách hủy trực tiếp khi thợ đang sửa
        if (request.Status == "IN_PROGRESS")
        {
            return BadRequest(new
            {
                message = "Yêu cầu đang được sửa chữa, không thể hủy trực tiếp."
            });
        }

        // ==============================
        // 1. Cập nhật ServiceRequest
        // ==============================

        request.Status = "CANCELLED";
        request.CancelledAt = DateTime.UtcNow;
        request.CancellationReason = "Khách hàng hủy yêu cầu.";

        // ==============================
        // 2. Ghi lịch sử trạng thái
        // ==============================

        var history = new RequestStatusHistory
        {
            RequestId = request.RequestId,
            Status = "CANCELLED",
            Note = request.CancellationReason,
            ChangedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestStatusHistories.Add(history);

        // ==============================
        // 3. Tạo Notification cho Worker
        // ==============================

        if (request.WorkerId != null)
        {
            var worker = await _context.WorkerProfiles
                .FirstOrDefaultAsync(w => w.WorkerId == request.WorkerId.Value);

            if (worker != null)
            {
                worker.IsAvailable = true;
            }

        }

        // ==============================
        // 4. Lưu tất cả
        // ==============================

        await _context.SaveChangesAsync();

        if (request.WorkerId != null)
        {
            await _notificationService.SendNotificationAsync(
                request.WorkerId.Value,
                request.RequestId,
                "Khách hàng đã hủy yêu cầu",
                "Khách hàng đã hủy yêu cầu sửa chữa.",
                "REQUEST_CANCELLED");
        }

        return Ok(new
        {
            success = true,
            message = "Đã hủy yêu cầu.",
            requestId = request.RequestId,
            status = request.Status
        });
    }

    [Authorize(Roles = "CUSTOMER")]
    [HttpGet("{id:long}/matching-workers")]
    public async Task<IActionResult> GetMatchingWorkers(long id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var request = await _context.ServiceRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.RequestId == id &&
                x.CustomerId == userId);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        if (request.Status != "SEARCHING")
        {
            return BadRequest(new
            {
                message = "Yêu cầu hiện không ở trạng thái SEARCHING."
            });
        }

        const double maxRadiusKm = 10.0;

        var candidates = await _context.WorkerProfiles
            .AsNoTracking()
            .Include(w => w.Worker)
            .Include(w => w.WorkerServices)
                .ThenInclude(ws => ws.Service)
            .Include(w => w.WorkerLocation)
            .Where(w =>
                w.IsOnline &&
                w.IsAvailable &&
                w.Worker.IsActive &&
                w.WorkerServices.Any(ws =>
                    ws.ServiceId == request.ServiceId &&
                    ws.Service.IsActive) &&
                w.WorkerLocation != null)
            .ToListAsync();

        var matchingWorkers = candidates
            .Select(worker =>
            {
                var location = worker.WorkerLocation!;

                var distanceKm = CalculateDistanceKm(
                    (double)request.Latitude,
                    (double)request.Longitude,
                    (double)location.Latitude,
                    (double)location.Longitude);

                return new MatchingWorkerResponse
                {
                    WorkerId = worker.WorkerId,
                    FullName = worker.Worker.FullName,
                    AvatarUrl = worker.Worker.AvatarUrl,
                    AverageRating = worker.AverageRating,
                    TotalJobs = worker.TotalJobs,
                    IsOnline = worker.IsOnline,
                    IsAvailable = worker.IsAvailable,
                    DistanceKm = Math.Round(distanceKm, 2),
                    Latitude = (double)location.Latitude,
                    Longitude = (double)location.Longitude
                };
            })
            .Where(x => x.DistanceKm <= maxRadiusKm)
            .OrderBy(x => x.DistanceKm)
            .ThenByDescending(x => x.AverageRating)
            .ToList();

        return Ok(new
        {
            requestId = request.RequestId,
            serviceId = request.ServiceId,
            status = request.Status,
            searchRadiusKm = maxRadiusKm,
            total = matchingWorkers.Count,
            workers = matchingWorkers
        });
    }


    [Authorize(Roles = "WORKER")]
    [HttpGet("worker-available")]
    public async Task<IActionResult> GetAvailableRequestsForWorker()
    {
        if (!TryGetUserId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var worker = await _context.WorkerProfiles
            .AsNoTracking()
            .Include(w => w.WorkerServices)
            .Include(w => w.WorkerLocation)
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        if (!worker.IsOnline || !worker.IsAvailable)
        {
            return Ok(new
            {
                total = 0,
                requests = Array.Empty<object>()
            });
        }

        if (worker.WorkerLocation == null)
        {
            return Ok(new
            {
                total = 0,
                requests = Array.Empty<object>()
            });
        }

        var workerServiceIds = worker.WorkerServices
            .Select(ws => ws.ServiceId)
            .ToList();

        var requests = await _context.ServiceRequests
            .AsNoTracking()
            .Include(r => r.Customer)
                .ThenInclude(c => c.Customer)
            .Include(r => r.Service)
            .Where(r =>
                r.Status == "SEARCHING" &&
                r.WorkerId == null &&
                workerServiceIds.Contains(r.ServiceId))
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        const double maxRadiusKm = 10.0;

        var result = requests
            .Select(r =>
            {
                var distanceKm = CalculateDistanceKm(
                    (double)worker.WorkerLocation.Latitude,
                    (double)worker.WorkerLocation.Longitude,
                    (double)r.Latitude,
                    (double)r.Longitude);

                return new
                {
                    r.RequestId,
                    r.CustomerId,
                    CustomerName = r.Customer.Customer.FullName,
                    r.ServiceId,
                    ServiceName = r.Service.ServiceName,
                    r.Description,
                    r.Status,
                    r.Latitude,
                    r.Longitude,
                    r.Address,
                    r.ScheduledAt,
                    r.CreatedAt,
                    DistanceKm = Math.Round(distanceKm, 2)
                };
            })
            .Where(r => r.DistanceKm <= maxRadiusKm)
            .OrderBy(r => r.DistanceKm)
            .ThenBy(r => r.CreatedAt)
            .ToList();

        return Ok(new
        {
            total = result.Count,
            searchRadiusKm = maxRadiusKm,
            requests = result
        });
    }


    [Authorize(Roles = "WORKER")]
    [HttpPut("{id:long}/accept")]
    public async Task<IActionResult> AcceptRequest(long id)
    {
        if (!TryGetUserId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var worker = await _context.WorkerProfiles
            .Include(w => w.WorkerServices)
            .Include(w => w.WorkerLocation)
            .Include(w => w.Worker)
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy hồ sơ thợ."
            });
        }

        var workerUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == workerId);

        if (workerUser == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy tài khoản thợ."
            });
        }

        if (!worker.IsOnline || !worker.IsAvailable)
        {
            return BadRequest(new
            {
                message = "Thợ hiện không thể nhận việc."
            });
        }

        if (worker.WorkerLocation == null)
        {
            return BadRequest(new
            {
                message = "Thợ chưa cập nhật vị trí."
            });
        }

        var request = await _context.ServiceRequests
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        if (request.Status != "SEARCHING")
        {
            return Conflict(new
            {
                message = $"Yêu cầu hiện đang ở trạng thái {request.Status}."
            });
        }

        if (request.WorkerId != null)
        {
            return Conflict(new
            {
                message = "Yêu cầu đã được thợ khác nhận."
            });
        }

        var canProvideService = worker.WorkerServices
            .Any(ws => ws.ServiceId == request.ServiceId);

        if (!canProvideService)
        {
            return Forbid();
        }

        var distanceKm = CalculateDistanceKm(
            (double)request.Latitude,
            (double)request.Longitude,
            (double)worker.WorkerLocation.Latitude,
            (double)worker.WorkerLocation.Longitude);

        const double maxRadiusKm = 10.0;

        if (distanceKm > maxRadiusKm)
        {
            return BadRequest(new
            {
                message = "Yêu cầu nằm ngoài bán kính phục vụ 10 km.",
                distanceKm = Math.Round(distanceKm, 2)
            });
        }

        request.WorkerId = workerId;
        request.Status = "ACCEPTED";
        request.AcceptedAt = DateTime.UtcNow;

        worker.IsAvailable = false;

        
        var history = new RequestStatusHistory
        {
            RequestId = request.RequestId,
            Status = "ACCEPTED",
            Note = $"Worker {workerId} đã nhận yêu cầu.",
            ChangedBy = workerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
        request.CustomerId,
        request.RequestId,
        "Thợ đã nhận yêu cầu",
        $"Thợ {worker.Worker.FullName} đã nhận yêu cầu sửa chữa của bạn.",
        "REQUEST_ACCEPTED");

        return Ok(new
        {
            success = true,
            message = "Nhận yêu cầu thành công.",
            requestId = request.RequestId,
            workerId = workerId,
            status = request.Status,
            distanceKm = Math.Round(distanceKm, 2),
            acceptedAt = request.AcceptedAt
        });
    }


    [Authorize(Roles = "WORKER")]
    [HttpPut("{id:long}/reject")]
    public async Task<IActionResult> RejectRequest(
        long id,
        [FromBody] RejectServiceRequestRequest requestDto)
    {
        if (!TryGetUserId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var request = await _context.ServiceRequests
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        if (request.Status != "SEARCHING")
        {
            return Conflict(new
            {
                message = $"Yêu cầu hiện đang ở trạng thái {request.Status}."
            });
        }

        var workerCanHandle = await _context.WorkerServices
            .AnyAsync(ws =>
                ws.WorkerId == workerId &&
                ws.ServiceId == request.ServiceId);

        if (!workerCanHandle)
        {
            return Forbid();
        }

        var note = string.IsNullOrWhiteSpace(requestDto.Reason)
            ? $"Worker {workerId} từ chối yêu cầu."
            : $"Worker {workerId} từ chối: {requestDto.Reason.Trim()}";

        var history = new RequestStatusHistory
        {
            RequestId = request.RequestId,
            Status = "SEARCHING",
            Note = note,
            ChangedBy = workerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đã từ chối yêu cầu.",
            requestId = request.RequestId,
            status = request.Status
        });
    }


    [Authorize(Roles = "WORKER")]
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] UpdateRequestStatusRequest requestDto)
    {
        if (!TryGetUserId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var newStatus = requestDto.Status
            .Trim()
            .ToUpperInvariant();

        var allowedStatuses = new[]
        {
            "ON_THE_WAY",
            "ARRIVED",
            "IN_PROGRESS",
            "COMPLETED"
        };

        if (!allowedStatuses.Contains(newStatus))
        {
            return BadRequest(new
            {
                message = "Trạng thái không hợp lệ."
            });
        }

        var request = await _context.ServiceRequests
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        if (request.WorkerId != workerId)
        {
            return Forbid();
        }

        var validTransition = request.Status switch
        {
            "ACCEPTED" when newStatus == "ON_THE_WAY" => true,

            "ON_THE_WAY" when newStatus == "ARRIVED" => true,

            "ARRIVED" when newStatus == "IN_PROGRESS" => true,

            "IN_PROGRESS" when newStatus == "COMPLETED" => true,

            _ => false
        };

        if (!validTransition)
        {
            return Conflict(new
            {
                message =
                    $"Không thể chuyển trạng thái từ {request.Status} sang {newStatus}."
            });
        }


        var statusMessages = new Dictionary<string, (string Title, string Message)>
        {
            ["ON_THE_WAY"] = (
                "Thợ đang đến",
                "Thợ sửa chữa đang di chuyển đến vị trí của bạn."
            ),

            ["ARRIVED"] = (
                "Thợ đã đến",
                "Thợ sửa chữa đã đến vị trí của bạn."
            ),

            ["IN_PROGRESS"] = (
                "Đang sửa chữa",
                "Thợ đã bắt đầu thực hiện công việc."
            ),

            ["COMPLETED"] = (
                "Đã hoàn thành",
                "Yêu cầu sửa chữa của bạn đã được hoàn thành."
            )
        };

        request.Status = newStatus;

        if (newStatus == "COMPLETED")
        {
            request.CompletedAt = DateTime.UtcNow;
        }

        var history = new RequestStatusHistory
        {
            RequestId = request.RequestId,
            Status = newStatus,
            Note = string.IsNullOrWhiteSpace(requestDto.Note)
                ? null
                : requestDto.Note.Trim(),
            ChangedBy = workerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestStatusHistories.Add(history);

        if (newStatus == "COMPLETED")
        {
            var worker = await _context.WorkerProfiles
                .FirstOrDefaultAsync(w => w.WorkerId == workerId);

            if (worker != null)
            {
                worker.IsAvailable = true;
                worker.TotalJobs += 1;
            }
        }


        await _context.SaveChangesAsync();

        if (statusMessages.TryGetValue(
            newStatus,
            out var notificationInfo))
        {
            await _notificationService.SendNotificationAsync(
                request.CustomerId,
                request.RequestId,
                notificationInfo.Title,
                notificationInfo.Message,
                $"REQUEST_{newStatus}");
        }

        return Ok(new
        {
            success = true,
            requestId = request.RequestId,
            workerId,
            status = request.Status,
            completedAt = request.CompletedAt
        });
    }

    private static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(latitude2 - latitude1);
        var dLon = DegreesToRadians(longitude2 - longitude1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(latitude1)) *
            Math.Cos(DegreesToRadians(latitude2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    }