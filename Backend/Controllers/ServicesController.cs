using EmergencyHomeService.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly EmergencyHomeServiceDbContext _context;

    public ServicesController(EmergencyHomeServiceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .Select(s => new
            {
                s.ServiceId,
                s.ServiceName,
                s.Description,
                s.IconUrl
            })
            .ToListAsync();

        return Ok(services);
    }
}