using System.Security.Authentication;
using ChiaWorks.PlotTracking.Controllers.Api;
using ChiaWorks.PlotTracking.Data;
using ChiaWorks.PlotTracking.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChiaWorks.PlotTracking.Controllers;

public class PlotController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    private readonly UserManager<IdentityUser> _userManager;

    public PlotController(UserManager<IdentityUser> userManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }


    // GET
    public async Task<IActionResult> Index()
    {
        if (User.Identity is not { IsAuthenticated: true }) return Redirect("/Identity/Account/Login");
        var user = await _userManager.FindByEmailAsync(User.Identity.Name);
        if (user == null) throw new AuthenticationException();

        var isAdmin = await _userManager.IsInRoleAsync(user, WarmUpsController.AdminRoleName);

        List<string> userDevices = null;
        if (!isAdmin)
        {
            userDevices = _dbContext.UserDevices.AsQueryable()
                .Where(w => w.UserId.ToString() == user.Id)
                .Select(w => w.IpAddress)
                .ToList();
        }

        var plots = await _dbContext.Plots.AsQueryable()
            .Where(w => isAdmin || userDevices.Contains(w.IpAddress))
            .OrderByDescending(w => w.CreatedOn)
            .ToListAsync();

        return View(plots);
    }
}