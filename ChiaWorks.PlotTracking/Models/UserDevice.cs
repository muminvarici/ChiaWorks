namespace ChiaWorks.PlotTracking.Models;

public class UserDevice : EntityBase
{
    public Guid UserId { get; set; }
    public string IpAddress { get; set; }

    public string GetIpAddress(bool isAdmin)
    {
        if (isAdmin)
            return IpAddress;
        if (!IpAddress.Contains('.')) return "************";
        return $"***.{IpAddress.Remove(IpAddress.LastIndexOf(".", StringComparison.Ordinal))}";
    }
}