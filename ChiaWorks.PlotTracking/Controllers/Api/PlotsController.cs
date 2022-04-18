using ChiaWorks.PlotTracking.Data;
using ChiaWorks.PlotTracking.Models;
using ChiaWorks.PlotTracking.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChiaWorks.PlotTracking.Controllers.Api;

[Route("/api/v1/[controller]")]
[ApiController]
public class PlotsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PlotsController> _logger;

    public PlotsController(
        ApplicationDbContext dbContext,
        ILogger<PlotsController> logger
    )
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PlotCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest();

        var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("{ip} sent {name}", ip, request.Name);

        await EnsureDeviceExistsAsync(ip);
        if (await _dbContext.Plots.AnyAsync(w => w.Name.Contains(request.Name) || request.Name.Contains(w.Name)))
            return Conflict();

        await _dbContext.Plots.AddAsync(new PlotInfo
        {
            Name = request.Name, IpAddress = ip, CreatedOn = DateTime.Now
        });
        var count = await _dbContext.SaveChangesAsync();

        return count > 0 ? Ok() : UnprocessableEntity();
    }


    [HttpPost("init")]
    public async Task<IActionResult> Post([FromBody] DeviceInitRequest request)
    {
        request.Files = request.Files.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();

        if (!request.Files.Any())
            return BadRequest();

        var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("{ip} sent {length} items", ip, request.Files.Length);

        await EnsureDeviceExistsAsync(ip);


        var plots = request.Files
            .Where(q => !_dbContext.Plots.Any(w => w.Name.Contains(q) || q.Contains(w.Name)))
            .Select(w => new PlotInfo
            {
                Name = w, IpAddress = ip, CreatedOn = DateTime.Now
            }).ToList();

        await _dbContext.Plots.AddRangeAsync(plots);
        var count = await _dbContext.SaveChangesAsync();

        return count > 0 ? Ok() : UnprocessableEntity();
    }

    private async Task EnsureDeviceExistsAsync(string? ip)
    {
        if (!await _dbContext.UserDevices.AnyAsync(w => w.IpAddress == ip))
        {
            await _dbContext.UserDevices.AddAsync(new()
            {
                IpAddress = ip, CreatedOn = DateTime.Now
            });
        }
    }
}