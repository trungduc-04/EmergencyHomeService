using EmergencyHomeService.API.Data;
using EmergencyHomeService.API.DTOs.Auth;
using EmergencyHomeService.API.Models;
using EmergencyHomeService.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(
        EmergencyHomeServiceDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Vui lòng nhập đầy đủ thông tin."
            });
        }

        if (request.Password.Length < 8 ||
            !request.Password.Any(char.IsUpper) ||
            !request.Password.Any(char.IsLower) ||
            !request.Password.Any(char.IsDigit) ||
            !request.Password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return BadRequest(new
            {
                message =
                    "Mật khẩu phải có ít nhất 8 ký tự, " +
                    "bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt."
            });
        }

        var role = request.Role.Trim().ToUpperInvariant();

        if (role != "CUSTOMER" && role != "WORKER")
        {
            return BadRequest(new
            {
                message = "Role không hợp lệ."
            });
        }

        var normalizedEmail = request.Email.Trim();

        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == normalizedEmail);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Email đã tồn tại."
            });
        }

        var phoneExists = await _context.Users
            .AnyAsync(x => x.Phone == request.Phone.Trim());

        if (phoneExists)
        {
            return Conflict(new
            {
                message = "Số điện thoại đã tồn tại."
            });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone.Trim(),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        if (role == "CUSTOMER")
        {
            var profile = new CustomerProfile
            {
                CustomerId = user.UserId
            };

            _context.CustomerProfiles.Add(profile);
        }
        else
        {
            var profile = new WorkerProfile
            {
                WorkerId = user.UserId,
                ExperienceYears = 0,
                IsOnline = false,
                IsAvailable = true,
                AverageRating = 0,
                TotalJobs = 0,
                IsVerified = false
            };

            _context.WorkerProfiles.Add(profile);
        }

        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = user.Role,
            Token = token
        });
    }

    ////////////////////////////////////////////////////////////////////////////
    
///////////////////////////////////////////////////////////////////////////////

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrPhone) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Vui lòng nhập tài khoản và mật khẩu."
            });
        }

        var account = request.EmailOrPhone.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email == account ||
                x.Phone == account);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Tài khoản hoặc mật khẩu không đúng."
            });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message = "Tài khoản đã bị khóa."
            });
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Tài khoản hoặc mật khẩu không đúng."
            });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Role = user.Role,
            Token = token
        });
    }
}