using EmergencyHomeService.API.Models;

namespace EmergencyHomeService.API.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}