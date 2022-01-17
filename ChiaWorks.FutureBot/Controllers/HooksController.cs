using ChiaWorks.FutureBot.Requests;
using ChiaWorks.FutureBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChiaWorks.FutureBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HooksController : ControllerBase
    {
        private readonly IFutureService _futureService;

        public HooksController(IFutureService futureService) //todo use mediator to handle requests
        {
            _futureService = futureService;
        }

        [Route("buy")]
        [HttpPost]
        public IActionResult Buy([FromBody] NewBuyRequest request)
        {
            var result = _futureService.Buy(request.Coin);
            return Ok(result);
        }

        [Route("sell")]
        [HttpPost]
        public IActionResult Sell([FromBody] NewSellRequest request)
        {
            var result = _futureService.Sell(request.Coin);
            return Ok(result);
        }

        [Route("test")]
        [HttpPost]
        public IActionResult Test([FromBody] NewSellRequest request)
        {
            _futureService.Test();
            return Ok();
        }
    }
}