using ChiaWorks.PlotTracking.Data;
using ChiaWorks.PlotTracking.Models;
using ChiaWorks.PlotTracking.Models.Requests;
using Microsoft.AspNetCore.Mvc;

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
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.IpAddress))
            return BadRequest();

        var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("{ip} sent {name}", ip, request.Name);

        await _dbContext.Plots.AddAsync(new PlotInfo
        {
            Name = request.Name, IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(), CreatedOn = DateTime.Now
        });
        var count = await _dbContext.SaveChangesAsync();

        return count > 0 ? Ok() : new StatusCodeResult(500);
    }


    [HttpPost("init")]
    public async Task<IActionResult> Post([FromBody] DeviceInitRequest request)
    {
        request.Files = request.Files.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
        var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("{ip} sent {length} items", ip, request.Files.Length);

        if (request.Files.Any())
            return BadRequest();

        var plots = request.Files.Select(w => new PlotInfo
        {
            Name = w, IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(), CreatedOn = DateTime.Now
        }).ToList();

        await _dbContext.Plots.AddRangeAsync(plots);
        var count = await _dbContext.SaveChangesAsync();

        return count > 0 ? Ok() : new StatusCodeResult(500);
    }
}