namespace ChiaWorks.PlotTracking.Models;

public class UserDevice : EntityBase
{
    public Guid UserId { get; set; }
    public string IpAddress { get; set; }
}