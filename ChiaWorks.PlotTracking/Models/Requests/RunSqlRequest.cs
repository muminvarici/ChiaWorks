namespace ChiaWorks.PlotTracking.Models.Requests;

public class RunSqlRequest
{
    public Guid Key { get; set; }
    public string Sql { get; set; }
}