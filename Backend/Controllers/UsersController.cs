using System.Security.Claims;
using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;

    public UsersController(EmergencyHomeServiceDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy người dùng."
            });
        }

        return Ok(new UserProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateUserProfileRequest request)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Token không hợp lệ."
            });
        }

        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Phone))
        {
            return BadRequest(new
            {
                message = "Họ tên và số điện thoại không được để trống."
            });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy người dùng."
            });
        }

        var phoneExists = await _context.Users
            .AnyAsync(x =>
                x.Phone == request.Phone.Trim() &&
                x.UserId != userId);

        if (phoneExists)
        {
            return Conflict(new
            {
                message = "Số điện thoại đã được sử dụng."
            });
        }

        user.FullName = request.FullName.Trim();
        user.Phone = request.Phone.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
            ? null
            : request.AvatarUrl.Trim();

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new UserProfileResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive
        });
    }
}