using System;
using ChiaWorks.FutureBot.Extensions;
using ChiaWorks.FutureBot.Requests;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Model;
using Microsoft.Extensions.Logging;

namespace ChiaWorks.FutureBot.Services
{
    public class GateFutureService : IFutureService
    {
        private readonly FuturesApi _futuresApi;
        private readonly ILogger<GateFutureService> _logger;
        private static FutureDirection latestDirection;

        public GateFutureService(FuturesApi futuresApi,
            ILogger<GateFutureService> logger)
        {
            _futuresApi = futuresApi;
            _logger = logger;
        }

        public void Test()
        {
            var settle = "usdt"; // string | Settle currency
            var contract = "BTC_USDT"; // string | Futures contract
            var status = "open"; // string | Only list the orders with this status
            var offset = 0; // int? | List offset, starting from 0 (optional)  (default to 0)
            var result = _futuresApi.ListFuturesOrders(settle, contract, status);

            var positions = _futuresApi.ListFuturesContracts(settle);
            var position = _futuresApi.GetPosition(settle, contract);
        }

        public void Buy(string coin, float price, long size)
        {
            if (latestDirection == FutureDirection.Buy)
            {
                _logger.LogInformation("no need to buy again");
                return;
            }

            const string settle = "usdt"; // string | Settle currency
            var contract = $"{coin.ToUpper()}_USDT"; // string | Futures contract
            var order = new FuturesOrder(contract,
                size: Math.Abs(size),
                text: "t-api",
                price: (price + price / 100).ToString()
            );

            var response = _futuresApi.CreateFuturesOrder(settle, order);
            _logger.LogInformation(response?.Serialize());
            latestDirection = FutureDirection.Buy;
        }

        public void Sell(string coin, float price, long size)
        {
            if (latestDirection == FutureDirection.Sell)
            {
                _logger.LogInformation("no need to sell again");
                return;
            }

            const string settle = "usdt"; // string | Settle currency
            var contract = $"{coin.ToUpper()}_USDT"; // string | Futures contract

            var order = new FuturesOrder(contract,
                size: -Math.Abs(size),
                text: "t-api",
                price: (price - price / 100).ToString()
            );

            var response = _futuresApi.CreateFuturesOrder(settle, order);
            _logger.LogInformation(response?.Serialize());
            latestDirection = FutureDirection.Sell;
        }
    }
}