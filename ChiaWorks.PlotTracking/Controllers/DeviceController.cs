using System.Linq;
using System.Security.Authentication;
using ChiaWorks.PlotTracking.Controllers.Api;
using ChiaWorks.PlotTracking.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChiaWorks.PlotTracking.Controllers;

public class DeviceController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public DeviceController(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        if (User.Identity is not { IsAuthenticated: true }) return Redirect("/Identity/Account/Login");
        var user = await _userManager.FindByEmailAsync(User.Identity.Name);
        if (user == null) throw new AuthenticationException();

        var isAdmin = await _userManager.IsInRoleAsync(user, WarmUpsController.AdminRoleName);

        var userDevices = _dbContext.UserDevices.AsQueryable()
            .Where(w => isAdmin || w.UserId.ToString() == user.Id)
            .ToList();

        return View(userDevices);
    }
}