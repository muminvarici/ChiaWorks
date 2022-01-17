using System;
using ChiaWorks.FutureBot.Extensions;
using ChiaWorks.FutureBot.Requests;
using ChiaWorks.FutureBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChiaWorks.FutureBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HooksController : ControllerBase
    {
        private readonly IFutureService _futureService;
        private readonly ILogger<HooksController> _logger;

        public HooksController(IFutureService futureService,
            ILogger<HooksController> logger) //todo use mediator to handle requests
        {
            _futureService = futureService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] NewCommonRequest request)
        {
            switch (request.Direction)
            {
                case FutureDirection.Buy:
                    return Buy(new NewBuyRequest { Coin = request.Coin, Price = request.Price });
                case FutureDirection.Sell:
                    return Sell(new NewSellRequest { Coin = request.Coin, Price = request.Price });
                default:
                    throw new NotImplementedException(request.Serialize());
            }
        }

        [Route("test")]
        [HttpPost]
        public IActionResult Test([FromBody] NewCommonRequest request)
        {
            _logger.LogInformation(request.Serialize());
            return Ok();
        }


        [Route("buy")]
        [HttpPost]
        public IActionResult Buy([FromBody] NewBuyRequest request)
        {
            _futureService.Buy(request.Coin, request.Price);
            return Ok();
        }

        [Route("sell")]
        [HttpPost]
        public IActionResult Sell([FromBody] NewSellRequest request)
        {
            _futureService.Sell(request.Coin, request.Price);
            return Ok();
        }
    }
}