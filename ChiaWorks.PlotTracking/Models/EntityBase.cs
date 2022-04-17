using System.ComponentModel.DataAnnotations;

namespace ChiaWorks.PlotTracking.Models;

public class EntityBase
{
    public int Id { get; set; }

    [Display(Name = "Created On")]
    public DateTime CreatedOn { get; set; }//todo move to base
}