using System.Net;

namespace ChiaWorks.PlotTracking.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public HttpStatusCode Code { get; set; }
    public string Message { get; set; }
}