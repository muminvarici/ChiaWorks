using ChiaWorks.PlotTracking.Data;
using ChiaWorks.PlotTracking.Models;
using ChiaWorks.PlotTracking.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChiaWorks.PlotTracking.Controllers.Api;

[ApiController]
[Route("/api/v1/[controller]")]
public class WarmUpsController : ControllerBase
{
    public const string AdminRoleName = "Admin";

    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly SecuritySettings _settings;

    public WarmUpsController(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<SecuritySettings> settings,
        ApplicationDbContext dbContext)
    {
        _settings = settings.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Hello world!");
    }

    [HttpPost("all")]
    public async Task<IActionResult> InitializeUsers([FromBody] Guid key)
    {
        if (key != _settings.Key)
            return Unauthorized();


        if (!await _roleManager.RoleExistsAsync(AdminRoleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole
            {
                Name = AdminRoleName
            });
            if (roleResult is not { Succeeded: true }) return BadRequest(roleResult?.Errors);
        }

        var user = await CreateUser(_settings.UserName, _settings.Password);

        if (!await _userManager.IsInRoleAsync(user, AdminRoleName))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, AdminRoleName);
            if (addRoleResult is not { Succeeded: true }) return BadRequest(addRoleResult?.Errors);
        }

        return Ok();
    }

    private async Task<IdentityUser> CreateUser(string userName, string password)
    {
        var user = await _userManager.FindByEmailAsync(userName);

        if (user == null)
        {
            var userResult = await _userManager.CreateAsync(new IdentityUser
            {
                Email = userName, UserName = userName, EmailConfirmed = true,
            }, password);

            if (userResult is not { Succeeded: true }) throw new Exception("User create failed");

            user = _userManager.Users.First(w => w.Email == userName);
        }

        return user;
    }


    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] Guid key)
    {
        if (key != _settings.Key)
            return Unauthorized();

        var testUser = await CreateUser("test@test.com", "Test12.");

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
                userDevices.Add(new()
                {
                    IpAddress = $"IpAddress{i % 10}", UserId = Guid.Parse(testUser.Id),
                });
            }

            _dbContext.UserDevices.AddRange(userDevices);
        }

        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}