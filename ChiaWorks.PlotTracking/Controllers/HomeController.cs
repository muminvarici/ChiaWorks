using System.Diagnostics;
using ChiaWorks.PlotTracking.Data;
using Microsoft.AspNetCore.Mvc;
using ChiaWorks.PlotTracking.Models;
using Microsoft.EntityFrameworkCore;

namespace ChiaWorks.PlotTracking.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public HomeController(
        ApplicationDbContext dbContext
    )
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var itemCount = 14;
        var day = DateTime.Now.Date;
        var model = new HomeModel
        {
            Labels = GetDaysOfWeek(day, itemCount), Data = await GetData(day, itemCount)
        };

        return View(model);
    }

    private async Task<int[]> GetData(DateTime day, int itemCount)
    {
        var data = await _dbContext.Plots
            .AsQueryable()
            .Where(w => w.CreatedOn.Date > day.AddDays(-itemCount))
            .GroupBy(w => w.CreatedOn.Date)
            .OrderBy(w => w.Key)
            .Select(w => w.Count())
            .ToArrayAsync();

        return new int [itemCount - data.Length].Concat(data).ToArray();
    }

    private string[] GetDaysOfWeek(DateTime day, int itemCount)
    {
        var result = new string[itemCount];

        for (var i = 0; i < itemCount - 1; i++)
        {
            result[i] = day.AddDays(i).DayOfWeek.ToString();
        }

        result[itemCount - 1] = day.DayOfWeek.ToString();

        return result;
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}