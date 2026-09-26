using System.Security.Claims;
using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.Rating;
using EmergencyHomeService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;

    public RatingsController(
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

    [Authorize(Roles = "CUSTOMER")]
    [HttpPost("{requestId:long}")]
    public async Task<IActionResult> CreateRating(
        long requestId,
        [FromBody] CreateRatingRequest requestDto)
    {
        if (!TryGetUserId(out var customerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        if (requestDto.Score < 1 || requestDto.Score > 5)
        {
            return BadRequest(new
            {
                message = "Điểm đánh giá phải từ 1 đến 5."
            });
        }

        var request = await _context.ServiceRequests
            .FirstOrDefaultAsync(r =>
                r.RequestId == requestId &&
                r.CustomerId == customerId);

        if (request == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy yêu cầu."
            });
        }

        if (request.Status != "COMPLETED")
        {
            return BadRequest(new
            {
                message = "Chỉ có thể đánh giá yêu cầu đã hoàn thành."
            });
        }

        if (request.WorkerId == null)
        {
            return BadRequest(new
            {
                message = "Yêu cầu chưa được gán cho thợ."
            });
        }

        var existingRating = await _context.Ratings
            .AnyAsync(r => r.RequestId == requestId);

        if (existingRating)
        {
            return Conflict(new
            {
                message = "Yêu cầu này đã được đánh giá."
            });
        }

        var workerId = request.WorkerId.Value;

        var rating = new Rating
        {
            RequestId = requestId,
            CustomerId = customerId,
            WorkerId = workerId,
            Score = requestDto.Score,
            Comment = string.IsNullOrWhiteSpace(requestDto.Comment)
                ? null
                : requestDto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);

        await _context.SaveChangesAsync();

        // Cập nhật điểm trung bình của Worker
        var worker = await _context.WorkerProfiles
            .FirstOrDefaultAsync(w => w.WorkerId == workerId);

        if (worker != null)
        {
            var averageRating = await _context.Ratings
                .Where(r => r.WorkerId == workerId)
                .AverageAsync(r => (double)r.Score);

            worker.AverageRating =
                Math.Round((decimal)averageRating, 2);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đánh giá thành công.",
            ratingId = rating.RatingId,
            requestId = rating.RequestId,
            workerId = rating.WorkerId,
            score = rating.Score,
            comment = rating.Comment,
            averageRating = worker?.AverageRating
        });
    }

    [Authorize(Roles = "CUSTOMER")]
    [HttpGet("request/{requestId:long}")]
    public async Task<IActionResult> GetMyRating(long requestId)
    {
        if (!TryGetUserId(out var customerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var rating = await _context.Ratings
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.RequestId == requestId &&
                r.CustomerId == customerId);

        if (rating == null)
        {
            return NotFound(new
            {
                message = "Chưa có đánh giá."
            });
        }

        return Ok(rating);
    }

    [Authorize(Roles = "WORKER")]
    [HttpGet("worker")]
    public async Task<IActionResult> GetMyRatings()
    {
        if (!TryGetUserId(out var workerId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var ratings = await _context.Ratings
            .AsNoTracking()
            .Where(r => r.WorkerId == workerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.RatingId,
                r.RequestId,
                r.CustomerId,
                r.Score,
                r.Comment,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(ratings);
    }
}