using System.ComponentModel.DataAnnotations;

namespace ChiaWorks.PlotTracking.Models;

public class PlotInfo : EntityBase
{
    public string Name { get; set; }

    [Display(Name = "Created On")]
    public DateTime CreatedOn { get; set; }//todo move to base

    [Display(Name = "Ip Address")]
    public string? IpAddress { get; set; }
}