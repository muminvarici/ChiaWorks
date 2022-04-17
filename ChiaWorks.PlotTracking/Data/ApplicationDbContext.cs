using ChiaWorks.PlotTracking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChiaWorks.PlotTracking.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public virtual DbSet<PlotInfo> Plots { get; set; }
    public virtual DbSet<UserDevice> UserDevices { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}