using ChiaWorks.PlotTracking.Data;
using ChiaWorks.PlotTracking.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ChiaWorks.PlotTracking.Services;

public class UserService
{
    public const string AdminRoleName = "Admin";

    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SecuritySettings _settings;

    public UserService(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<SecuritySettings> settings )
    {
        _settings = settings.Value;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> Warmup()
    {
        if (!await _roleManager.RoleExistsAsync(AdminRoleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole
            {
                Name = AdminRoleName
            });
            if (roleResult is not { Succeeded: true }) return false;
        }

        var user = await CreateUser(_settings.UserName, _settings.Password);

        if (!await _userManager.IsInRoleAsync(user, AdminRoleName))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, AdminRoleName);
            if (addRoleResult is not { Succeeded: true }) return false;
        }

        return true;
    }

    private async Task<IdentityUser> CreateUser(string userName, string password)
    {
        var user = await _userManager.FindByEmailAsync(userName);

        if (user != null) return user;

        var userResult = await _userManager.CreateAsync(new IdentityUser
        {
            Email = userName, UserName = userName, EmailConfirmed = true,
        }, password);

        if (userResult is not { Succeeded: true }) throw new Exception("User create failed");

        user = _userManager.Users.First(w => w.Email == userName);

        return user;
    }

}