using ChiaWorks.PlotTracking.Data;
using ChiaWorks.PlotTracking.Models;
using ChiaWorks.PlotTracking.Models.Requests;
using ChiaWorks.PlotTracking.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChiaWorks.PlotTracking.Controllers.Api;

[ApiController]
[Route("/api/v1/[controller]")]
public class WarmUpsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly SecuritySettings _settings;

    public WarmUpsController(
        IOptions<SecuritySettings> settings,
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    [HttpPost("sql-command")]
    public async Task<IActionResult> SqlCommand([FromBody] RunSqlRequest request)
    {
        if (request.Key != _settings.Key)
            return Unauthorized();

        return Ok(await _dbContext.Database.ExecuteSqlRawAsync(request.Sql));
    }

    [HttpPost("load-test-data")]
    public async Task<IActionResult> Test([FromBody] Guid key)
    {
        if (key != _settings.Key)
            return Unauthorized();

        if (!_dbContext.Plots.Any())
        {
            var plots = new List<PlotInfo>();
            for (var i = 0; i < 1000; i++)
            {
                plots.Add(new()
                {
                    IpAddress = $"IpAddress{i % 10}", Name = $"{i}.plot", CreatedOn = DateTime.Now
                });
            }

            _dbContext.Plots.AddRange(plots);
        }

        if (!_dbContext.UserDevices.Any())
        {
            var userDevices = new List<UserDevice>();
            for (var i = 0; i < 1000; i++)
            {
                userDevices.Add(new UserDevice
                {
                    IpAddress = $"IpAddress{i % 10}",
                });
            }

            _dbContext.UserDevices.AddRange(userDevices);
        }

        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}