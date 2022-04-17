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

    public PlotsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PlotCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.IpAddress))
            return BadRequest();

        _dbContext.Plots.Add(new PlotInfo
        {
            Name = request.Name, IpAddress = request.IpAddress,
            CreatedOn = DateTime.Now
        });
        var count = await _dbContext.SaveChangesAsync();

        return count > 0 ? Ok() : new StatusCodeResult(500);
    }
}